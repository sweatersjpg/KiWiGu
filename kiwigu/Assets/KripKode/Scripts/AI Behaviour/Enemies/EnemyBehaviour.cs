using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyBehaviour : EnemyBase
{
    float shotTimer = 0;
    float lastShotTime = 0;

    [HideInInspector] public bool canShoot;

    protected override void Start()
    {
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
            Debug.Log("Offense Drone Movement");
        }
        else if (Small || Medium)
        {
            if (Vector3.Distance(transform.position, player.position) <= AvoidPlayerDistance)
            {
                if (GunObject)
                {
                    GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;
                    GunObjectExitPoint.transform.LookAt(player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy)));
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
                    GunObjectExitPoint.transform.LookAt(player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy)));
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

        if (isHoldingGun && playerInSight)
        {
            if (Time.time - lastShotTime >= 1 / EnemyFireRate)
            {
                for (int i = 0; i < GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.burstSize; i++)
                    Invoke(nameof(EnemyShoot), i * 1 / GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.autoRate);

                lastShotTime = Time.time;
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
