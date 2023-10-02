using UnityEngine;

public class EnemySmall : EnemyBase
{
    [Header("Small Enemy")] public GameObject BulletPrefab;

    float shotTimer = 0;
    float lastShotTime = 0;

    [HideInInspector] public bool canShoot;

    protected override void Start()
    {
        base.Start();
        shotTimer = Time.time;
    }

    public override void EnemyMovement()
    {
        base.EnemyMovement();
    }

    protected override void Update()
    {
        base.Update();

        if (isHoldingGun && CheckPlayerVisibility() && !startedFleeing)
        {
            if (Time.time - lastShotTime >= 1 / EnemyFireRate)
            {
                for (int i = 0; i < GunAssetInfo.burstSize; i++)
                    Invoke(nameof(EnemyShoot), i * 1 / GunAssetInfo.autoRate);
                
                lastShotTime = Time.time;
            }
        }
    }

    void EnemyShoot()
    {
        if (!isHoldingGun)
            return;

        isShooting = true;

        for (int i = 0; i < GunAssetInfo.projectiles; i++)
            SpawnBullet();
    }

    void SpawnBullet()
    {
        GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;

        GameObject bullet = Instantiate(BulletPrefab, GunObjectExitPoint.transform.position, GunObjectExitPoint.transform.rotation);
        bullet.transform.parent = gameObject.transform;

        Vector3 direction = GunObjectExitPoint.transform.forward;
        direction += SpreadDirection(GunAssetInfo.spread, 3);

        bullet.transform.position = GunObjectExitPoint.transform.position;
        bullet.transform.rotation = Quaternion.LookRotation(direction.normalized);

        EnemyBullet b = bullet.GetComponent<EnemyBullet>();
        b.BulletSpeed = GunAssetInfo.bulletSpeed;
        b.BulletGravity = GunAssetInfo.bulletGravity;
        isShooting = false;
    }

    Vector3 SpreadDirection(float spread, int rolls)
    {
        Vector3 offset = new();
        for (int i = 0; i < rolls; i++) offset += Random.onUnitSphere * spread;
        return offset / rolls;
    }
}
