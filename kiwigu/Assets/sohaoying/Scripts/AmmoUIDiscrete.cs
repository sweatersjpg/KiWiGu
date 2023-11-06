using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// WIP I'LL FIX IT SOON OK

public class AmmoUIDiscrete : MonoBehaviour
{
    bool justSwapped = true;

    public ShootBullet gun;

    public Transform playerHand;

    [SerializeField] Color noWeaponColor;

    [SerializeField] List<Sprite> sprites;
    List<Image> bulletImages;

    [SerializeField] GameObject bulletGrid;

    void OnEnable()
    {
        bulletImages = new List<Image>(bulletGrid.GetComponentsInChildren<Image>());

        FetchGun();
        foreach (Image i in bulletImages) i.color = Color.white;
    }

    void FixedUpdate()
    {
        if (!FetchGun())
        {
            foreach (Image i in bulletImages)
            {
                i.sprite = sprites[1];
                i.color = noWeaponColor;
            }
            justSwapped = true;

            return;
        }

        if ((gun.ammo.count == gun.ammo.capacity) && justSwapped)
        {
            justSwapped = false;
            gun.ShootEvent.AddListener(UseBullet);
            foreach (Image i in bulletImages)
            {
                i.color = Color.white;
                i.sprite = sprites[0];
            }
        }
    }

    bool FetchGun()
    {
        gun = playerHand.GetComponentInChildren<ShootBullet>();
        return gun != null;
    }

    public void UseBullet()
    {
        bulletImages[(int)gun.ammo.count].sprite = sprites[1];
        bulletImages[(int)gun.ammo.count].color = noWeaponColor;

        //print("SHOT " + gun.ammo.count + " " + gun.ammo.capacity);
    }
}
