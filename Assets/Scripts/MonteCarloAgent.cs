using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonteCarloAgent : MonoBehaviour
{
    public List<Transform> items; // List of items to move towards
    public float decisionTime = 1f; // Time for MCTS to decide the best path

    private NavMeshAgent navMeshAgent;
    private Transform targetItem;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (items.Count > 0)
        {
            RunMCTS();
        }
    }

    void Update()
    {
        // Check if the agent has reached the target
        if (targetItem != null && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            targetItem = null; // Reached the item
        }
    }

    void RunMCTS()
    {
        float bestScore = float.MinValue;
        Transform bestItem = null;

        // Start Monte Carlo Tree Search to find the best item
        float startTime = Time.time;
        while (Time.time - startTime < decisionTime)
        {
            foreach (Transform item in items)
            {
                float score = Simulate(item);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestItem = item;
                }
            }
        }

        targetItem = bestItem;
        if (targetItem != null)
        {
            navMeshAgent.SetDestination(targetItem.position);
        }
    }

    float Simulate(Transform item)
    {
        // Simple simulation: use the negative distance as the score
        float distance = Vector3.Distance(transform.position, item.position);
        return -distance; // Closer items have a higher score
    }
}