using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class MoveHook : MonoBehaviour
{
    // Start is called before the first frame update

    [HideInInspector] public ThrowHook home;

    public LineRenderer chain;

    //public float startingSpeed = 8;
    public float hookRange = 5;
    public float trackingAcceleration = 6;
    // public float deceleration = 32;

    public float catchDistance = 0.2f;

    // bool canCatch = false;

    GunInfo caughtGun;

    bool headingBack = false;

    float speed;
    [HideInInspector] public Vector3 velocity;

    float deltaTime = 0;

    Vector3 pPosition; // past position

    float chainPointTimer;
    public float chainSegmentSize = 0.5f;

    void Start()
    {
        float startingSpeed = Mathf.Sqrt(2 * trackingAcceleration * hookRange);

        velocity = transform.forward * startingSpeed;

        Vector3 v = sweatersController.instance.velocity;
        v.y = 0;

        velocity += v;

        speed = -velocity.magnitude;

    }

    // Update is called once per frame
    void Update()
    {
        if (PauseSystem.paused)
        {
            deltaTime = 0;
            return;
        }
        else deltaTime = Time.deltaTime;

        pPosition = transform.position;

        Vector3 heading = home.transform.position - transform.position;

        //if(canCatch && heading.magnitude < catchDistance)
        //{
        //    home.CatchHook(caughtGun);

        //    Destroy(gameObject);
        //    return;
        //} else if(heading.magnitude > catchDistance) {
        //    canCatch = true;
        //}
        if(headingBack && heading.magnitude < catchDistance)
        {
            home.CatchHook(caughtGun);

            Destroy(gameObject);
            return;
        }

        if (speed > 0)
        {
            velocity = heading.normalized;
            if(!headingBack)
            {
                headingBack = true;
                home.PullBack();
            }
        }

        speed += trackingAcceleration * deltaTime * 0.5f;
        velocity = velocity.normalized * Mathf.Abs(speed);
        transform.position += velocity * deltaTime;
        speed += trackingAcceleration * deltaTime * 0.5f;

        DoPhysics();
        UpdateChain();
    }

    void DoPhysics()
    {
        // raycast from ppos to pos

        if(caughtGun == null) HookGun();

        bool hasHit = Physics.SphereCast(pPosition, 0.25f, transform.position - pPosition, 
            out RaycastHit hit, (transform.position - pPosition).magnitude, ~LayerMask.GetMask("GunHand", "Player", "HookTarget"));

        if(hasHit)
        {
            ResolveCollision(hit);
            AddChainSegment(hit.point);
        }

    }

    void HookGun()
    {
        bool hasHit = Physics.SphereCast(pPosition, 0.25f, transform.position - pPosition,
            out RaycastHit hit, (transform.position - pPosition).magnitude, LayerMask.GetMask("HookTarget"));

        if (hasHit)
        {
            //Debug.Log(hit.transform.name);

            //HookTarget ht = hit.transform.GetComponent<HookTarget>();
            //if (ht == null) ht = hit.transform.GetComponentInChildren<HookTarget>();
            //if (ht == null) caughtGun = hit.transform.GetComponent<ThrownGun>().info;
            //if(ht != null) caughtGun = ht.info;

            //hit.transform.parent = transform;
            //hit.transform.localPosition = new();

            //hit.transform.tag = "Untagged";
            //hit.transform.gameObject.layer = LayerMask.NameToLayer("GunHand");

            //if (hit.rigidbody)
            //{
            //    Destroy(hit.transform.GetComponent<PhysicsHit>());
            //    Destroy(hit.transform.GetComponent<Rigidbody>());
            //}

            GameObject target = hit.transform.gameObject;
            HookTarget ht = target.transform.GetComponentInChildren<HookTarget>();

            if (ht == null) caughtGun = target.transform.GetComponent<ThrownGun>().info;
            else
            {
                target = ht.gameObject;
                caughtGun = ht.info;
            }

            target.transform.parent = transform;
            target.transform.localPosition = new();
            target.transform.gameObject.layer = LayerMask.NameToLayer("GunHand");

            if (target.transform.GetComponent<Rigidbody>() != null)
            {
                Destroy(target.transform.GetComponent<PhysicsHit>());
                Destroy(target.transform.GetComponent<Rigidbody>());
            }

            speed = 0;

            return;
        }
    }

    void ResolveCollision(RaycastHit hit)
    {
        
        if (hit.transform.gameObject.CompareTag("RigidTarget"))
        {
            hit.transform.gameObject.GetComponent<PhysicsHit>().Hit(hit.point, velocity);
        }

        if (headingBack) return;

        speed /= 2;

        transform.position = hit.point;

        Vector3 d = velocity;
        Vector3 n = hit.normal;

        Vector3 r = d - 2 * Vector3.Dot(d, n) * n;

        velocity = r;
    }

    void UpdateChain()
    {

        //chainPointTimer += deltaTime;
        //if(!headingBack && chainPointTimer > timeBetweenChainNodes)
        //{
        //    chainPointTimer = 0;
        //    AddChainSegment(transform.position + Random.insideUnitSphere * 0.1f - velocity.normalized * 0.1f);
        //}
        if(!headingBack && (chain.GetPosition(0) - chain.GetPosition(1)).magnitude > chainSegmentSize)
        {
            AddChainSegment(transform.position + Random.insideUnitSphere * 0.1f - velocity.normalized * 0.1f);
        }
        
        chain.SetPosition(chain.positionCount - 1, home.transform.position);
        chain.SetPosition(0, transform.position);

        for(int i = 1; i < chain.positionCount-1; i++)
        {
            Vector3 p = chain.GetPosition(i);

            Vector3 r = home.transform.position;
            Vector3 h = transform.position;

            Vector3 d = r - h;
            //Vector3 v = Vector3.Project(p - h, r - h) + h;
            Vector3 v = (d.magnitude / chain.positionCount) * i * d.normalized + h;

            p += 50 * deltaTime * ((v - p) / (headingBack ? 2 : 8));

            chain.SetPosition(i, p);
        }
    }

    void AddChainSegment(Vector3 pos)
    {
        chain.positionCount++;

        // shift positions down
        for(int i = chain.positionCount-2; i >= 1; i--)
        {
            chain.SetPosition(i + 1, chain.GetPosition(i));
        }

        chain.SetPosition(1, pos);
    }
}
