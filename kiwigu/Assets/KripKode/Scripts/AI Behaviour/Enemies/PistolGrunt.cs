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

    Vector3 hidingPos;

    bool panicDone;

    public int timesShot;
    bool coolDown;

    protected override void Update()
    {
        if (isDead)
            return;

        base.Update();

        if (!idle && !enemyMainVariables.animator.GetComponent<HitVariable>().wasHit)
        {
            EnemyMovement();
        }

        if (doingShootPattern)
        {
            Camera.main.GetComponent<Music>().Violence = 1;
            RotateGunAndBodyTowardsPlayer();
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

        if (!hiding && !doingShootPattern && isPlayerVisible && !coolDown && isHoldingGun && (Vector3.Distance(transform.position, playerPosition) < enemyMovementVariables.EnemyAwareDistance))
        {
            StartCoroutine(ShootPlayer());
        }

        if (coolDown || !isHoldingGun)
            enemyMainVariables.animator.SetBool("shooting", false);

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

        if (agent.isOnNavMesh && agent.remainingDistance > agent.stoppingDistance)
        {
            hiding = false;
        }
    }

    IEnumerator ShootPlayer()
    {
        doingShootPattern = true;

        if (!isHoldingGun)
        {
            canFacePlayer = false;
            enemyMainVariables.animator.SetBool("shooting", false);
            doingShootPattern = false;
            yield break;
        }

        canFacePlayer = true;

        while (isPlayerVisible && !gotHit && timesShot < 3)
        {
            if (!isHoldingGun)
            {
                canFacePlayer = false;
                enemyMainVariables.animator.SetBool("shooting", false);
                doingShootPattern = false;
                yield break;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);

            if (distanceToPlayer > enemyMovementVariables.AvoidPlayerDistance)
            {
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.SetDestination(playerPosition);
                }
            }
            else
            {
                agent.ResetPath();
                enemyMainVariables.animator.SetBool("shooting", true);
            }

            yield return null;
        }

        timesShot = 0;

        StartCoroutine(CoolDown());
    }

    IEnumerator CoolDown()
    {
        canFacePlayer = false;
        enemyMainVariables.animator.SetBool("shooting", false);
        doingShootPattern = false;
        coolDown = true;
        yield return new WaitForSeconds(3);
        coolDown = false;
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

                if (!panicDone)
                    StartCoroutine(PanicCountdown());
            }
            else if (isPlayerVisible && isPlayerVisibleKnees)
            {
                enemyMainVariables.animator.SetBool("Crouching", false);
            }
        }
    }

    IEnumerator PanicCountdown()
    {
        panicDone = true;
        yield return new WaitForSeconds(15f);
        gotHit = false;
        panicDone = false;
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

    private void RotateGunAndBodyTowardsPlayer()
    {
        if (!canFacePlayer) return;

        if (agent.isOnNavMesh)
            RotateNavMeshAgentTowardsObj(playerPosition);

        RotateGunObjectExitPoint(playerPosition);
    }

    private void RotateNavMeshAgentTowardsObj(Vector3 objPos)
    {
        if (!canFacePlayer) return;

        agent.SetDestination(agent.transform.position);

        Quaternion targetRotation = Quaternion.LookRotation(objPos - agent.transform.position);

        agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, Time.deltaTime * 10);
    }


    private void RotateGunObjectExitPoint(Vector3 rotPos)
    {
        if (!canFacePlayer) return;

        gunObjectExitPoint = enemyMainVariables.GunObject.transform.GetChild(0).gameObject;

        Quaternion targetRotation = Quaternion.LookRotation(
            (rotPos + new Vector3(Random.Range(-enemyGunStats.GunInaccuracy, enemyGunStats.GunInaccuracy), 1.5f, Random.Range(-enemyGunStats.GunInaccuracy, enemyGunStats.GunInaccuracy))) - gunObjectExitPoint.transform.position
        );

        gunObjectExitPoint.transform.rotation = Quaternion.Slerp(
            gunObjectExitPoint.transform.rotation,
            targetRotation,
            Time.deltaTime * enemyGunStats.gunExitPointRotationSpeed
        );
    }

    public float EnemyShoot()
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
        timesShot++;

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
