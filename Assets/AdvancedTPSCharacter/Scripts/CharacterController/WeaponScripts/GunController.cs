using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GunController : MonoBehaviour
{
    private CrossHairTarget target;
    public int fireRate = 25;
    public ActiveWeapon.WeaponSlot WeaponSlotType;
    [SerializeField] private ParticleSystem[] muzzleFlash;
    [SerializeField] private ParticleSystem hiteffect;
    [SerializeField] private Transform RaycastOrigin;
    [SerializeField] private Transform RaycastDestination;
    private TrailRenderer tracerEffect;
    public float Damage = 10f;
    Ray ray;
    RaycastHit hitInfo;
    public bool isFiring;
    public GameObject Magazine;
    public int ammoCount;
    public int ClipSize;

    [Header("Combat Weapons")]
    public bool AxeAttack;
    public bool KnifeAttacking;
    float accumalatedTime;
    public WeaponProceduralRecoil recoil { get; set; }
    [Range(0f, 1f)]
    public float ricochetChance;
    public float minRicochetAngle;
    [SerializeField] private GameObject hitArea;

    private int remainingEnemies;
    private int bulletsHitBoss;

    private void Awake()
    {
        recoil = GetComponent<WeaponProceduralRecoil>();
    }

    private void Start()
    {
        hitArea = GameObject.FindGameObjectWithTag("HitArea");
        target = FindObjectOfType<CrossHairTarget>();
        RaycastDestination = target.gameObject.transform;
        tracerEffect = FindObjectOfType<ActiveWeapon>().renderer;

        // Initialize variables
        remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        bulletsHitBoss = 0;
    }

    private void Update()
    {
        if (isFiring)
        {
            UpdateFiring(Time.deltaTime);
        }
    }

    public void StartFiring()
    {
        isFiring = true;
        recoil.Reset();
        accumalatedTime = 0.0f;
        FireBullet();
    }

    public void StopFiring()
    {
        isFiring = false;
    }

    public void UpdateFiring(float deltaTime)
    {
        accumalatedTime += deltaTime;
        float fireInterval = 1.0f / fireRate;
        while (accumalatedTime >= 0.0f)
        {
            FireBullet();
            accumalatedTime -= fireInterval;
        }
    }

    private void FireBullet()
    {
        if (ammoCount <= 0)
        {
            return;
        }
        ammoCount--;
        recoil.GenerateRecoil(WeaponSlotType.ToString());
        muzzleFlash[0].Emit(1);
        muzzleFlash[1].Emit(1);
        ray.origin = RaycastOrigin.position;
        ray.direction = RaycastDestination.position - RaycastOrigin.position;
        var tracer = Instantiate(tracerEffect, ray.origin, Quaternion.identity);
        tracer.AddPosition(ray.origin);
        if (Physics.Raycast(ray, out hitInfo, 1000f) && hitInfo.collider.tag != "Player")
        {
            hiteffect.transform.position = hitInfo.point;
            hiteffect.transform.forward = hitInfo.normal;
            hiteffect.Emit(1);
            tracer.transform.position = hitInfo.point;

            // Check if the hit collider belongs to the boss
            if (hitInfo.collider.CompareTag("Boss"))
            {
                // Increment the counter for bullets that hit the boss
                bulletsHitBoss++;

                // Check if the boss has been hit by 100 bullets
                if (bulletsHitBoss >= 100)
                {
                    // Perform actions for defeating the boss
                    DefeatBoss();
                }
             

                var rb2d = hitInfo.collider.GetComponent<Rigidbody>();
                if (hitInfo.collider.gameObject.GetComponent<Destructable>() != null)
                {
                    hitInfo.collider.gameObject.GetComponent<Destructable>().DestroyAndReplace();
                }
                if (rb2d)
                {
                    rb2d.AddForceAtPosition(ray.direction * 20, hitInfo.point, ForceMode.Impulse);
                }
            }
            if (hitInfo.collider.CompareTag("Enemy"))
            {
                // Destroy the enemy game object
                Destroy(hitInfo.collider.gameObject);

                // Decrement the remainingEnemies count
                remainingEnemies--;

                // Check for level completion
                if (remainingEnemies <= 0)
                {
                    // Display "Level Complete" (you can implement your own logic here)

                    // Load a new scene (replace "NextSceneName" with your actual scene name)
                    SceneManager.LoadScene("BOSS");
                }
            }
        }
    }

        private void DefeatBoss()
    {
        // Actions to perform when the boss is defeated
        // For example, load a new scene or display a victory message
        SceneManager.LoadScene("END");
    }
    private void DefeatWave()
    {
        // Actions to perform when the boss is defeated
        // For example, load a new scene or display a victory message
        SceneManager.LoadScene("BOSS");
    }

    public void StartAxeAttack(Animator Main, Animator Rig)
    {
        AxeAttack = true;
        Main.SetBool("AttackAxe", true);
        Rig.Play("Weapon_Constrain_Axe", 1);
        StartCoroutine(AxeStopSequence(Main, Rig));
        Invoke("CalculateAxeHit", 0.75f);
    }

    IEnumerator AxeStopSequence(Animator Main, Animator Rig)
    {
        yield return new WaitForSeconds(((2.267f / 1.25f) / Main.GetCurrentAnimatorStateInfo(0).speed - 0.3f) - 0.25f);
        Main.SetBool("AttackAxe", false);
        Rig.SetBool("ConstrainAxe", false);
        yield return new WaitForSeconds(0.25f);
        AxeAttack = false;
    }

    public void KnifeAttack(Animator Main, Animator Rig)
    {
        KnifeAttacking = true;
        Main.SetBool("AttackKnife", true);
        Rig.Play("Weapon_Constrain_Knife", 1);
        StartCoroutine(KnifeStopSequence(Main, Rig));
    }

    IEnumerator KnifeStopSequence(Animator Main, Animator Rig)
    {
        yield return new WaitForSeconds(((2.133f / 1.25f) / Main.GetCurrentAnimatorStateInfo(0).speed - 0.3f) - 0.25f);
        Main.SetBool("AttackKnife", false);
        Rig.SetBool("ConstrainKnife", false);
        yield return new WaitForSeconds(0.25f);
        KnifeAttacking = false;
    }

    private void CalculateAxeHit()
    {
        Collider[] hitcolliders = Physics.OverlapSphere(transform.position, 3f);
        foreach (var hitCollider in hitcolliders)
        {
            if (hitCollider.tag == "Tree")
            {
                hitCollider.gameObject.GetComponent<TreeDamageable>().Break();
            }
        }
    }
}