using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcquireTarget : MonoBehaviour
{
    public static AcquireTarget instance;
    public Vector3 target;

    public float maxDistance = 50;
    public float minDistance = 3;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        bool hasHit = Physics.Raycast(transform.position, transform.forward,
            out RaycastHit hit, maxDistance, ~LayerMask.GetMask("GunHand", "Player", "HookTarget"));

        if(hasHit)
        {
            target = hit.point;
            if((target - transform.position).magnitude < minDistance)
            {
                target = transform.position + transform.forward * minDistance;
            }
        } else target = transform.position + transform.forward * maxDistance;

    }
}
