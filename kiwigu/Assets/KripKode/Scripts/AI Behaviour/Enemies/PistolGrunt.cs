using FMODUnity;
using System.Collections;
using UnityEngine;

public class PistolGrunt : EnemyBase
{
    [SerializeField] private StudioEventEmitter sfxEmitterAvailable;

    [SerializeField] MeshCollider coverDetectCollider;
    [SerializeField] Transform headBone;
    [SerializeField] bool idle;

    float hidingDistanceThreshold = 1f;
    float nextWanderTime;

    bool hiding;
    public bool doingShootPattern;
    public bool canShootPlayer = true;

    Vector3 hidingPos;

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
        float playerDistance = Vector3.Distance(transform.position, playerPosition);

        if (!hiding && !doingShootPattern && isPlayerVisible && canShootPlayer)
        {
            StartCoroutine(ShootPlayer());
        }

        if (canShootPlayer)
            return;

        if (gotHit)
        {
            CheckCrouch();

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
        }
        else
        {
            WanderRandomly();
        }

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            hiding = false;
        }
    }

    IEnumerator ShootPlayer()
    {
        doingShootPattern = true;

        agent.SetDestination(playerPosition);

        while (Vector3.Distance(transform.position, playerPosition) > enemyMovementVariables.AvoidPlayerDistance)
        {
            yield return null;
        }

        agent.ResetPath();

        yield return new WaitForSeconds(EnemyShoot());

        doingShootPattern = false;
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
                hiding = true;
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

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(oppositePoint);
            hidingPos = oppositePoint;
        }
    }

    private float EnemyShoot()
    {
        if (!isHoldingGun || !isPlayerVisible)
            return 0;

        HookTarget gun = transform.GetComponentInChildren<HookTarget>();
        if (gun == null)
        {
            isHoldingGun = false;
            return 0;
        }
        GunInfo info = gun.info;

        isShooting = true;

        float burst = info.burstSize;
        if (info.fullAuto) burst = info.autoRate;

        for (int j = 0; j < burst; j++)
        {
            isShooting = true;

            sfxEmitterAvailable.SetParameter("Charge", 0.5f);
            sfxEmitterAvailable.Play();

            for (int i = 0; i < info.projectiles; i++) Invoke(nameof(SpawnBullet), j * 1 / info.autoRate);
        }

        return burst * 1 / info.autoRate;
    }

    private void SpawnBullet()
    {
        HookTarget gun = transform.GetComponentInChildren<HookTarget>();
        GunInfo info = gun.info;

        GameObject bullet = Instantiate(info.bulletPrefab, gunObjectExitPoint.transform.position, gunObjectExitPoint.transform.rotation);

        Vector3 direction = gunObjectExitPoint.transform.forward;
        direction += SpreadDirection(info.spread, 3);

        bullet.transform.position = gunObjectExitPoint.transform.position;
        bullet.transform.rotation = Quaternion.LookRotation(direction.normalized);

        Bullet b = bullet.GetComponent<Bullet>();
        b.speed = info.bulletSpeed;
        b.gravity = info.bulletGravity;
        b.ignoreMask = ~LayerMask.GetMask("GunHand", "HookTarget", "Enemy");
        b.trackTarget = false;
        b.fromEnemy = true;
        b.bulletDamage = info.damage;
        b.charge = 0.5f;

        isShooting = false;
    }

    private Vector3 SpreadDirection(float spread, int rolls)
    {
        Vector3 offset = Vector3.zero;
        for (int i = 0; i < rolls; i++)
            offset += Random.onUnitSphere * spread;
        return offset / rolls;
    }
}
