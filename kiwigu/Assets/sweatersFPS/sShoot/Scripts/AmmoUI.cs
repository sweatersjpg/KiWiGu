using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    EnergyBar display;

    ShootBullet gun;

    public Transform playerHand;

    public Color noWeaponColor;

    public List<Image> images;
    
    // Start is called before the first frame update
    void Start()
    {
        display = GetComponentInChildren<EnergyBar>();
        gun = playerHand.GetComponentInChildren<ShootBullet>();
    }

    void FixedUpdate()
    {
        //if (!FetchGun()) {
        //    display.TargetPercent = 0;

        //    foreach (Image i in images) i.color = noWeaponColor;

        //    return;
        //}

        foreach (Image i in images) i.color = Color.white;

        display.TargetPercent = gun.ammo.count / gun.ammo.capacity;

        if(display.TargetPercent == 0)
        {
            foreach (Image i in images) i.color = noWeaponColor;
        }
    }

    bool FetchGun()
    {
        gun = playerHand.GetComponentInChildren<ShootBullet>();

        return gun != null;
    }
}
