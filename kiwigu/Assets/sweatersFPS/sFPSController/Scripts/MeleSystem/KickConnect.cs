using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickConnect : MonoBehaviour
{
    public Transform attackLocation;
    public Transform leg;

    float targetDistance;

    // Start is called before the first frame update
    void Start()
    {
        targetDistance = attackLocation.localPosition.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        bool hasHit = Physics.Raycast(transform.position, (attackLocation.position - transform.position), out RaycastHit hit, targetDistance,
            ~LayerMask.GetMask("GunHand", "Player", "HookTarget", "TransparentFX", "HookShot", "DialogTrigger"));

        Debug.DrawRay(transform.position, (attackLocation.position - transform.position));

        if(hasHit)
        {
            attackLocation.position = hit.point;
            leg.position = transform.position + GetOffset(hit.point);
        } else
        {
            attackLocation.localPosition = attackLocation.localPosition.normalized * targetDistance;
            leg.localPosition = new();
        }
    }

    Vector3 GetOffset(Vector3 hitPoint)
    {
        Vector3 direction = attackLocation.position - transform.position;

        Vector3 startPoint = transform.position + direction.normalized * targetDistance;

        return hitPoint - startPoint;
    }
}
