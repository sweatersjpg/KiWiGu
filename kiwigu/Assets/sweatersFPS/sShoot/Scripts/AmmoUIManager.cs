using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoUIManager : MonoBehaviour
{
    GameObject display;

    public Transform playerHand;
    public GameObject hookIcon;

    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        ShootBullet fetchedGun = FetchGun();

        if (fetchedGun == null && display != null)
        {
            Destroy(display);
            display = null;

            // enable hook image
            hookIcon.SetActive(true);
        }

        if (fetchedGun != null && display == null)
        {

            if (!fetchedGun.anim.info.guUI) return;

            hookIcon.SetActive(false);

            display = Instantiate(fetchedGun.anim.info.guUI, transform);

            AmmoUIDiscrete ammo = display.GetComponent<AmmoUIDiscrete>();
            if (ammo) ammo.playerHand = playerHand;
            else display.GetComponentInChildren<AmmoUI>().playerHand = playerHand;
        }

    }

    ShootBullet FetchGun()
    {
        ShootBullet gun = playerHand.GetComponentInChildren<ShootBullet>();

        return gun;
    }
}
