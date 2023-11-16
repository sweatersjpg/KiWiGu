using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bullet : MonoBehaviour
{
    [Header("Damage")]
    public float bulletDamage = 5;

    [Header("Metrics")]
    public float speed = 370;
    public float gravity = -9.8f;
    public float acceleration = 0;

    [Space]
    public float radius = 0.2f;

    [Header("Tracking")]
    public bool trackTarget = false;
    public float trackingRadius = 2;
    public float trackingSpeed = 2;
    Transform target;
    Transform ogTarget;
    Vector3 ogTargetPosition;

    [Space]
    public float lifeTime = 3;

    public GameObject bulletMesh;
    public GameObject bulletHolePrefab;

    [Space]
    public GameObject sparksPrefab;

    public GameObject[] spawnOnHit;

    bool dead = false;

    float startTime;

    [HideInInspector] public float charge;

    [HideInInspector] public LayerMask ignoreMask;
    [HideInInspector] public bool fromEnemy = false;

    [Space] public bool justInfo = false;

    // Start is called before the first frame update
    void Start()
    {
        // velocity = transform.forward * speed;
        startTime = Time.time;

        if(trackTarget) target = AcquireTarget.instance.GetBulletTarget();
        if (target != null) ogTarget = target;
        
        ogTargetPosition = AcquireTarget.instance.target;
    }

    // Update is called once per frame
    void Update()
    {
        if (justInfo) return;
        
        float time = Time.time - startTime;

        // if (target != null) transform.LookAt(target);

        if (ogTarget != null) ogTargetPosition = ogTarget.position;

        if (trackTarget && time > 0.2f)
        {
            Collider[] hits = Physics.OverlapSphere(bulletMesh.transform.position, trackingRadius,
                LayerMask.GetMask("Enemy", "PhysicsObject"));

            //if (hits.Length > 0)
            //{
            //    target = hits[0].transform;
            //}
            Transform closest = null;

            foreach (Collider c in hits)
            {
                if (closest == null
                    || Vector3.Distance(c.transform.position, ogTargetPosition) > Vector3.Distance(closest.position, ogTargetPosition))
                {
                    closest = c.transform;
                }
            }

            if (closest != null) target = closest;
            else target = ogTarget;
        }

        if (target != null && !dead)
        {
            Vector3 tpos = target.position;

            tpos = (tpos - transform.position).normalized * 
                (bulletMesh.transform.position - transform.position).magnitude + transform.position;

            Vector3 dir = (tpos - bulletMesh.transform.position);
            float delta = Mathf.Min(dir.magnitude, trackingSpeed * Time.deltaTime);

            dir = delta * dir.normalized;

            transform.LookAt(bulletMesh.transform.position + dir);

        }

        if (!dead)
        {
            CastRay(time);
        }

        if (time > lifeTime) Destroy(gameObject);
    }

    void CastRay(float time)
    {
        Vector3 origin = EvaluateLocation(time - Time.deltaTime);
        Vector3 direction = EvaluateLocation(time) - origin;

        bulletMesh.transform.position = origin;

        bool hasHit = Physics.SphereCast(origin, radius, direction, out RaycastHit hit, direction.magnitude,
            LayerMask.GetMask("Enemy", "PhysicsObject"));

        if (fromEnemy) hasHit = false;

        if(hasHit)
        {
            DoHit(hit, direction);
        } else
        {
            bool hasHitTwoElectricBoogaloo = Physics.Raycast(origin, direction, out RaycastHit hitTwo, direction.magnitude,
                ignoreMask);

            if (hasHitTwoElectricBoogaloo)
            {
                DoHit(hitTwo, direction);
            }
        }
    }

    //void CastRay(float time, float radius, LayerMask mask)
    //{
        
    //    Vector3 origin = EvaluateLocation(time - Time.deltaTime);
    //    Vector3 direction = EvaluateLocation(time) - origin;

    //    bulletMesh.transform.position = origin;

    //    bool hasHit = Physics.SphereCast(origin, radius, direction, out RaycastHit hit, direction.magnitude, 
    //        mask);

    //    if (hasHit)
    //    {
    //        DoHit(hit, direction);
    //    }
    //}

    void DoHit(RaycastHit hit, Vector3 direction)
    {
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("EnergyWall"))
        {
            if(Vector3.Dot(hit.transform.right, direction) > 0) return;
        }
        else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            hit.transform.GetComponent<PlayerHealth>().DealDamage(bulletDamage, -direction);
        }
        else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            EnemyBase enemy = hit.transform.gameObject.GetComponentInChildren<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(bulletDamage);
            }
        }
        else if (hit.transform.gameObject.CompareTag("RigidTarget"))
        {
            hit.transform.gameObject.GetComponent<PhysicsHit>().Hit(hit.point, transform.forward * speed);

            SpawnHole(hit);
        }
        else
        {
            // Debug.Log(hit.transform.name);

            SpawnHole(hit);
        }

        if (sparksPrefab != null) SpawnSparks(hit, direction);
        bulletMesh.transform.position = hit.point;

        foreach (GameObject s in spawnOnHit)
        {
            GameObject o = Instantiate(s);
            o.transform.position = hit.point;
        }

        //Destroy(gameObject);
        lifeTime = Time.time - startTime + 0.5f;

        MeshRenderer view = bulletMesh.GetComponentInChildren<MeshRenderer>();
        if(view != null) Destroy(view.gameObject);
        // bulletMesh.SetActive(false);
        dead = true;
    }

    void SpawnHole(RaycastHit hit)
    {
        if (bulletHolePrefab == null) return;
        
        Transform hole = Instantiate(bulletHolePrefab).transform;
        hole.SetPositionAndRotation(hit.point, Quaternion.LookRotation(-hit.normal));
        hole.parent = hit.transform;
    }

    void SpawnSparks(RaycastHit hit, Vector3 direction)
    {
        Vector3 d = direction;
        Vector3 n = hit.normal;

        Vector3 r = d - 2 * Vector3.Dot(d, n) * n;

        Vector3 facing = r;

        Transform sparks = Instantiate(sparksPrefab).transform;
        sparks.SetPositionAndRotation(hit.point, Quaternion.LookRotation(facing));
    }

    Vector3 EvaluateLocation(float time)
    {
        // y = v * t + 0.5 * gravity * t * t

        Vector3 velocity = transform.forward * speed;
        Vector3 acc = transform.forward * acceleration;

        float x = velocity.x * time + 0.5f * acc.x * time * time;
        float y = velocity.y * time + (0.5f * gravity * time * time) + (0.5f * acc.y * time * time);
        float z = velocity.z * time + 0.5f * acc.z * time * time;

        return new Vector3(x, y, z) + transform.position;
    }
}
