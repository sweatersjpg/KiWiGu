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
    
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.localPosition;
        targetPosition = startPosition;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ObstacleAvoidance();

        if (Input.GetMouseButtonDown(0) && hasHook) Throw();
        
        transform.localPosition += 50 * ((targetPosition - transform.localPosition) / 4) * Time.deltaTime;
    }
    void ObstacleAvoidance()
    {
        Vector3 origin = transform.parent.position + new Vector3(0, transform.localPosition.y, 0);
        Vector3 direction = (transform.position - origin).normalized;

        Debug.DrawRay(origin, direction);

        RaycastHit hit;

        bool hasHit = Physics.Raycast(origin, direction, out hit, direction.magnitude, ~LayerMask.GetMask("GunHand", "Player"));

        Vector3 offset = new(0, 0, 0);

        if (hasHit)
        {
            offset = hit.point - (origin + direction);

            targetPosition = transform.InverseTransformPoint(transform.TransformPoint(startPosition) + (hit.normal + new Vector3(0, -1, 0)) * offset.magnitude);

            return;
        }

        targetPosition = startPosition;

    }

    void Throw()
    {
        GameObject hook = Instantiate(hookPrefab);
        hook.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(transform.forward));

        hook.GetComponent<MoveHook>().home = this;

        hookView.SetActive(false);
        transform.localPosition += new Vector3(0, 0, 0.4f);
        startPosition += new Vector3(0, 0, 0.2f);

        hasHook = false;
    }

    public void CatchHook()
    {
        hookView.SetActive(true);

        transform.localPosition += new Vector3(0, 0, -0.4f);
        startPosition -= new Vector3(0, 0, 0.2f);

        hasHook = true;
    }
}
