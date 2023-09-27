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
    
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.parent.localPosition;
        homePosition = startPosition;
        targetPosition = startPosition;

        transform.parent.localPosition += new Vector3(0, -1, -0.2f);

        if (transform.parent.localPosition.x > 0) mouseButton = 1;
        else mouseButton = 0;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // ObstacleAvoidance();

        if (Input.GetMouseButtonDown(mouseButton) || Input.GetKeyDown(mouseButton == 0 ? KeyCode.Q : KeyCode.E)
            && hasHook) Throw();
        
        transform.parent.localPosition += 50 * ((targetPosition - transform.parent.localPosition) / 4) * Time.deltaTime;
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
        GameObject hook = Instantiate(hookPrefab);
        hook.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(transform.forward));
        hook.transform.LookAt(AcquireTarget.instance.target);

        hook.GetComponent<MoveHook>().home = this;

        hookView.SetActive(false);
        transform.parent.localPosition += new Vector3(0, 0, 0.4f);
        homePosition = startPosition + new Vector3(0, 0, 0.2f);

        hasHook = false;
    }

    public void CatchHook(GunInfo info)
    {
        hookView.SetActive(true);

        transform.parent.localPosition += new Vector3(0, 0, -0.4f);
        homePosition = startPosition;

        hasHook = true;

        if(info != null)
        {
            Instantiate(info.gunPrefab, transform.parent);
            Destroy(gameObject);

            transform.parent.localPosition = startPosition;
        }
    }

    public void PullBack()
    {
        homePosition = startPosition - new Vector3(0, 0, 0.5f);
    }
}
