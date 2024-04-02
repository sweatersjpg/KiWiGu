using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class AcquireTarget : MonoBehaviour
{
    public static AcquireTarget instance;
    public Vector3 target;

    public float maxDistance = 50;
    public float minDistance = 3;

    public float radius = 0.2f;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        // finer target including walls
        Vector3 target1 = GetTarget(0.01f, ~LayerMask.GetMask("GunHand", "Player", "HookTarget", "EnergyWall", "BulletView"));

        // wider target including enemies / PhysicsObjects
        Vector3 target2 = GetTarget(radius, LayerMask.GetMask("Enemy", "PhysicsObject"));

        if ((transform.position - target1).magnitude < (transform.position - target2).magnitude)
        {
            target = target1;
        }
        else target = target2;
    }

    Vector3 GetTarget(float radius, LayerMask mask)
    {
        bool hasHit = Physics.SphereCast(transform.position, radius, transform.forward,
            out RaycastHit hit, maxDistance, mask);

        Vector3 target;
        
        if (hasHit)
        {
            target = hit.point;
            if ((target - transform.position).magnitude < minDistance)
            {
                target = transform.position + transform.forward * minDistance;
            }

            return target;
        }
        else target = transform.position + transform.forward * maxDistance;

        return target;
    }

    public Transform GetBulletTarget()
    {
        bool hasHit = Physics.SphereCast(transform.position, radius * 2, transform.forward,
            out RaycastHit hit, maxDistance, LayerMask.GetMask("Enemy", "PhysicsObject"));

        if(hasHit)
        {
            MeshRenderer mr = hit.transform.GetComponentInChildren<MeshRenderer>();

            if (mr != null) return mr.transform;
        }

        return null;
    }

    //public Vector3 GetHookTarget()
    //{
    //    bool hasHit = Physics.SphereCast(transform.position, radius*radius, transform.forward,
    //        out RaycastHit hit, maxDistance, ~LayerMask.GetMask("GunHand", "Player", "BulletView"));

    //    if(hasHit)
    //    {
    //        HookTarget ht = hit.transform.GetComponentInChildren<HookTarget>();

    //        Vector3 target = hit.point;

    //        if (ht) target = ht.transform.position;

    //        if ((target - transform.position).magnitude < minDistance)
    //        {
    //            target = transform.position + (target - transform.position).normalized * minDistance;
    //        }

    //        return target;
    //    }

    //    return transform.position + transform.forward * maxDistance;
    //}

    public Vector3 GetJustHookTarget(out HookTarget hookTarget)
    {
        bool hasHit = Physics.SphereCast(Camera.main.transform.position, radius * radius, Camera.main.transform.forward,
            out RaycastHit hit, maxDistance, ~LayerMask.GetMask("GunHand", "Player", "Shield", "BulletView"));

        hookTarget = null;

        if (hasHit)
        {
            Transform targetTransform;
             
            if (hit.transform.GetComponent<HookTarget>() == null)
                targetTransform = GetRootParent(hit.transform);
            else
                targetTransform = hit.transform;

            // if(!targetTransform.CompareTag("Enemy")) targetTransform = hit.transform;
            
            HookTarget[] hts = targetTransform.GetComponentsInChildren<HookTarget>();
            HookTarget ht = hts.Length > 0 ? hts[0] : null;

            for(int i = 0; i < hts.Length; i++)
            {
                if (!hts[i].blockSteal) ht = hts[i];
            }

            if(ht && Vector3.Distance(ht.transform.position, hit.point) > 4) ht = null;

            Vector3 target = hit.point;

            if (ht)
            {
                target = ht.transform.position;
                hookTarget = ht;
            }
            else target = transform.position + transform.forward * maxDistance;

            if ((target - transform.position).magnitude < minDistance)
            {
                target = transform.position + (target - transform.position).normalized * minDistance;
            }

            return target;
        }

        return transform.position + transform.forward * maxDistance;
    }
    private Transform GetRootParent(Transform child)
    {
        Transform parent = child.parent;

        while (parent != null)
        {
            child = parent;
            parent = child.parent;
        }

        return child;
    }
}
