using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;

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
    Ammunition caughtGunAmmo;

    [HideInInspector] public HookTarget hookTarget;
    public float playerDistance = 4;
    public float distToHook = 0;

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
            home.CatchHook(caughtGun, caughtGunAmmo);

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

        if(hookTarget)
        {
            hookTarget.resistance -= deltaTime;

            if (hookTarget.resistance > 0)
            {
                UpdateChain();

                sweatersController player = sweatersController.instance;

                Vector3 toPlayer = player.transform.position - transform.position;

                distToHook = Mathf.Min(toPlayer.magnitude, distToHook);

                float distance = Mathf.Max(distToHook, playerDistance);

                if (toPlayer.magnitude > distance)
                {
                    player.transform.position = transform.position + toPlayer.normalized * distance;

                    Vector3 normal = -toPlayer.normalized;
                    player.velocity -= Vector3.Project(player.velocity, normal);
                }

                return;
            } else
            {
                DamageEnemy(transform);
                transform.parent = null;

                TakeHookTarget();
                hookTarget = null;
                sweatersController.instance.isEncombered = false;
            }
        }

        speed += trackingAcceleration * deltaTime * 0.5f;
        velocity = velocity.normalized * Mathf.Abs(speed);
        transform.position += velocity * deltaTime;
        speed += trackingAcceleration * deltaTime * 0.5f;

        DoPhysics();
        UpdateChain();
    }

    void DamageEnemy(Transform t)
    {
        EnemyBase e = t.GetComponentInParent<EnemyBase>();

        if (e)
        {
            e.TakeDamage(99999);
        }
    }

    void DoPhysics()
    {
        // raycast from ppos to pos

        if(caughtGun == null) HookGun();

        bool hasHit = Physics.Raycast(pPosition, transform.position - pPosition, 
            out RaycastHit hit, (transform.position - pPosition).magnitude,
            ~LayerMask.GetMask("GunHand", "Player", "HookTarget", "TransparentFX"));

        if(hasHit)
        {
            ResolveCollision(hit);
            AddChainSegment(hit.point);
        }

    }

    void HookGun()
    {
        bool hasHit = Physics.SphereCast(pPosition, 1f, transform.position - pPosition,
            out RaycastHit hit, (transform.position - pPosition).magnitude, LayerMask.GetMask("HookTarget"));

        if (hasHit)
        {

            GameObject target = hit.transform.gameObject;
            HookTarget ht = target.transform.GetComponentInChildren<HookTarget>();

            if (ht == null)
            {
                caughtGun = target.transform.GetComponent<ThrownGun>().info;
                caughtGunAmmo = target.transform.GetComponent<ThrownGun>().ammo; // transfer ammo info
            }
            else
            {
                ht.resistance -= deltaTime;
                if (ht.resistance > 0)
                {
                    speed = 0.1f;
                    hookTarget = ht;

                    ht.gameObject.layer = LayerMask.NameToLayer("GunHand");

                    // sweatersController.instance.isEncombered = true;
                    distToHook = (sweatersController.instance.transform.position - ht.transform.position).magnitude;
                    transform.parent = ht.transform;
                    transform.localPosition = new();

                    return; // don't take gun
                }

                target = ht.gameObject;
                caughtGun = ht.info;
                caughtGunAmmo = new Ammunition(ht.info.capacity); // max capacity
            }

            target.transform.parent = transform;
            target.transform.localPosition = new();
            target.layer = LayerMask.NameToLayer("GunHand");

            if (target.transform.GetComponent<Rigidbody>() != null)
            {
                Destroy(target.transform.GetComponent<PhysicsHit>());
                Destroy(target.transform.GetComponent<Rigidbody>());
            }

            speed = 0;
        }
    }

    public void TakeThrownGun(GameObject target)
    {
        caughtGun = target.transform.GetComponent<ThrownGun>().info;
        caughtGunAmmo = target.transform.GetComponent<ThrownGun>().ammo;

        target.transform.parent = transform;
        target.transform.localPosition = new();
        target.layer = LayerMask.NameToLayer("GunHand");

        if (target.transform.GetComponent<Rigidbody>() != null)
        {
            Destroy(target.transform.GetComponent<PhysicsHit>());
            Destroy(target.transform.GetComponent<Rigidbody>());
        }

        speed = 0;
    }

    void TakeHookTarget()
    {
        GameObject target = hookTarget.gameObject;

        caughtGun = hookTarget.info;
        caughtGunAmmo = new Ammunition(caughtGun.capacity);

        target.transform.parent = transform;
        target.transform.localPosition = new();
        target.transform.gameObject.layer = LayerMask.NameToLayer("GunHand");

        if (target.transform.GetComponent<Rigidbody>() != null)
        {
            Destroy(target.transform.GetComponent<PhysicsHit>());
            Destroy(target.transform.GetComponent<Rigidbody>());
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
