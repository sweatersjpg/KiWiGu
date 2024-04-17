using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class GunHand : MonoBehaviour
{

    [HideInInspector] public bool obstructed = false; // gun against wall

    [HideInInspector] public bool downSights = false;

    //Vector3 startPosition; // local
    //Vector3 targetPosition; // local

    //Vector3 sightsPosition;

    public int mouseButton;
    public float aimDelay = 0.4f;

    public GunInfo info;
    //float gunAngle = 0;
    //float targetAngle = 0;

    //float aimTimer = 0;

    public bool canShoot;
    public bool hasGun = true;

    public GameObject thrownGunPrefab;
    public GameObject hookShotPrefab;

    WeaponCameraFX cameraFX;

    //public Transform view;
    //public Transform parent;

    float deltaTime = 0;
    bool paused = false;

    [HideInInspector] public bool outOfAmmo = false;

    Animator anim;

    bool hasBullets;

    // Start is called before the first frame update
    void Start()
    {
        // if (view == null) view = transform.parent.parent;
        // if (parent == null) parent = transform.parent.parent;

        // startPosition = view.localPosition;
        // targetPosition = startPosition;

        if (transform.parent.parent.parent.localScale.x < 0) mouseButton = 1;
        else mouseButton = 0;

        anim = transform.GetComponentInParent<Animator>();

        hasBullets = GetComponentInChildren<ShootBullet>().enabled;

        // Transform sights = transform.Find("Sights");
        // if(sights != null) sightsPosition = sights.localPosition;

        cameraFX = sweatersController.instance.playerCamera.GetComponent<WeaponCameraFX>();

        anim.Play("catch");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (PauseSystem.paused) paused = true;

        if (mouseButton == 0 && (Input.GetButtonUp("LeftShoot")))
        {
            paused = false;
        }
        if (mouseButton == 1 && !PauseSystem.paused) paused = false;

        if (paused) deltaTime = 0;
        else deltaTime = Time.deltaTime;

        if (paused) return;

        //if (Input.GetMouseButtonDown(1)) ToggleDownSights();
        //if (Input.GetMouseButtonDown(0)) AnimateShoot();

        //if (Input.GetMouseButton(mouseButton) && aimTimer <= aimDelay) aimTimer += deltaTime;
        //if (!Input.GetMouseButton(mouseButton) && aimTimer >= 0) aimTimer -= deltaTime;

        // if (Input.GetKey(KeyCode.LeftShift) && aimTimer <= aimDelay) aimTimer += deltaTime;
        // if (!Input.GetKey(KeyCode.LeftShift) && aimTimer >= 0) aimTimer -= deltaTime;

        //if (Input.GetMouseButtonUp(mouseButton))
        //{
        //    //if(downSights) Invoke(nameof(ToggleDownSights), 0.1f);
        //    //aimTimer = 0;
        //    AnimateShoot();
        //}

        // view.localPosition += 50 * ((targetPosition - view.localPosition) / 4) * deltaTime;

        anim.SetBool("outOfAmmo", outOfAmmo);
        // Debug.Log(outOfAmmo);

        //if((targetPosition - view.localPosition).magnitude < 0.01 && !hasGun)
        //{
        //    view.localPosition = startPosition;
        //    Instantiate(hookShotPrefab, transform.parent);
        //    Destroy(gameObject);
        //}
        // we don't actually need to do that in this script, the shootBullet script can do that for us
        //view.LookAt(AcquireTarget.instance.target);

        // if (outOfAmmo) targetAngle = 45;

        // gunAngle += 50 * ((targetAngle - gunAngle) / 8) * deltaTime;
        // transform.localEulerAngles = new(gunAngle, 0, 0);

        if (!canShoot)
        {
            // targetAngle = 5;
            canShoot = true;
        }

        if (!hasGun) return;

        string[] throwButtons = { "LeftThrow", "RightThrow" };
        string throwButton = throwButtons[mouseButton];

        string[] shootButtons = { "LeftShoot", "RightShoot" };
        string shootButton = shootButtons[mouseButton];

        if (Input.GetButtonDown(throwButton) || (!hasBullets && Input.GetButtonDown(shootButton)))
        {
            // targetAngle = -45;
            // targetPosition = startPosition + new Vector3(0, 0.3f, -0.2f);
            canShoot = false;
            anim.Play("swap");
            Invoke(nameof(ThrowGun), 0.05f);
        }

        // stupid
        //string[] shootButtons = { "LeftShoot", "RightShoot" };
        //string shootButton = shootButtons[mouseButton];

        //if (info.damage == 69)
        //{
        //    if (Input.GetButtonDown(shootButton)) anim.Play("dwink");
        //}
        // else targetAngle = 0;

        //if (Input.GetKeyUp(mouseButton == 0 ? KeyCode.Q : KeyCode.E))
        //{
        //    CancelInvoke(nameof(ThrowGun));
        //    // targetPosition = startPosition;
        //}

        // if (Input.GetKeyUp(mouseButton == 0 ? KeyCode.Q : KeyCode.E)) ThrowGun();

        // if (aimTimer <= 0 && downSights) ToggleDownSights();
        // if (downSights && outOfAmmo) ToggleDownSights();

        // if (aimTimer >= aimDelay && !downSights && info.canAim && !outOfAmmo) ToggleDownSights();

        // if (downSights) cameraFX.RequestFOV(info.scopeFOV);
    }

    void ThrowGun()
    {
        ThrownGun gun = Instantiate(thrownGunPrefab, transform).GetComponent<ThrownGun>();
        gun.transform.parent = null;
        gun.transform.LookAt(AcquireTarget.instance.target);

        gun.ammo = GetComponentInChildren<ShootBullet>().ammo;

        Transform gunView = transform.Find("GunView");

        // gun.SetMesh(gunView.GetComponent<MeshFilter>().mesh, info.gunPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial);
        gun.info = info;

        gunView.gameObject.SetActive(false);
        hasGun = false;
        Invoke(nameof(SwapToHook), 0.7f);

        // Invoke(nameof(AnimateSwap), 0.5f);
        // targetPosition = startPosition + new Vector3(0, -1f, 0.5f);
    }

    public void SwapToHook()
    {
        Instantiate(hookShotPrefab, transform.parent);
        Destroy(gameObject);
        anim.SetBool("outOfAmmo", false);
    }

    public void ToggleDownSights()
    {
        //if (downSights) targetPosition = startPosition;
        //else
        //{
        //    //if (!cameraFX.ScopedIn())
        //    //{
        //        // targetPosition = new(-sightsPosition.x, -sightsPosition.y, startPosition.z);
        //        // targetPosition += new Vector3(startPosition.x / 5, startPosition.y / 5, 0);
        //    //}
        //    //else return;
        //}

        downSights = !downSights;
    }

    public void AnimateShoot()
    {
        // gunAngle -= info.recoil / 2;

        // view.localPosition += new Vector3(0, 0, -0.1f);

        if (info.damage == 69) anim.Play("dwink");
        else if (info.damage == 420) anim.Play("kisssss");

        else if (info.recoil > 0) anim.Play("shoot");
        else anim.Play("shootNoRecoil");
        
    }

    //public void AnimateSwap()
    //{
    //    anim.Play("swap");
    //}
}
