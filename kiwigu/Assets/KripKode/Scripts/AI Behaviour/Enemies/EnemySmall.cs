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

        if (isHoldingGun && CheckPlayerVisibility())
        {
            if (Time.time - lastShotTime >= 1 / EnemyFireRate)
            {
                for (int i = 0; i < info.burstSize; i++)
                    Invoke(nameof(EnemyShoot), i * 1 / info.autoRate);
                
                lastShotTime = Time.time;
            }
        }
    }

    void EnemyShoot()
    {
        if (!isHoldingGun)
            return;

        for (int i = 0; i < info.projectiles; i++)
            SpawnBullet();
    }

    void SpawnBullet()
    {
        GameObject bullet = Instantiate(BulletPrefab, GunObject.transform.position, GunObject.transform.rotation);
        bullet.transform.parent = gameObject.transform;

        Vector3 direction = GunObject.transform.forward;
        direction += SpreadDirection(info.spread, 3);

        bullet.transform.position = GunObject.transform.position;
        bullet.transform.rotation = Quaternion.LookRotation(direction.normalized);

        EnemyBullet b = bullet.GetComponent<EnemyBullet>();
        b.BulletSpeed = info.bulletSpeed;
        b.BulletGravity = info.bulletGravity;
    }

    Vector3 SpreadDirection(float spread, int rolls)
    {
        Vector3 offset = new();
        for (int i = 0; i < rolls; i++) offset += Random.onUnitSphere * spread;
        return offset / rolls;
    }
}
