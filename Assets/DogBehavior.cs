using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DogBehavior : MonoBehaviour
{
    public Animator _animator;
    public Transform player;
    public NavMeshAgent agent;
    public float patrolRadius = 10f;
    public float chaseDistance = 1.5f;

    public Transform backupPointsParent;
    public List<Transform> backupPoints = new List<Transform>();

    private bool biteFinished = true;
    private bool isChasing = false;
    private bool isPatrolling = true;

    private void Start()
    {
        foreach(Transform child in backupPointsParent)
        {
            backupPoints.Add(child);
        }

        StartCoroutine(Patrol());
    }

    private IEnumerator Patrol()
    {
        while (isPatrolling)
        {
            // Try to find a valid random patrol position within patrolRadius
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += transform.position;
            NavMeshHit hit;

            bool foundValidPosition = false;

            if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
            {
                // Check if the position is reachable by using NavMesh.Raycast
                if (!NavMesh.Raycast(transform.position, hit.position, out _, NavMesh.AllAreas))
                {
                    foundValidPosition = true;
                    agent.SetDestination(hit.position);
                }
            }

            // Only proceed if a valid position was found
            if (foundValidPosition)
            {
                _animator.SetBool("isWalking", true);
                _animator.SetBool("isIdle", false);
                _animator.SetBool("isRunning", false);

                // Wait until the dog arrives at the patrol position
                yield return new WaitUntil(() => Vector3.Distance(transform.position, hit.position) < 1f);
                _animator.SetBool("isWalking", false);
                _animator.SetBool("isIdle", true);
                _animator.SetBool("isRunning", false);
                yield return new WaitForSeconds(Random.Range(1f, 3f));
            }
            else
            {
                // Retry if no valid position was found
                yield return null;
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isChasing)
        {
            isChasing = true;
            isPatrolling = false;
            StopCoroutine(Patrol());
            StartCoroutine(ChasePlayer());
        }
    }

    private IEnumerator ChasePlayer()
    {
        while (isChasing)
        {
            agent.SetDestination(player.position);

            // Check the distance to the player
            if (Vector3.Distance(transform.position, player.position) < chaseDistance && biteFinished)
            {
                yield return new WaitUntil(() => player.parent.GetComponent<AgentManager>()._animator.GetCurrentAnimatorStateInfo(0).IsName("WalkForward"));
                yield return new WaitForSeconds(0.2f);
                biteFinished = false;
                _animator.SetTrigger("Bite");

                // Wait for bite animation to finish
                yield return new WaitUntil(() => biteFinished);

                // Switch to backup mode after biting
                StartCoroutine(GoToBackupPoint());
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator GoToBackupPoint()
    {
        // Find the furthest point in the backupPoints array from the player
        Transform furthestPoint = null;
        float maxDistance = 0f;

        foreach (Transform point in backupPoints)
        {
            float distance = Vector3.Distance(transform.position, point.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                furthestPoint = point;
            }
        }

        // Set the destination to the furthest backup point
        if (furthestPoint != null)
        {
            agent.SetDestination(furthestPoint.position);
        }

        // Keep checking the distance to the backup point
        yield return new WaitUntil(() => Vector3.Distance(transform.position, furthestPoint.position) < 1f);

        // Return to patrol state after reaching the backup point
        isChasing = false;
        isPatrolling = true;
        StartCoroutine(Patrol());
    }


    public void SwitchBite()
    {
        biteFinished = true;
    }

    public void HitPlayer()
    {
        player.parent.GetComponent<AgentManager>().GetHitByDog();
    }
}
