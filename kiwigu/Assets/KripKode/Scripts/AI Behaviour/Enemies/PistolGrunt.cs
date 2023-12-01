using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PistolGrunt : EnemyBase
{
    [SerializeField] MeshCollider coverDetectCollider;
    [SerializeField] Transform headBone;
    [SerializeField] bool idle;
    private bool sWander;

    protected override void Update()
    {
        base.Update();

        isPlayerVisible = CheckEyesVisibility();

        if (enemyMainVariables.hasKnees)
            isPlayerVisibleKnees = CheckKneesVisibility();

        if (!idle && !enemyMainVariables.animator.GetComponent<HitVariable>().wasHit)
        {
            EnemyMovement();
        }
    }

    protected override void HitBase()
    {
        base.HitBase();
    }

    private void EnemyMovement()
    {
        if (!isPlayerVisible && !sWander)
        {
            sWander = true;
            StartCoroutine(WanderRandomly());
        }

        EnemyAnimations();
        HandleRegularEnemyMovement();
    }

    private IEnumerator WanderRandomly()
    {
        while (!isPlayerVisible)
        {
            Vector3 randomDirection = Random.insideUnitSphere * enemyMovementVariables.WanderRadius;

            Vector3 destination = initialPosition + randomDirection;

            NavMeshHit hit;
            NavMesh.SamplePosition(destination, out hit, enemyMovementVariables.WanderRadius, 1);
            Vector3 finalPosition = hit.position;

            agent.SetDestination(finalPosition);

            yield return new WaitForSeconds(enemyMovementVariables.IdleTime);

            agent.isStopped = true;

            yield return new WaitForSeconds(1f);

            agent.isStopped = false;
        }

        sWander = false;
    }

    private void EnemyAnimations()
    {
        enemyMainVariables.animator.SetFloat("Movement", agent.velocity.magnitude);
    }

    private void HandleRegularEnemyMovement()
    {
        CheckCrouch();

        if (enemyMainVariables.hasKnees)
        {
            if (isPlayerVisibleKnees)
            {
                MoveAroundCover();
            }
            else
            {
                coverDetectCollider.enabled = false;
            }
        }
        else
        {
            if (isPlayerVisible)
            {
                MoveAroundCover();
            }
            else
            {
                coverDetectCollider.enabled = false;
            }
        }
    }

    private void CheckCrouch()
    {
        if (agent.isOnNavMesh)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (isPlayerVisible && !isPlayerVisibleKnees)
                {
                    enemyMainVariables.animator.SetBool("Crouching", true);
                }
            }
            else if (isPlayerVisible && isPlayerVisibleKnees)
            {
                enemyMainVariables.animator.SetBool("Crouching", false);
            }
        }
    }

    private void MoveAroundCover()
    {
        if (enemyMainVariables.animator.GetComponent<HitVariable>().wasHit) return;

        coverDetectCollider.enabled = true;

        GameObject collidedCover = coverDetectCollider.GetComponent<EnemyCoverDetection>().coverObject;

        if (collidedCover == null)
        {
            FindAndMoveToNearestCover();
        }
        else
        {
            MoveToOppositePoint(collidedCover.transform.position);
        }
    }

    private void FindAndMoveToNearestCover()
    {
        GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");
        GameObject nearestCover = null;
        float minDistance = float.MaxValue;

        foreach (GameObject coverObject in coverObjects)
        {
            float distance = Vector3.Distance(transform.position, coverObject.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCover = coverObject;
            }
        }

        if (nearestCover != null)
        {
            MoveToOppositePoint(nearestCover.transform.position);
        }
    }

    private void MoveToOppositePoint(Vector3 targetPosition)
    {
        Vector3 directionToPlayer = transform.position - playerPosition;
        Vector3 oppositePoint = targetPosition + directionToPlayer.normalized;

        if (agent.isOnNavMesh) agent.SetDestination(oppositePoint);
    }
}
