using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AmmoUIDiscrete : MonoBehaviour
{
    bool justSwapped = true;

    ShootBullet gun;

    public Transform playerHand;

    [SerializeField] Color emptyColor;
    [SerializeField] Color halfColor;

    [SerializeField] List<Sprite> sprites;
    List<Image> ammoImages;
    [SerializeField] Image weaponImage;

    [SerializeField] Transform ammoGrid;

    void OnEnable()
    {
        ammoImages = new List<Image>(ammoGrid.GetComponentsInChildren<Image>());

        FetchGun();
        foreach (Image i in ammoImages) i.color = Color.white;
    }

    void FixedUpdate()
    {
        if (!FetchGun())
        {
            foreach (Image i in ammoImages)
            {
                i.sprite = sprites[1];  // empty bullet
                i.color = emptyColor;
            }
            weaponImage.color = emptyColor;
            justSwapped = true;

            return;
        }

        if (justSwapped && (gun.ammo.count == gun.ammo.capacity))
        {
            justSwapped = false;
            gun.ShootEvent.AddListener(UseBullet);
            foreach (Image i in ammoImages)
            {
                i.sprite = sprites[0];  // full ammo image
                i.color = Color.white;
            }
            weaponImage.color = Color.white;
        }
    }

    bool FetchGun()
    {
        gun = playerHand.GetComponentInChildren<ShootBullet>();
        return gun != null;
    }

    public void UseBullet()
    {
        if (gun.ammo.capacity < ammoImages.Count) // 1 mag
        {
            ammoImages[(int)(gun.ammo.capacity - gun.ammo.count - 1)].sprite = sprites[1];  // empty ammo image
            ammoImages[(int)(gun.ammo.capacity - gun.ammo.count - 1)].color = emptyColor;
        }
        else    // 2 mags
        {
            if (gun.ammo.count > (gun.ammo.capacity / 2 - 1))   // 1st mag
            {
                ammoImages[(int)((gun.ammo.capacity - gun.ammo.count) - 1)].color = halfColor;
            }
            else    // 2nd mag
            {
                ammoImages[(int)((gun.ammo.capacity / 2) - gun.ammo.count - 1)].sprite = sprites[1];  // empty ammo image
                ammoImages[(int)((gun.ammo.capacity / 2) - gun.ammo.count - 1)].color = emptyColor;
            }

        }

        if (gun.ammo.count == 0)
        {
            weaponImage.color = emptyColor;
        }
    }
}
