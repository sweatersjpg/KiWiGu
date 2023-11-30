using UnityEngine;

public class PistolGrunt : EnemyBase
{
    [SerializeField] MeshCollider coverDetectCollider;
    [SerializeField] Transform headBone;
    [SerializeField] bool idle;

    protected override void Update()
    {
        isPlayerVisible = CheckPlayerVisibility();

        base.Update();

        EnemyMovement();
    }

    public void EnemyMovement()
    {
        if (idle)
            return;

        EnemyAnimations();
        HandleRegularEnemyMovement();
    }

    private void EnemyAnimations()
    {
        enemyMainVariables.animator.SetFloat("Movement", agent.velocity.magnitude);
    }

    private void HandleRegularEnemyMovement()
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

    private void MoveAroundCover()
    {
        coverDetectCollider.enabled = true;

        GameObject collidedCover = coverDetectCollider.GetComponent<EnemyCoverDetection>().coverObject;

        if(collidedCover == null)
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
                Vector3 directionToPlayer = transform.position - playerPosition;
            Vector3 oppositePoint = nearestCover.transform.position + directionToPlayer.normalized;

            agent.SetDestination(oppositePoint);
            }
        }
        else
        {
            Vector3 directionToPlayer = transform.position - playerPosition;
            Vector3 oppositePoint = collidedCover.transform.position + directionToPlayer.normalized;

            agent.SetDestination(oppositePoint);
        }
    }
}
