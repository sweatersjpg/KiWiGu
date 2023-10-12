using System.Collections;
using UnityEngine;

public class EnemyBehaviour : EnemyBase
{
    float exitPointRotationSpeed;

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
    private float verticalOffset = 0.25f;
    private float movementDuration;
    private float movementStartTime;
    private bool isCurrentlyInView;

    protected override void Start()
    {
        exitPointRotationSpeed = 180;

        if (DefenseDrone || OffenseDrone)
        {
            movementDuration = Random.Range(2, 4);
            initialYPosition = BodyMesh.transform.position.y;
            movementStartTime = Time.time;
        }

        base.Start();
        shootingTimer = Time.time;
    }

    public void EnemyMovement()
    {
        if (DefenseDrone)
        {
            // leave this empty for now
        }
        else if (OffenseDrone)
        {
            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.center = new Vector3(sphereCollider.center.x, BodyMesh.transform.position.y, sphereCollider.center.z);

            if (detectedPlayer)
            {
                if (GunObject)
                {
                    RotateGunObjectExitPoint();
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

                RotateBodyMeshTowardsPlayer();
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
        else if (Small || Medium)
        {
            if (Vector3.Distance(transform.position, playerPosition) <= AvoidPlayerDistance)
            {
                if (GunObject)
                {
                    RotateGunObjectExitPoint();
                }

                Vector3 direction = playerPosition - transform.position;
                direction.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), RotationSpeed * Time.deltaTime);
            }
            else
            {
                if (GunObject)
                {
                    RotateGunObjectExitPoint();
                }

                Vector3 offset = (transform.position - playerPosition).normalized * AvoidPlayerDistance;
                agent.SetDestination(playerPosition + offset);
            }
        }
    }

    void RotateBodyMeshTowardsPlayer()
    {
        Quaternion rRot = Quaternion.LookRotation(playerPosition - BodyMesh.transform.position);
        BodyMesh.transform.rotation = Quaternion.Slerp(BodyMesh.transform.rotation, rRot, Time.deltaTime * 10);
    }
    
    void RotateGunObjectExitPoint()
    {
        GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;

        Quaternion targetRotation = Quaternion.LookRotation(
            (playerPosition + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy))) - GunObjectExitPoint.transform.position
        );

        GunObjectExitPoint.transform.rotation = Quaternion.Slerp(
            GunObjectExitPoint.transform.rotation,
            targetRotation,
            Time.deltaTime * exitPointRotationSpeed
        );
    }

    IEnumerator MoveBodyMesh(Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0;
        Vector3 initialPosition = BodyMesh.transform.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            BodyMesh.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            yield return null;
        }
    }

    private IEnumerator OffenseDronePattern(int pattern, Vector3 playerPosition)
    {
        isShootingPatternActive = true;

        for (int i = 0; i < 4; i++)
        {
            isMovingUp = !isMovingUp;
            Vector3 currentPosition = transform.position;
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.Normalize();

            Vector3 targetPosition = currentPosition + randomDirection * 4;
            agent.SetDestination(targetPosition);

            float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
            droneMoveTime = distanceToTarget / agent.speed;

            if (isMovingUp)
            {
                patternTargetPosition = new Vector3(BodyMesh.transform.position.x, BodyMesh.transform.position.y + 1.5f, BodyMesh.transform.position.z);
            }
            else
            {
                patternTargetPosition = new Vector3(BodyMesh.transform.position.x, BodyMesh.transform.position.y - 1.5f, BodyMesh.transform.position.z);
            }

            targetYPosition = patternTargetPosition.y;

            isMovingUpOrDown = true;

            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.1f);

            isMovingUpOrDown = false;

            switch(pattern)
            {
                case 0:
                    EnemyShoot();
                    break;
                case 1:
                    if (i == 3)
                    {
                        yield return new WaitForSeconds(0.25f);
                        EnemyShoot();
                        yield return new WaitForSeconds(0.25f);
                        EnemyShoot();
                        yield return new WaitForSeconds(0.25f);
                        EnemyShoot();
                    }
                    break;
            }
        }

        agent.ResetPath();
        yield return new WaitForSeconds(DroneIdleTime);
        isShootingPatternActive = false;
    }


    protected override void Update()
    {
        playerInSight = CheckPlayerVisibility();

        base.Update();

        if (!isCurrentlyInView)
        {
            if (isHoldingGun && playerInSight)
            {
                if (!DefenseDrone && !OffenseDrone)
                {
                    if (Time.time - lastShotTimestamp >= 1 / EnemyFireRate)
                    {
                        for (int i = 0; i < GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.burstSize; i++)
                            Invoke(nameof(EnemyShoot), i * 1 / GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.autoRate);

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
        else if (!isHoldingGun && (DefenseDrone || OffenseDrone))
        {
            EnemyMovement();
        }
        else
        {
            EnemyBehaviour();
        }

        if (!isShootingPatternActive)
        {
            if (DefenseDrone || OffenseDrone)
            {
                lerpParameter = Mathf.PingPong((Time.time - movementStartTime) / movementDuration, 1);
                targetYPosition = Mathf.Lerp(initialYPosition - verticalOffset, initialYPosition + verticalOffset, lerpParameter);
                patternTargetPosition = new Vector3(BodyMesh.transform.position.x, targetYPosition, BodyMesh.transform.position.z);
                BodyMesh.transform.position = patternTargetPosition;
            }
        }

        if (isMovingUpOrDown)
        {
            Vector3 goTo = new Vector3(BodyMesh.transform.position.x, targetYPosition, BodyMesh.transform.position.z);
            Vector3 lerpPosition = Vector3.Lerp(BodyMesh.transform.position, goTo, Time.deltaTime / droneMoveTime);
            BodyMesh.transform.position = lerpPosition;
        }
    }

    void EnemyShoot()
    {
        if (!isHoldingGun)
            return;

        isShooting = true;

        for (int i = 0; i < GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.projectiles; i++)
            SpawnBullet();
    }

    void SpawnBullet()
    {
        GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;

        GameObject bullet = Instantiate(GunObject.GetComponent<EnemyGunInfo>().BulletPrefab, GunObjectExitPoint.transform.position, GunObjectExitPoint.transform.rotation);
        bullet.transform.parent = gameObject.transform;

        Vector3 direction = GunObjectExitPoint.transform.forward;
        direction += SpreadDirection(GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.spread, 3);

        bullet.transform.position = GunObjectExitPoint.transform.position;
        bullet.transform.rotation = Quaternion.LookRotation(direction.normalized);

        EnemyBullet b = bullet.GetComponent<EnemyBullet>();
        b.BulletSpeed = GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.bulletSpeed;
        b.BulletGravity = GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.bulletGravity;
        isShooting = false;
    }

    Vector3 SpreadDirection(float spread, int rolls)
    {
        Vector3 offset = new();
        for (int i = 0; i < rolls; i++) offset += Random.onUnitSphere * spread;
        return offset / rolls;
    }
}