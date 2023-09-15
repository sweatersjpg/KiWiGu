using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShootBullet : MonoBehaviour
{
    public GunHand anim;
    GunInfo info;

    // this script is in charge of all the perameters for the guns

    float shotTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        shotTimer = Time.time;

        anim = transform.parent.GetComponent<GunHand>();
        info = anim.info;
    }

    // Update is called once per frame
    void Update()
    {
        bool canShoot = (Time.time - shotTimer) > 1/info.fireRate;
        anim.canShoot = canShoot;

        bool doShoot = info.canAim ? Input.GetMouseButtonUp(anim.mouseButton) : Input.GetMouseButtonDown(anim.mouseButton);
        if (info.fireType == GunInfo.FireType.Auto) doShoot = Input.GetMouseButton(anim.mouseButton);

        if (doShoot && canShoot)
        {
            for(int i = 0; i < info.projectiles; i++) SpawnBullet();
            shotTimer = Time.time;
            anim.AnimateShoot();
        }
    }

    void SpawnBullet()
    {
        GameObject bullet = Instantiate(info.bulletPrefab);

        Vector3 direction = transform.forward;
        direction += SpreadDirection(info.spread, 3);

        bullet.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(direction.normalized));

        Bullet b = bullet.GetComponent<Bullet>();
        b.speed = info.bulletSpeed;
        b.gravity = info.bulletGravity;

    }

    Vector3 SpreadDirection(float spread, int rolls)
    {
        Vector3 offset = new();
        for (int i = 0; i < rolls; i++) offset += Random.onUnitSphere * spread;
        return offset / rolls;
    }
}
