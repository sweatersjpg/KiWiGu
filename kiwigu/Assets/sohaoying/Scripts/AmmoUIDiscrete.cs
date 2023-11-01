using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// WIP I'LL FIX IT BEFORE GOING TO THE STUDIO TOMORROW OK

public class AmmoUIDiscrete : MonoBehaviour
{
    EnergyBar display;

    ShootBullet gun;

    public Transform playerHand;

    [SerializeField] Color noWeaponColor;

    [SerializeField] Image weaponIcon;
    List<Image> images;

    [SerializeField] GameObject bulletGrid;
    [SerializeField] GameObject emptyBulletGrid;

    public List<Transform> bulletTransforms;
    public List<Transform> emptyBulletTransforms;
    
    void Start()
    {
        bulletTransforms = new List<Transform>(bulletGrid.GetComponentsInChildren<Transform>());
        bulletTransforms.Remove(bulletGrid.transform);

        emptyBulletTransforms = new List<Transform>(emptyBulletGrid.GetComponentsInChildren<Transform>());
        emptyBulletTransforms.Remove(emptyBulletGrid.transform);

            Debug.Log(bulletTransforms.Count +" "+emptyBulletTransforms.Count);

        images = new List<Image>(emptyBulletGrid.GetComponentsInChildren<Image>());
        images.Add(weaponIcon);

        foreach (Transform t in emptyBulletTransforms) t.gameObject.SetActive(false);

        FetchGun();
        gun.ShootEvent.AddListener(UseBullet);
    }

    void FixedUpdate()
    {
        if (!FetchGun()) {
            foreach (Transform t in bulletTransforms) t.gameObject.SetActive(false);
            foreach (Transform t in emptyBulletTransforms) t.gameObject.SetActive(true);

            foreach (Image i in images) i.color = noWeaponColor;

            return;
        }

        foreach (Image i in images) i.color = Color.white;
    }

    bool FetchGun()
    {
        gun = playerHand.GetComponentInChildren<ShootBullet>();

        return gun != null;
    }

    void UseBullet()
    {
        bulletTransforms[(int)gun.ammo.count].gameObject.SetActive(false);
        emptyBulletTransforms[(int)gun.ammo.count].gameObject.SetActive(true);
        
        print(gun.ammo.count+" "+gun.ammo.capacity);
        
        if (gun.ammo.count == 0) {
            foreach (Image i in images) i.color = noWeaponColor;
        }
    }
}
