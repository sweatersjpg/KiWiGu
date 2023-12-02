using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PistolGrunt : EnemyBase
{
    [SerializeField] MeshCollider coverDetectCollider;
    [SerializeField] Transform headBone;
    [SerializeField] bool idle;

    float nextWanderTime;
    bool hiding;

    protected override void Update()
    {
        base.Update();

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
        EnemyAnimations();
        HandleRegularEnemyMovement();
    }

    private void EnemyAnimations()
    {
        enemyMainVariables.animator.SetFloat("Movement", agent.velocity.magnitude);

        if (enemyMainVariables.animator.GetFloat("Movement") > 0.1f)
        {
            enemyMainVariables.animator.SetBool("Crouching", false);
        }
    }

    private void HandleRegularEnemyMovement()
    {
        CheckCrouch();

        float playerDistance = Vector3.Distance(transform.position, playerPosition);

        if (playerDistance <= enemyMovementVariables.AvoidPlayerDistance && isPlayerVisible)
        {
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
        else if (!hiding && playerDistance > enemyMovementVariables.AvoidPlayerDistance)
        {
            WanderRandomly();
        }

        hiding = !(isPlayerVisible && isPlayerVisibleKnees);
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

    private void WanderRandomly()
    {
        if (Time.time > nextWanderTime)
        {
            Vector3 randomPoint = initialPosition + Random.insideUnitSphere * enemyMovementVariables.WanderRadius;
            randomPoint.y = transform.position.y;

            if (agent.isOnNavMesh) agent.SetDestination(randomPoint);

            nextWanderTime = Time.time + enemyMovementVariables.IdleTime;
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
