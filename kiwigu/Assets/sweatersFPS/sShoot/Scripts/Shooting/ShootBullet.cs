using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FMODUnity;

public class ShootBullet : MonoBehaviour
{
    [SerializeField] StudioEventEmitter sfxEmitterAvailable;
    [SerializeField] StudioEventEmitter sfxEmitterOut;

    public GunHand anim;
    public ParticleSystem flash;

    float spreadSpeed = 5;
    float spreadTimeStart = 0;

    GunInfo info;

    //Camera playerCamera;
    WeaponCameraFX cameraRecoil;

    float recoil = 0;
    float smoothRecoil = 0;

    float deltaTime;
    float time;

    [HideInInspector] public float charge;
    float chargeTimer;

    public Ammunition ammo;

    // this script is in charge of all the perameters for the guns

    float shotTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        shotTimer = 0;

        anim = transform.parent.GetComponent<GunHand>();
        info = anim.info;

        cameraRecoil = sweatersController.instance.playerCamera.GetComponent<WeaponCameraFX>();

        spreadTimeStart = Random.Range(0, 100);

        if (ammo.capacity == 0) ammo = new Ammunition(info.capacity);
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

        bool canShoot = (Time.time - shotTimer) > 1 / info.fireRate && anim.canShoot && anim.hasGun;
        anim.canShoot = canShoot;

        bool doShoot = (info.canAim || info.canCharge) ?
            Input.GetMouseButtonUp(anim.mouseButton) : Input.GetMouseButtonDown(anim.mouseButton);
        if (info.fullAuto) doShoot = Input.GetMouseButton(anim.mouseButton);

        if (info.canCharge)
        {
            // if (Input.GetMouseButtonDown(anim.mouseButton)) chargeTimerStart = time;
            if (Input.GetMouseButton(anim.mouseButton)) chargeTimer += deltaTime / info.timeToMaxCharge;

            if (chargeTimer > 1) chargeTimer = 1;
            if (chargeTimer < 0) chargeTimer = 0;

            charge = info.chargeCurve.Evaluate(chargeTimer);
        }

        if (doShoot && canShoot)
        {
            if (ammo.count > 0)
            {
                shotTimer = Time.time;
                for (int i = 0; i < info.burstSize; i++) Invoke(nameof(Shoot), i * 1 / info.autoRate);
            }
            else
            {
                if (Input.GetMouseButtonDown(anim.mouseButton))
                    sfxEmitterOut.Play();
            }
        }

        if (ammo.count <= 0) anim.outOfAmmo = true;

        transform.LookAt(AcquireTarget.instance.target);

        if (!doShoot) recoil -= 1 / info.recoilReturnTime * deltaTime;
        if (recoil < 0) recoil = 0;

        smoothRecoil += (recoil - smoothRecoil) / 4 * deltaTime * 50;

        float recoilAngle = info.cameraRecoil.Evaluate(smoothRecoil) * info.recoil;

        cameraRecoil.RequestRecoil(recoilAngle);
        //sweatersController.instance.playerCamera.transform.localEulerAngles = new(-recoilAngle, 0, 0);
    }

    void Shoot()
    {
        ammo.count -= 1;

        sfxEmitterAvailable.SetParameter("Charge", charge);
        sfxEmitterAvailable.Play();

        for (int i = 0; i < info.projectiles; i++) SpawnBullet();
        anim.AnimateShoot();
        if (flash != null) flash.Play();

        chargeTimer = 0;
        // Debug.Log(charge);
    }

    void SpawnBullet()
    {
        GameObject bullet = Instantiate(info.bulletPrefab);

        Vector3 direction = transform.forward;

        float spread = info.spreadVariation.Evaluate(recoil) * info.spread;
        if (anim.downSights) spread = 0;

        if (info.projectiles == 1) direction += PerlinSpreadDirection(spread, 1);
        else direction += SpreadDirection(spread, 2);

        bullet.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(direction.normalized));

        Bullet b = bullet.GetComponent<Bullet>();
        b.speed = info.bulletSpeed;
        b.gravity = info.bulletGravity;
        b.charge = charge;
        b.ignoreMask = ~LayerMask.GetMask("GunHand", "Player", "HookTarget");
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
