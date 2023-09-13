using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shootBullet : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GunHand hand;

    // this script is in charge of all the perameters for the guns




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(hand.mouseButton)) SpawnBullet();
    }

    void SpawnBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(transform.forward));
    }
}
