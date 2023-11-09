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

    Vector3 targetPosition;

    Vector3 startPosition;
    Vector3 homePosition;

    Transform view;

    GameObject hook;
    
    // Start is called before the first frame update
    void Start()
    {
        view = transform.parent.parent;

        startPosition = view.localPosition;
        homePosition = startPosition;
        targetPosition = startPosition;

        view.localPosition += new Vector3(0, -1, -0.2f);

        if (view.localPosition.x > 0) mouseButton = 1;
        else mouseButton = 0;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // ObstacleAvoidance();

        if (Input.GetMouseButtonDown(mouseButton) || Input.GetKeyDown(mouseButton == 0 ? KeyCode.Q : KeyCode.E))
        {
            if (hasHook) Throw();
            else hook.GetComponent<MoveHook>().Pullback();
        }

        view.localPosition += 50 * ((targetPosition - view.localPosition) / 4) * Time.deltaTime;
    }

    //void ObstacleAvoidance()
    //{
    //    Vector3 origin = transform.parent.position + new Vector3(0, transform.localPosition.y, 0);
    //    Vector3 direction = (transform.position - origin).normalized;

    //    Debug.DrawRay(origin, direction);

    //    RaycastHit hit;

    //    bool hasHit = Physics.Raycast(origin, direction, out hit, direction.magnitude, ~LayerMask.GetMask("GunHand", "Player"));

    //    Vector3 offset = new(0, 0, 0);

    //    if (hasHit)
    //    {
    //        offset = hit.point - (origin + direction);

    //        targetPosition = transform.InverseTransformPoint(transform.TransformPoint(startPosition) + (hit.normal + new Vector3(0, -1, 0)) * offset.magnitude);

    //        return;
    //    }

    //    targetPosition = homePosition;

    //}

    void Throw()
    {
        hook = Instantiate(hookPrefab);
        hook.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(transform.forward));
        hook.transform.LookAt(AcquireTarget.instance.GetHookTarget());

        hook.GetComponent<MoveHook>().home = this;

        hookView.SetActive(false);
        view.localPosition += new Vector3(0, 0, 0.4f);
        homePosition = startPosition + new Vector3(0, 0, 0.2f);

        hasHook = false;
    }

    public void CatchHook(GunInfo info, Ammunition ammo)
    {
        CancelInvoke();
        hookView.SetActive(true);

        view.localPosition += new Vector3(0, 0, -0.4f);
        targetPosition = startPosition;

        hasHook = true;

        if(info != null)
        {
            ShootBullet gun = Instantiate(info.gunPrefab, transform.parent).GetComponentInChildren<ShootBullet>();

            gun.ammo = new Ammunition(ammo.capacity);
            gun.ammo.count = ammo.count;

            Destroy(gameObject);

            view.localPosition = startPosition;
        }
    }

    public void PullBack()
    {
        CancelInvoke();
        targetPosition = startPosition - new Vector3(0, 0, 0.5f);
        Invoke(nameof(Reach), 0.5f);
    }

    public void Reach()
    {
        targetPosition = startPosition + new Vector3(0, 0, 0.2f);
    }
}
