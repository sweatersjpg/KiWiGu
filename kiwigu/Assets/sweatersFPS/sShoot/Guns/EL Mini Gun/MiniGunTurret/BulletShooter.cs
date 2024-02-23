using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MiniMenuSystem;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

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

    public bool isShooting = false;

    // Start is called before the first frame update
    void Start()
    {
        
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
            shotTimer = Time.time;

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
        float spread = info.spreadVariation.Evaluate(recoil) * info.spread;

        if (info.projectiles == 1) direction += PerlinSpreadDirection(spread, 1);
        else direction += SpreadDirection(spread, 2);

        bullet.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(direction.normalized));

        Bullet b = bullet.GetComponent<Bullet>();
        b.speed = info.bulletSpeed;
        b.gravity = info.bulletGravity;
        b.charge = charge;
        b.ignoreMask = ~LayerMask.GetMask("GunHand", "HookTarget");
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
}
