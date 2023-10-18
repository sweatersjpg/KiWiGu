using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EnemyBehaviour : EnemyBase
{
    [SerializeField] private float exitPointRotationSpeed = 180f;
    [SerializeField] private float verticalOffset = 0.75f;
    [SerializeField] private float movementDuration = 2f;

    private GameObject gunObjectExitPoint;
    private bool isDroneStopped;
    private bool isShootingPatternActive;

    [HideInInspector] public bool CanShoot;

    // Drone Pattern Variables
    private float droneMoveTime;
    private float initialYPosition;
    private Transform bodyMeshDrone;
    private Vector3 targetYPos;
    private float droneHeightOffset = 0.25f;
    private bool droneSwitch;
    private float lastExecutionTime = 0f;
    private float interval;
    private Vector3 randomDestination;

    bool canFacePlayer = true;

    protected override void Start()
    {
        if (hitBoxScript.CheckIfHitboxScript)
            return;

        base.Start();

        if (enemyTypeVariables.DefenseDrone || enemyTypeVariables.OffenseDrone)
        {
            interval = Random.Range(0.5f, 1.0f);

            initialYPosition = agent.height;

            bodyMeshDrone = enemyMainVariables.BodyMesh.transform;
            bodyMeshDrone.localPosition = new Vector3(0, initialYPosition, 0);
            targetYPos = bodyMeshDrone.localPosition;
        }
    }

    public void EnemyMovement()
    {
        if (enemyTypeVariables.DefenseDrone && detectedEnemy)
        {
            if (enemyMainVariables.GunObject)
                RotateGunObjectExitPoint(playerPosition);

            if (!isDroneStopped)
            {
                agent.ResetPath();
                StopAllCoroutines();
                isDroneStopped = true;
            }

            if (!isShootingPatternActive)
                StartCoroutine(DefenseDronePattern(enemyPosition));

            RotateBodyMeshTowardsObj(playerPosition);
        }
        else if (enemyTypeVariables.OffenseDrone && detectedPlayer)
        {
            if (enemyMainVariables.GunObject)
                RotateGunObjectExitPoint(playerPosition);

            RotateBodyMeshTowardsObj(playerPosition);

            if (!isDroneStopped)
            {
                agent.ResetPath();
                StopAllCoroutines();
                isDroneStopped = true;
            }

            if (!isShootingPatternActive)
                StartCoroutine(OffenseDronePattern(Random.Range(0, 2), playerPosition));
        }
        else if (enemyTypeVariables.Small || enemyTypeVariables.Medium)
        {
            if (Vector3.Distance(transform.position, playerPosition) <= enemyMovementVariables.AvoidPlayerDistance)
            {
                if (enemyMainVariables.GunObject)
                    RotateGunObjectExitPoint(playerPosition);

                Vector3 direction = playerPosition - transform.position;
                direction.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), enemyMovementVariables.RotationSpeed * Time.deltaTime);
            }
            else
            {
                if (enemyMainVariables.GunObject)
                    RotateGunObjectExitPoint(playerPosition);

                Vector3 offset = (transform.position - playerPosition).normalized * enemyMovementVariables.AvoidPlayerDistance;
                agent.SetDestination(playerPosition + offset);
            }
        }
    }

    private void RotateBodyMeshTowardsObj(Vector3 objPos)
    {
        if (!canFacePlayer) return;

        Quaternion rRot = Quaternion.LookRotation(objPos - bodyMeshDrone.position);
        bodyMeshDrone.rotation = Quaternion.Slerp(bodyMeshDrone.rotation, rRot, Time.deltaTime * 10);
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
            Time.deltaTime * exitPointRotationSpeed
        );
    }

    private IEnumerator OffenseDronePattern(int pattern, Vector3 thePlayerPosition)
    {
        isShootingPatternActive = true;

        int iPattern = (pattern == 0) ? 4 : 3;

        for (int i = 0; i < iPattern; i++)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, thePlayerPosition);
            Vector3 currentPosition = transform.position;

            if (distanceToPlayer < enemyMovementVariables.AvoidPlayerDistance)
            {
                Vector3 moveDirection = transform.position - thePlayerPosition;
                randomDestination = transform.position + moveDirection.normalized * (enemyMovementVariables.AvoidPlayerDistance + 2);
                agent.SetDestination(randomDestination);
            }
            else if (distanceToPlayer > (enemyMovementVariables.AvoidPlayerDistance + 2) || !agent.pathPending)
            {
                float randomDistance = Random.Range(enemyMovementVariables.AvoidPlayerDistance, enemyMovementVariables.AvoidPlayerDistance + 2);
                Vector3 randomDirection = Random.insideUnitSphere * randomDistance;
                randomDirection += thePlayerPosition;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDirection, out hit, randomDistance, NavMesh.AllAreas);

                // Set the random destination
                randomDestination = hit.position;
                agent.SetDestination(randomDestination);
            }

            float distanceToTarget = Vector3.Distance(currentPosition, randomDestination);

            droneMoveTime = distanceToTarget / agent.speed;

            if (agent.isOnNavMesh)
                yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.1f);

            if (droneSwitch)
            {
                MoveUp();
                droneSwitch = false;
            }
            else
            {
                MoveDown();
                droneSwitch = true;
            }

            if (pattern == 0 && isHoldingGun)
            {
                canFacePlayer = false;
                yield return new WaitForSeconds(0.25f);
                if (detectedPlayer)
                    yield return new WaitForSeconds(EnemyShoot());
                canFacePlayer = true;
            }
            else if (pattern == 1 && i == 2)
            {
                for (int j = 0; j < 3; j++)
                {
                    yield return new WaitForSeconds(0.25f);
                    canFacePlayer = false;
                    yield return new WaitForSeconds(0.25f);
                    if (isHoldingGun && detectedPlayer)
                        yield return new WaitForSeconds(EnemyShoot());
                    canFacePlayer = true;
                }
            }
        }

        agent.ResetPath();
        yield return new WaitForSeconds(enemyMovementVariables.DroneIdleTime);
        isShootingPatternActive = false;
        isWandering = false;

        bodyMeshDrone.localRotation = Quaternion.Euler(0, 0, 0);
    }

    private IEnumerator DefenseDronePattern(Vector3 theProtectorPosition)
    {
        isShootingPatternActive = true;

        for (int i = 0; i < 3; i++)
        {
            Vector3 enemyPosition = theProtectorPosition;
            float radius = enemyMovementVariables.AvoidPlayerDistance;

            Vector3 currentPosition = transform.position;

            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.Normalize();

            Vector3 directionToEnemy = enemyPosition - currentPosition;
            directionToEnemy.y = 0;

            if (directionToEnemy.sqrMagnitude < enemyMovementVariables.AvoidPlayerDistance)
                randomDirection += directionToEnemy.normalized;

            Vector3 targetPosition = currentPosition + randomDirection * radius;
            agent.SetDestination(targetPosition);

            float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);

            droneMoveTime = distanceToTarget / agent.speed;

            if (droneSwitch)
            {
                MoveUp();
                droneSwitch = false;
            }
            else
            {
                MoveDown();
                droneSwitch = true;
            }

            if (agent.isOnNavMesh)
                yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.1f);
        }

        agent.ResetPath();
        yield return new WaitForSeconds(enemyMovementVariables.DroneIdleTime);
        isShootingPatternActive = false;
        isWandering = false;

        bodyMeshDrone.localRotation = Quaternion.Euler(0, 0, 0);
    }

    protected override void Update()
    {
        if (hitBoxScript.CheckIfHitboxScript)
            return;

        playerInSight = CheckPlayerVisibility();

        base.Update();

        if (isHoldingGun && agent != null && (playerInSight || wasHit))
            EnemyMovement();

        if (!isShootingPatternActive && (enemyTypeVariables.DefenseDrone || enemyTypeVariables.OffenseDrone))
        {
            bodyMeshDrone.localPosition = Vector3.Lerp(bodyMeshDrone.localPosition, targetYPos, Time.deltaTime * 2);

            if (Time.time - lastExecutionTime >= interval)
            {
                if (droneSwitch)
                {
                    MoveUp();
                    droneSwitch = false;
                }
                else
                {
                    MoveDown();
                    droneSwitch = true;
                }

                lastExecutionTime = Time.time;
                interval = Random.Range(0.5f, 1.0f);
            }
        }
        else if (isShootingPatternActive && (enemyTypeVariables.DefenseDrone || enemyTypeVariables.OffenseDrone))
        {
            bodyMeshDrone.localPosition = Vector3.Lerp(bodyMeshDrone.localPosition, targetYPos, Time.deltaTime / droneMoveTime);
        }
    }

    private void MoveUp()
    {
        if (isShootingPatternActive)
            targetYPos.y = initialYPosition + (droneHeightOffset * 5);
        else
            targetYPos.y = initialYPosition + droneHeightOffset;
    }

    private void MoveDown()
    {
        if (isShootingPatternActive)
            targetYPos.y = initialYPosition - (droneHeightOffset * 5);
        else
            targetYPos.y = initialYPosition - droneHeightOffset;
    }

    private float EnemyShoot()
    {
        if (!isHoldingGun)
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

        for(int j = 0; j < burst; j++)
        {
            isShooting = true;
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