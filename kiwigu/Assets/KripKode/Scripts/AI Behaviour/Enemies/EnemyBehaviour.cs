using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static EnemyBase;

public class EnemyBehaviour : EnemyBase
{
    private float exitPointRotationSpeed;

    private bool isMovingUpOrDown;
    private float droneMoveTime;

    private bool isMovingUp;
    private bool isDroneStopped;

    private bool isShootingPatternActive;
    Vector3 patternTargetPosition;

    private float lerpParameter;
    private float targetYPosition;

    private float shootingTimer = 0;
    private float lastShotTimestamp = 0;

    [HideInInspector] public bool canShoot;

    private float initialYPosition;
    private float verticalOffset = 0.75f;
    private float movementDuration;
    private float movementStartTime;
    private bool isCurrentlyInView;

    protected override void Start()
    {
        if (hitBoxScript.CheckIfHitboxScript)
            return;

        base.Start();

        exitPointRotationSpeed = 180;

        if (enemyTypeVariables.DefenseDrone || enemyTypeVariables.OffenseDrone)
        {
            movementDuration = Random.Range(2, 4);
            initialYPosition = agent.height;
            movementStartTime = Time.time;
        }

        shootingTimer = Time.time;
    }

    public void EnemyMovement()
    {
        if (enemyTypeVariables.DefenseDrone)
        {
            if (detectedEnemy)
            {
                if (enemyMainVariables.GunObject)
                {
                    RotateGunObjectExitPoint(playerPosition);
                }

                if (!isDroneStopped)
                {
                    agent.ResetPath();
                    StopAllCoroutines();
                    isWandering = true;
                    isDroneStopped = true;
                }

                if (!isShootingPatternActive)
                {
                    StartCoroutine(DefenseDronePattern(enemyPosition));
                }

                RotateBodyMeshTowardsObj(playerPosition);
            }
            else if (!isShootingPatternActive)
            {
                if (isDroneStopped && isWandering)
                {
                    StartCoroutine(Wander());
                    isDroneStopped = false;
                }
            }
        }
        else if (enemyTypeVariables.OffenseDrone)
        {
            if (detectedPlayer)
            {
                if (enemyMainVariables.GunObject)
                {
                    RotateGunObjectExitPoint(playerPosition);
                }

                if (!isDroneStopped)
                {
                    agent.ResetPath();
                    StopAllCoroutines();
                    isWandering = true;
                    isDroneStopped = true;
                }

                if (!isShootingPatternActive)
                {
                    StartCoroutine(OffenseDronePattern(Random.Range(0, 2), playerPosition));
                }

                RotateBodyMeshTowardsObj(playerPosition);
            }
            else if (!isShootingPatternActive)
            {
                if (isDroneStopped && isWandering)
                {
                    StartCoroutine(Wander());
                    isDroneStopped = false;
                }
            }
        }
        else if (enemyTypeVariables.Small || enemyTypeVariables.Medium)
        {
            if (Vector3.Distance(transform.position, playerPosition) <= enemyMovementVariables.AvoidPlayerDistance)
            {
                if (enemyMainVariables.GunObject)
                {
                    RotateGunObjectExitPoint(playerPosition);
                }

                Vector3 direction = playerPosition - transform.position;
                direction.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), enemyMovementVariables.RotationSpeed * Time.deltaTime);
            }
            else
            {
                if (enemyMainVariables.GunObject)
                {
                    RotateGunObjectExitPoint(playerPosition);
                }

                Vector3 offset = (transform.position - playerPosition).normalized * enemyMovementVariables.AvoidPlayerDistance;
                agent.SetDestination(playerPosition + offset);
            }
        }
    }

    void RotateBodyMeshTowardsObj(Vector3 objPos)
    {
        Quaternion rRot = Quaternion.LookRotation(objPos - enemyMainVariables.BodyMesh.transform.position);
        enemyMainVariables.BodyMesh.transform.rotation = Quaternion.Slerp(enemyMainVariables.BodyMesh.transform.rotation, rRot, Time.deltaTime * 10);
    }

    void RotateGunObjectExitPoint(Vector3 rotPos)
    {
        GameObject GunObjectExitPoint = enemyMainVariables.GunObject.transform.GetChild(0).gameObject;

        Quaternion targetRotation = Quaternion.LookRotation(
            (rotPos + new Vector3(Random.Range(-enemyGunStats.GunInaccuracy, enemyGunStats.GunInaccuracy), 1.5f, Random.Range(-enemyGunStats.GunInaccuracy, enemyGunStats.GunInaccuracy))) - GunObjectExitPoint.transform.position
        );

        GunObjectExitPoint.transform.rotation = Quaternion.Slerp(
            GunObjectExitPoint.transform.rotation,
            targetRotation,
            Time.deltaTime * exitPointRotationSpeed
        );
    }

    private IEnumerator OffenseDronePattern(int pattern, Vector3 thePlayerPosition)
    {
        isShootingPatternActive = true;

        int iPattern;

        if (pattern == 0)
            iPattern = 4;
        else
            iPattern = 3;

        for (int i = 0; i < iPattern; i++)
        {
            isMovingUp = !isMovingUp;

            Vector3 currentPosition = transform.position;

            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.Normalize();

            Vector3 directionToPlayer = thePlayerPosition - currentPosition;
            directionToPlayer.y = 0;

            if (directionToPlayer.sqrMagnitude < enemyMovementVariables.AvoidPlayerDistance)
            {
                randomDirection += directionToPlayer.normalized;
            }

            Vector3 targetPosition = currentPosition + randomDirection * 2;
            agent.SetDestination(targetPosition);

            float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);

            droneMoveTime = distanceToTarget / agent.speed;

            if (isMovingUp)
            {
                patternTargetPosition = new Vector3(enemyMainVariables.BodyMesh.transform.localPosition.x, enemyMainVariables.BodyMesh.transform.localPosition.y + 1.5f, enemyMainVariables.BodyMesh.transform.localPosition.z);
            }
            else
            {
                patternTargetPosition = new Vector3(enemyMainVariables.BodyMesh.transform.localPosition.x, enemyMainVariables.BodyMesh.transform.localPosition.y - 1.5f, enemyMainVariables.BodyMesh.transform.localPosition.z);
            }

            targetYPosition = patternTargetPosition.y;

            isMovingUpOrDown = true;

            if (agent.isOnNavMesh)
                yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.1f);

            isMovingUpOrDown = false;

            switch (pattern)
            {
                case 0:
                    if (isHoldingGun)
                        EnemyShoot();
                    break;
                case 1:
                    if (i == 2)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            yield return new WaitForSeconds(0.35f);
                            if (isHoldingGun)
                                EnemyShoot();
                        }
                    }
                    break;
            }
        }

        agent.ResetPath();
        yield return new WaitForSeconds(enemyMovementVariables.DroneIdleTime);
        isShootingPatternActive = false;
    }

    private IEnumerator DefenseDronePattern(Vector3 theProtectorPosition)
    {
        isShootingPatternActive = true;

        for (int i = 0; i < 3; i++)
        {
            isMovingUp = !isMovingUp;

            Vector3 currentPosition = transform.position;

            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.Normalize();

            Vector3 directionToEnemy = theProtectorPosition - currentPosition;
            directionToEnemy.y = 0;

            if (directionToEnemy.sqrMagnitude < enemyMovementVariables.AvoidPlayerDistance)
            {
                randomDirection += directionToEnemy.normalized;
            }

            Vector3 targetPosition = currentPosition + randomDirection * 2;
            agent.SetDestination(targetPosition);

            float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);

            droneMoveTime = distanceToTarget / agent.speed;

            if (isMovingUp)
            {
                patternTargetPosition = new Vector3(enemyMainVariables.BodyMesh.transform.localPosition.x, enemyMainVariables.BodyMesh.transform.localPosition.y + 1.5f, enemyMainVariables.BodyMesh.transform.localPosition.z);
            }
            else
            {
                patternTargetPosition = new Vector3(enemyMainVariables.BodyMesh.transform.localPosition.x, enemyMainVariables.BodyMesh.transform.localPosition.y - 1.5f, enemyMainVariables.BodyMesh.transform.localPosition.z);
            }

            targetYPosition = patternTargetPosition.y;

            isMovingUpOrDown = true;

            if (agent.isOnNavMesh)
                yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.1f);

            isMovingUpOrDown = false;
        }

        agent.ResetPath();
        yield return new WaitForSeconds(enemyMovementVariables.DroneIdleTime);
        isShootingPatternActive = false;
    }

    protected override void Update()
    {
        if (hitBoxScript.CheckIfHitboxScript)
            return;

        if (enemyTypeVariables.Small || enemyTypeVariables.Medium || enemyTypeVariables.OffenseDrone)
            playerInSight = CheckPlayerVisibility();

        base.Update();

        if (!isCurrentlyInView)
        {
            if (isHoldingGun && playerInSight)
            {
                if (!enemyTypeVariables.DefenseDrone && !enemyTypeVariables.OffenseDrone)
                {
                    if (Time.time - lastShotTimestamp >= 1 / enemyGunStats.EnemyFireRate)
                    {
                        for (int i = 0; i < enemyMainVariables.GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.burstSize; i++)
                            Invoke(nameof(EnemyShoot), i * 1 / enemyMainVariables.GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.autoRate);

                        lastShotTimestamp = Time.time;
                    }
                }
            }
        }

        if (isHoldingGun)
        {
            if (agent != null && (playerInSight || wasHit))
            {
                EnemyMovement();
            }
        }
        else if (!isHoldingGun && enemyTypeVariables.DefenseDrone || enemyTypeVariables.OffenseDrone)
        {
            EnemyMovement();
        }
        else
        {
            EnemyBehaviour();
        }

        if (!isShootingPatternActive)
        {
            if (enemyTypeVariables.DefenseDrone || enemyTypeVariables.OffenseDrone)
            {
                lerpParameter = Mathf.PingPong((Time.time - movementStartTime) / movementDuration, 1);
                targetYPosition = Mathf.Lerp(initialYPosition - verticalOffset, initialYPosition + verticalOffset, lerpParameter);
                patternTargetPosition = new Vector3(enemyMainVariables.BodyMesh.transform.localPosition.x, targetYPosition, enemyMainVariables.BodyMesh.transform.localPosition.z);
                enemyMainVariables.BodyMesh.transform.localPosition = patternTargetPosition;
            }
        }

        if (isMovingUpOrDown)
        {
            Vector3 goTo = new Vector3(enemyMainVariables.BodyMesh.transform.localPosition.x, targetYPosition, enemyMainVariables.BodyMesh.transform.localPosition.z);
            Vector3 lerpPosition = Vector3.Lerp(enemyMainVariables.BodyMesh.transform.localPosition, goTo, Time.deltaTime / droneMoveTime);
            enemyMainVariables.BodyMesh.transform.localPosition = lerpPosition;
        }
    }

    void EnemyShoot()
    {
        if (!isHoldingGun)
            return;

        isShooting = true;

        for (int i = 0; i < enemyMainVariables.GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.projectiles; i++)
            SpawnBullet();
    }

    void SpawnBullet()
    {
        GameObject GunObjectExitPoint = enemyMainVariables.GunObject.transform.GetChild(0).gameObject;

        GameObject bullet = Instantiate(enemyMainVariables.GunObject.GetComponent<EnemyGunInfo>().BulletPrefab, GunObjectExitPoint.transform.position, GunObjectExitPoint.transform.rotation);
        bullet.transform.parent = gameObject.transform;

        Vector3 direction = GunObjectExitPoint.transform.forward;
        direction += SpreadDirection(enemyMainVariables.GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.spread, 3);

        bullet.transform.position = GunObjectExitPoint.transform.position;
        bullet.transform.rotation = Quaternion.LookRotation(direction.normalized);

        EnemyBullet b = bullet.GetComponent<EnemyBullet>();
        b.BulletSpeed = enemyMainVariables.GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.bulletSpeed;
        b.BulletGravity = enemyMainVariables.GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.bulletGravity;
        isShooting = false;
    }

    Vector3 SpreadDirection(float spread, int rolls)
    {
        Vector3 offset = new();
        for (int i = 0; i < rolls; i++) offset += Random.onUnitSphere * spread;
        return offset / rolls;
    }
}
