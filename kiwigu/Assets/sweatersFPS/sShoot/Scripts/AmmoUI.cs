using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    EnergyBar display;

    ShootBullet gun;

    public Transform playerHand;

    public Color noWeaponColor = new Color(1f, 1f, 1f, 0.02f);
    [SerializeField] Color iconColor = new Color(.75f, .75f, .75f);

    public Image energyBarImage;

    [SerializeField] List<Image> otherImages;


    void Start()
    {
        display = GetComponentInChildren<EnergyBar>();
        gun = playerHand.GetComponentInChildren<ShootBullet>();

        foreach (Image i in otherImages) i.color = iconColor;
    }

    void FixedUpdate()
    {
        energyBarImage.color = Color.white;

        display.TargetPercent = gun.ammo.count / gun.ammo.capacity;

        if(display.DisplayPercent == 0)
        {
            energyBarImage.color = noWeaponColor;
            foreach (Image i in otherImages) i.color = noWeaponColor;
        }
    }

    bool FetchGun()
    {
        gun = playerHand.GetComponentInChildren<ShootBullet>();

        return gun != null;
    }
}
