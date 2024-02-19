using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ShootBullet : MonoBehaviour
{
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

    public UnityEvent ShootEvent;

    // Start is called before the first frame update
    void Start()
    {
        if (ShootEvent == null) ShootEvent = new UnityEvent();

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

        string[] shootButtons = { "LeftShoot", "RightShoot" };
        string shootButton = shootButtons[anim.mouseButton];

        bool doShoot = (info.canCharge) ?
            Input.GetButtonUp(shootButton) : Input.GetButtonDown(shootButton);
        if (info.fullAuto) doShoot = Input.GetButton(shootButton);

        if (info.canCharge)
        {
            // if (Input.GetMouseButtonDown(anim.mouseButton)) chargeTimerStart = time;
            if (Input.GetButton(shootButton)) chargeTimer += deltaTime / info.timeToMaxCharge;

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
                if (Input.GetButtonDown(shootButton))
                    Debug.Log("play audio sound here code 1");
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

        for (int i = 0; i < info.projectiles; i++) SpawnBullet();
        anim.AnimateShoot();
        if (flash != null) flash.Play();

        chargeTimer = 0;
        // Debug.Log(charge);

        ShootEvent.Invoke();

        DoRumble();
    }

    void DoRumble()
    {
        if (Input.GetJoystickNames().Length == 0) return;
        
        Gamepad.current.SetMotorSpeeds(Random.Range(0.2f, 0.8f), Random.Range(0.5f, 1f));
        Invoke(nameof(StopRumble), 0.2f);
    }

    void StopRumble()
    {
        if (Input.GetJoystickNames().Length == 0) return;
        Gamepad.current.SetMotorSpeeds(0, 0);
    }

    private void OnDestroy()
    {
        StopRumble();
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
