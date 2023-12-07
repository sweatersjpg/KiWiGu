using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ThrowHook : MonoBehaviour
{
    public GameObject hookPrefab;

    public GameObject hookView;

    public int mouseButton = 0;

    bool hasHook = true;

    //Vector3 targetPosition;

    //Vector3 startPosition;
    //Vector3 homePosition;

    //Transform view;

    GameObject hook;

    Animator anim;

    bool keyPressed = false;
    
    // Start is called before the first frame update
    void Start()
    {
        // view = transform.parent.parent;

        //startPosition = view.localPosition;
        //homePosition = startPosition;
        //targetPosition = startPosition;

        // view.localPosition += new Vector3(0, -1, -0.2f);

        //if (view.localPosition.x > 0) mouseButton = 1;
        //else mouseButton = 0;
        anim = GetComponentInParent<Animator>();

        if (transform.parent.parent.parent.localScale.x < 0) mouseButton = 1;
        else mouseButton = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !hasHook && !sweatersController.instance.wasGrounded)
        {
            MoveHook mh = hook.GetComponent<MoveHook>();
            if (mh.hookTarget != null && mh.hookTarget.tether) hook.GetComponent<MoveHook>().PullbackWithForce(30);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // ObstacleAvoidance();

        if (Input.GetMouseButton(mouseButton) || Input.GetKey(mouseButton == 0 ? KeyCode.Q : KeyCode.E))
        {
            if (hasHook && !keyPressed)
            {
                keyPressed = true;
                Invoke(nameof(Throw), 0.05f);
                anim.Play("throw");
            }
            // else hook.GetComponent<MoveHook>().PullbackWithForce(0);
        }

        if (Input.GetMouseButtonUp(mouseButton) || Input.GetKeyUp(mouseButton == 0 ? KeyCode.Q : KeyCode.E))
        {
            if (!hasHook) hook.GetComponent<MoveHook>().PullbackWithForce(0);
            else CancelInvoke(nameof(Throw));
            keyPressed = false;
        }

        // view.localPosition += 50 * ((targetPosition - view.localPosition) / 4) * Time.deltaTime;

    }

    void Throw()
    {
        hook = Instantiate(hookPrefab);
        hook.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(transform.forward));
        hook.transform.LookAt(AcquireTarget.instance.GetHookTarget());

        hook.GetComponent<MoveHook>().home = this;

        hookView.SetActive(false);
        //view.localPosition += new Vector3(0, 0, 0.4f);
        //homePosition = startPosition + new Vector3(0, 0, 0.2f);

        hasHook = false;
    }

    public void AnimateCatch()
    {
        anim.Play("catch");
    }

    public void CatchHook(GunInfo info, Ammunition ammo)
    {
        CancelInvoke();
        hookView.SetActive(true);

        //view.localPosition += new Vector3(0, 0, -0.4f);
        //targetPosition = startPosition;

        hasHook = true;

        if(info != null)
        {
            ShootBullet gun = Instantiate(info.gunPrefab, transform.parent).GetComponentInChildren<ShootBullet>();

            gun.ammo = new Ammunition(ammo.capacity);
            gun.ammo.count = ammo.count;

            Destroy(gameObject);

            //view.localPosition = startPosition;
        }
    }

    public void PullBack()
    {
        //CancelInvoke();
        //targetPosition = startPosition - new Vector3(0, 0, 0.5f);
        //Invoke(nameof(Reach), 0.5f);
        anim.Play("pull");
    }

    public void Reach()
    {
        // targetPosition = startPosition + new Vector3(0, 0, 0.2f);
    }
}
