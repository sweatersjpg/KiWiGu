using System.Collections;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyBehaviour : EnemyBase
{
    float shotTimer = 0;
    float lastShotTime = 0;

    [HideInInspector] public bool canShoot;

    private float initialPositionY;
    private float verticalOffset = 0.25f;
    private float duration;
    private float startTime;
    private bool isInView;

    protected override void Start()
    {
        if (DefenseDrone || OffenseDrone)
        {
            duration = Random.Range(1f, 3f);
            initialPositionY = BodyMesh.transform.position.y;
            startTime = Time.time;
        }


        base.Start();
        shotTimer = Time.time;
    }

    public void EnemyMovement()
    {
        if (DefenseDrone)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, EnemyAwareDistance);
            bool enemyDetected = false;

            foreach (var collider in hitColliders)
            {
                if (collider.gameObject.tag == "Enemy" && collider.gameObject != gameObject)
                {
                    Vector3 direction = collider.gameObject.transform.position - transform.position;
                    direction.y = 0;

                    float noiseX = Mathf.PerlinNoise(Time.time * RotationSpeed, 0) * 2 - 1;
                    float noiseY = Mathf.PerlinNoise(0, Time.time * RotationSpeed) * 2 - 1;
                    Quaternion noiseRotation = Quaternion.Euler(new Vector3(noiseX, noiseY, 0));

                    Quaternion targetRotation = Quaternion.LookRotation(direction) * noiseRotation;

                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

                    Vector3 noiseOffset = new Vector3(
                        Mathf.PerlinNoise(Time.time * MovementSpeed, 0) * 1 - (1 / 2),
                        0,
                        Mathf.PerlinNoise(0, Time.time * MovementSpeed) * 1 - (1 / 2)
                    );

                    Vector3 targetPosition = collider.gameObject.transform.position + noiseOffset;

                    agent.SetDestination(targetPosition);

                    enemyDetected = true;
                    break;
                }
            }

            if (!enemyDetected && !isWandering)
            {
                StartCoroutine(Wander());
            }
        }
        else if (OffenseDrone)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, EnemyAwareDistance);
            bool enemyDetected = false;

            foreach (var collider in hitColliders)
            {
                if (collider.gameObject.tag == "Player" && collider.gameObject != gameObject)
                {
                    if (GunObject)
                    {
                        GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;

                        Quaternion targetRotation = Quaternion.LookRotation(
                            (player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy))) - GunObjectExitPoint.transform.position
                        );

                        float rotationSpeed = 50;
                        //GunObjectExitPoint.transform.rotation = Quaternion.Slerp(
                        //    GunObjectExitPoint.transform.rotation,
                        //    targetRotation,
                        //    Time.deltaTime * rotationSpeed
                        //);
                    }

                    Vector3 playerPosition = collider.gameObject.transform.position;
                    Vector3 direction = playerPosition - transform.position;
                    direction.y = 0;

                    float distanceToPlayer = direction.magnitude;

                    if (distanceToPlayer <= AvoidPlayerDistance)
                    {
                        Vector3 toObject = transform.position - Camera.main.transform.position;
                        float angleToObject = Vector3.Angle(Camera.main.transform.forward, toObject);

                        if (angleToObject <= Camera.main.fieldOfView * 0.5f)
                        {
                            isInView = false;

                            agent.ResetPath();

                            Vector3 randomDirection = Quaternion.Euler(0, Random.Range(Random.Range(-100, -50), Random.Range(50, 100)), 0) * Camera.main.transform.forward;
                            Vector3 targetPosition = Camera.main.transform.position + randomDirection * AvoidPlayerDistance;

                            agent.SetDestination(targetPosition);
                        }
                        else
                        {
                            isInView = true;

                            Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * AvoidPlayerDistance;

                            agent.SetDestination(targetPosition);
                        }
                    }
                    else
                    {
                        agent.SetDestination(player.position);
                    }

                    Quaternion rRot = Quaternion.LookRotation(playerPosition - BodyMesh.transform.position);
                    BodyMesh.transform.rotation = Quaternion.Slerp(BodyMesh.transform.rotation, rRot, Time.deltaTime * 10);
                }
            }

            if (!enemyDetected && !isWandering)
            {
                StartCoroutine(Wander());
            }
        }
        else if (Small || Medium)
        {
            if (Vector3.Distance(transform.position, player.position) <= AvoidPlayerDistance)
            {
                if (GunObject)
                {
                    GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;

                    Quaternion targetRotation = Quaternion.LookRotation(
                        (player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy))) - GunObjectExitPoint.transform.position
                    );

                    float rotationSpeed = 50;
                    //GunObjectExitPoint.transform.rotation = Quaternion.Slerp(
                    //    GunObjectExitPoint.transform.rotation,
                    //    targetRotation,
                    //    Time.deltaTime * rotationSpeed
                    //);
                }

                Vector3 direction = player.position - transform.position;
                direction.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), RotationSpeed * Time.deltaTime);
            }
            else
            {
                if (GunObject)
                {
                    GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;

                    Quaternion targetRotation = Quaternion.LookRotation(
                        (player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy))) - GunObjectExitPoint.transform.position
                    );

                    float rotationSpeed = 50;
                    //GunObjectExitPoint.transform.rotation = Quaternion.Slerp(
                    //    GunObjectExitPoint.transform.rotation,
                    //    targetRotation,
                    //    Time.deltaTime * rotationSpeed
                    //);
                }

                Vector3 offset = (transform.position - player.position).normalized * AvoidPlayerDistance;
                agent.SetDestination(player.position + offset);
            }
        }
    }


    protected override void Update()
    {
        playerInSight = CheckPlayerVisibility();

        base.Update();

        if (OffenseDrone && isInView)
        {
            // uwu
        }
        else
        {
            if (isHoldingGun && playerInSight)
            {
                if (Time.time - lastShotTime >= 1 / EnemyFireRate)
                {
                    for (int i = 0; i < GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.burstSize; i++)
                        Invoke(nameof(EnemyShoot), i * 1 / GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.autoRate);

                    lastShotTime = Time.time;
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

        if (DefenseDrone || OffenseDrone)
        {
            float t = Mathf.PingPong((Time.time - startTime) / duration, 1f);
            float newYPosition = Mathf.Lerp(initialPositionY - verticalOffset, initialPositionY + verticalOffset, t);
            Vector3 newPosition = new Vector3(BodyMesh.transform.position.x, newYPosition, BodyMesh.transform.position.z);
            BodyMesh.transform.position = newPosition;
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
        //bullet.transform.rotation = Quaternion.LookRotation(direction.normalized);

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
