using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShootBullet : MonoBehaviour
{
    public GunHand anim;
    GunInfo info;

    //Camera playerCamera;
    WeaponCameraFX cameraRecoil;

    float recoil = 0;
    float smoothRecoil = 0;

    // this script is in charge of all the perameters for the guns

    float shotTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        shotTimer = Time.time;

        anim = transform.parent.GetComponent<GunHand>();
        info = anim.info;

        cameraRecoil = sweatersController.instance.playerCamera.GetComponent<WeaponCameraFX>();
    }

    // Update is called once per frame
    void Update()
    {
        bool canShoot = (Time.time - shotTimer) > 1/info.fireRate && anim.canShoot && anim.hasGun;
        anim.canShoot = canShoot;

        bool doShoot = info.canAim ? Input.GetMouseButtonUp(anim.mouseButton) : Input.GetMouseButtonDown(anim.mouseButton);
        if (info.fullAuto) doShoot = Input.GetMouseButton(anim.mouseButton);

        if (doShoot && canShoot)
        {
            shotTimer = Time.time;
            for(int i = 0; i < info.burstSize; i++) Invoke(nameof(Shoot), i * 1/info.autoRate);
        }

        transform.LookAt(AcquireTarget.instance.target);

        if(!doShoot) recoil -= 1 / info.recoilReturnTime * Time.deltaTime;
        if(recoil < 0) recoil = 0;

        smoothRecoil += (recoil - smoothRecoil) / 4 * Time.deltaTime * 50;

        float recoilAngle = info.cameraRecoil.Evaluate(smoothRecoil) * info.recoil;

        cameraRecoil.RequestRecoil(recoilAngle);
        //sweatersController.instance.playerCamera.transform.localEulerAngles = new(-recoilAngle, 0, 0);
    }

    void Shoot()
    {
        for (int i = 0; i < info.projectiles; i++) SpawnBullet();
        anim.AnimateShoot();
    }

    void SpawnBullet()
    {
        GameObject bullet = Instantiate(info.bulletPrefab);

        Vector3 direction = transform.forward;

        float spread = info.spreadVariation.Evaluate(recoil) * info.spread;
        if (anim.downSights) spread = 0;

        direction += SpreadDirection(spread, 3);

        bullet.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(direction.normalized));

        Bullet b = bullet.GetComponent<Bullet>();
        b.speed = info.bulletSpeed;
        b.gravity = info.bulletGravity;

        recoil += info.recoilPerShot;
        if (recoil > 1) recoil = 1;

    }

    Vector3 SpreadDirection(float spread, int rolls)
    {
        Vector3 offset = new();
        for (int i = 0; i < rolls; i++) offset += Random.onUnitSphere * spread;
        return offset / rolls;
    }
}
