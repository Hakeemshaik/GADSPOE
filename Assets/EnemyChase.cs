using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    public Transform target;  // Reference to the character's transform.
    private NavMeshAgent agent; // Reference to the NavMeshAgent component.

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
}
