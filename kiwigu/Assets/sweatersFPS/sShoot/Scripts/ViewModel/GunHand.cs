using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunHand : MonoBehaviour
{

    [HideInInspector] public bool obstructed = false; // gun against wall

    [HideInInspector] public bool downSights = false;

    Vector3 startPosition; // local
    Vector3 targetPosition; // local

    Vector3 sightsPosition;

    public int mouseButton;
    public float aimDelay = 0.4f;

    public GunInfo info;
    float gunAngle = 0;
    float targetAngle = 0;

    float aimTimer = 0;

    public bool canShoot;
    public bool hasGun = true;

    public GameObject thrownGunPrefab;
    public GameObject hookShotPrefab;

    WeaponCameraFX cameraFX;

    public Transform view;
    public Transform parent;

    float deltaTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (view == null) view = transform.parent.parent;
        if (parent == null) parent = transform.parent.parent;

        startPosition = view.localPosition;
        targetPosition = startPosition;

        if (parent.localPosition.x > 0) mouseButton = 1;
        else mouseButton = 0;

        Transform sights = transform.Find("Sights");
        if(sights != null) sightsPosition = sights.localPosition;

        cameraFX = sweatersController.instance.playerCamera.GetComponent<WeaponCameraFX>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (PauseSystem.paused) deltaTime = 0;
        else deltaTime = Time.deltaTime;

        if (PauseSystem.paused) return;
        
        //if (Input.GetMouseButtonDown(1)) ToggleDownSights();
        //if (Input.GetMouseButtonDown(0)) AnimateShoot();

        if (Input.GetMouseButton(mouseButton) && aimTimer <= aimDelay) aimTimer += deltaTime;
        if (!Input.GetMouseButton(mouseButton) && aimTimer >= 0) aimTimer -= deltaTime;

        //if (Input.GetMouseButtonUp(mouseButton))
        //{
        //    //if(downSights) Invoke(nameof(ToggleDownSights), 0.1f);
        //    //aimTimer = 0;
        //    AnimateShoot();
        //}

        view.localPosition += 50 * ((targetPosition - view.localPosition) / 4) * deltaTime;

        if((targetPosition - view.localPosition).magnitude < 0.01 && !hasGun)
        {
            view.localPosition = startPosition;
            Instantiate(hookShotPrefab, transform.parent);
            Destroy(gameObject);
        }
        // we don't actually need to do that in this script, the shootBullet script can do that for us
        //view.LookAt(AcquireTarget.instance.target);

        gunAngle += 50 * ((targetAngle - gunAngle) / 8) * deltaTime;
        transform.localEulerAngles = new(gunAngle, 0, 0);

        if (!canShoot)
        {
            targetAngle = 5;
            canShoot = true;
        }

        if (!hasGun) return;

        if (Input.GetKeyDown(mouseButton == 0 ? KeyCode.Q : KeyCode.E))
        {
            targetAngle = -45;
            targetPosition = startPosition + new Vector3(0, 0.3f, -0.2f);
            canShoot = false;
            Invoke(nameof(ThrowGun), 0.2f);
        }
        else targetAngle = 0;

        if (Input.GetKeyUp(mouseButton == 0 ? KeyCode.Q : KeyCode.E))
        {
            CancelInvoke(nameof(ThrowGun));
            targetPosition = startPosition;
        }

        // if (Input.GetKeyUp(mouseButton == 0 ? KeyCode.Q : KeyCode.E)) ThrowGun();

        if (aimTimer <= 0 && downSights) ToggleDownSights();
        if (aimTimer >= aimDelay && !downSights && info.canAim) ToggleDownSights();

        if (downSights) cameraFX.RequestFOV(info.scopeFOV);
    }

    void ThrowGun()
    {
        ThrownGun gun = Instantiate(thrownGunPrefab, transform).GetComponent<ThrownGun>();
        gun.transform.parent = null;
        gun.transform.LookAt(AcquireTarget.instance.target);

        gun.ammo = GetComponentInChildren<ShootBullet>().ammo;

        Transform gunView = transform.Find("GunView");

        gun.SetMesh(gunView.GetComponent<MeshFilter>().mesh);
        gun.info = info;

        gunView.gameObject.SetActive(false);
        hasGun = false;

        targetPosition = startPosition + new Vector3(0, -1f, 0.5f);
    }

    public void ToggleDownSights()
    {
        if (downSights) targetPosition = startPosition;
        else
        {
            if (!cameraFX.ScopedIn()) targetPosition = new(-sightsPosition.x, -sightsPosition.y, startPosition.z);
            else return;
        }

        downSights = !downSights;
    }

    public void AnimateShoot()
    {
        gunAngle -= info.recoil / 2;

        view.localPosition += new Vector3(0, 0, -0.1f);
    }
}
