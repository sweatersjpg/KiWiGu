using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletShooter : MonoBehaviour
{
    public GunInfo info;
    public ParticleSystem flash;
    float deltaTime;
    float time;

    float recoil = 0;
    float smoothRecoil = 0;

    float spreadSpeed = 5;
    float spreadTimeStart = 0;

    float shotTimer = 0;

    float charge;
    float chargeTimer = 0;

    bool canShoot = true;
    bool hasGun = true;

    [Space]
    [SerializeField] float shootDelay = 0.1f;
    [SerializeField] float shootDelayVariation = 0.1f;

    [Space]
    public float baseSpread = 0.01f;

    [Space]
    public bool isShooting = false;
    public float shotsToTake = 0; // discrete number for non auto -- time for full auto
    // OR override per frame

    [Header("Debug Only")]
    public bool shootForever = false;

    // Start is called before the first frame update
    void Start()
    {
        if (info.gunName == "ELB")
        {
            transform.Find("LaserBeamFX").gameObject.SetActive(true);
            flash.gameObject.SetActive(false);

            baseSpread = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseSystem.paused)
        {
            deltaTime = 0;
            return;
        }
        else
        {
            deltaTime = Time.deltaTime;
            time += Time.deltaTime;
        }
        
        // shoot until no shots left
        if(shotsToTake > 0 || shootForever)
        {
            isShooting = true;
            shotsToTake = Mathf.Max(0, shotsToTake - deltaTime);
        }

        float fireRate = info.fireRate;

        // if can charger and full audo => link charge to fire rate
        if (info.fullAuto && info.canCharge) fireRate = charge * info.fireRate;

        bool canShoot = (Time.time - shotTimer) > 1 / fireRate;

        bool doShoot = isShooting;

        if (info.canCharge)
        {
            // if (Input.GetMouseButtonDown(anim.mouseButton)) chargeTimerStart = time;
            if(isShooting) chargeTimer += deltaTime / info.timeToMaxCharge;
            else chargeTimer = 0;

            if (chargeTimer > 1) chargeTimer = 1;
            if (chargeTimer < 0) chargeTimer = 0;

            charge = info.chargeCurve.Evaluate(chargeTimer);
        }

        if (doShoot && canShoot)
        {
            float variation = shootDelay + Random.Range(-shootDelayVariation, shootDelayVariation);
            if (info.fullAuto) variation = 0;
            else chargeTimer = 0;

            shotTimer = Time.time + variation;

            // if(!info.fullAuto) shotsToTake = Mathf.Max(0, shotsToTake - 1);
            isShooting = false;

            for (int i = 0; i < info.burstSize; i++) Invoke(nameof(Shoot), i * 1 / info.autoRate);
        }
    }

    void Shoot()
    {
        GlobalAudioManager.instance.PlayGunFire(transform, info);

        for (int i = 0; i < info.projectiles; i++) SpawnBullet();
        // anim.AnimateShoot();
        if (flash != null) flash.Play();

        // Debug.Log(charge);
    }

    void SpawnBullet()
    {
        GameObject bullet = Instantiate(info.bulletPrefab);
        Vector3 direction = transform.forward;
        float spread = info.spreadVariation.Evaluate(recoil) * info.spread + baseSpread;

        if (info.projectiles == 1) direction += PerlinSpreadDirection(spread, 1);
        else direction += SpreadDirection(spread, 2);

        bullet.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(direction.normalized));

        Bullet b = bullet.GetComponent<Bullet>();
        b.speed = info.bulletSpeed;
        b.gravity = info.bulletGravity;
        b.charge = charge;
        b.ignoreMask = ~LayerMask.GetMask("GunHand", "HookTarget", "BulletView");
        b.bulletDamage = info.damage;

        recoil += info.recoilPerShot;
        if (recoil > 1) recoil = 1;

    }

    Vector3 PerlinSpreadDirection(float spread, int rolls)
    {
        Vector3 offset = new();

        for (int i = 0; i < rolls; i++)
        {
            float t = time * spreadSpeed + spreadTimeStart;

            float x = Mathf.PerlinNoise(t, 0) * 2 - 1;
            float y = Mathf.PerlinNoise(0, t) * 2 - 1;
            float z = Mathf.PerlinNoise(t * Mathf.Sqrt(2) / 2, t * Mathf.Sqrt(2) / 2) * 2 - 1;

            Vector3 v = new(x, y, z);
            // v = v.normalized * (v.magnitude % 1); // stay within 1

            offset += v * spread;
        }

        return offset / rolls;
    }

    Vector3 SpreadDirection(float spread, int rolls)
    {
        Vector3 offset = new();
        for (int i = 0; i < rolls; i++) offset += Random.onUnitSphere * spread;
        return offset / rolls;
    }

    public void SetShots(float shots)
    {
        shotsToTake = shots;
    }

    public void SetShootTime(float t)
    {
        shotsToTake = t;
    }

    public void SetIsShooting()
    {
        isShooting = true;
    }

}
