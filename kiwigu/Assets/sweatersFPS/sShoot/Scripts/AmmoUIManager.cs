using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoUIManager : MonoBehaviour
{
    GameObject display;

    public Transform playerHand;
    public GameObject hookIcon;

    [SerializeField] List<GameObject> controls;
    [SerializeField] GameObject noAmmoIndicator;

    void FixedUpdate()
    {
        ShootBullet fetchedGun = FetchGun();

        if (fetchedGun == null && display != null)  // when you've just thrown a gun
        {
            Destroy(display);
            display = null;

            // enable hook image
            hookIcon.SetActive(true);
            // disable keyboard control tooltips
            foreach (GameObject o in controls)
            {
                o.SetActive(false);
            }
            noAmmoIndicator.SetActive(false);
        }

        if (fetchedGun != null && display == null)  // when you've just grabbed a gun
        {

            if (!fetchedGun.anim.info.guUI) return;

            hookIcon.SetActive(false);
            // enable keyboard control tooltips
            foreach (GameObject o in controls)
            {
                o.SetActive(true);
            }

            display = Instantiate(fetchedGun.anim.info.guUI, transform);

            AmmoUIDiscrete ammo = display.GetComponent<AmmoUIDiscrete>();
            if (ammo) ammo.playerHand = playerHand;
            else display.GetComponentInChildren<AmmoUI>().playerHand = playerHand;
        }

        if (fetchedGun != null) // when you're holding a gun
        {
            if (fetchedGun.ammo.count == 0) noAmmoIndicator.SetActive(true);
        }

    }

    ShootBullet FetchGun()
    {
        ShootBullet gun = playerHand.GetComponentInChildren<ShootBullet>();

        return gun;
    }
}
