using UnityEngine;

public class PistolGrunt : EnemyBase
{
    [SerializeField] MeshCollider coverDetectCollider;
    [SerializeField] Transform headBone;
    [SerializeField] bool idle;

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
        EnemyAnimations();
        HandleRegularEnemyMovement();
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
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
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
        agent.SetDestination(oppositePoint);
    }
}
