using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBullet : MonoBehaviour
{
    public GunHand anim;
    public GunInfo info;

    // this script is in charge of all the perameters for the guns




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(anim.mouseButton)) SpawnBullet();
    }

    void SpawnBullet()
    {
        GameObject bullet = Instantiate(info.bulletPrefab);
        bullet.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(transform.forward));

        Bullet b = bullet.GetComponent<Bullet>();

    }
}
