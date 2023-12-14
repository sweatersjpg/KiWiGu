using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

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
    [Space]
    public GameObject[] HitFX;
    public LayerMask[] HitFXLayers;

    public GameObject[] spawnOnHit;

    public bool dead = false;

    float startTime;
    float time;

    [HideInInspector] public float charge;

    [HideInInspector] public LayerMask ignoreMask;
    [HideInInspector] public bool fromEnemy = false;

    [Space] public bool justInfo = false;
    public GameObject rbObject;
    public float rbForce = 10;

    // Start is called before the first frame update
    void Start()
    {
        // velocity = transform.forward * speed;
        startTime = Time.time;

        if (trackTarget) target = AcquireTarget.instance.GetBulletTarget();
        if (target != null) ogTarget = target;

        ogTargetPosition = AcquireTarget.instance.target;

        if (justInfo)
        {
            GameObject o = Instantiate(rbObject, transform);
            o.transform.parent = null;

            Vector3 forceDirection = o.transform.forward;

            Rigidbody rb = o.GetComponent<Rigidbody>();

            if (!fromEnemy)
            {
                Vector3 vel = new(sweatersController.instance.velocity.x, 0, sweatersController.instance.velocity.z);

                rb.AddForce(vel / 2, ForceMode.VelocityChange);
            }
            rb.AddForce(forceDirection * 10f, ForceMode.Impulse);

            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseSystem.paused)
        {
            return;
        }

        time += Time.deltaTime;

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

        if (hasHit)
        {
            DoHit(hit, direction);
        }
        else
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

    private void ApplyDamage(EnemyHitBox enemy, float damageMultiplier)
    {
        if (enemy == null)
            return;

        var scriptType = System.Type.GetType(enemy.ReferenceScript);

        Transform rootParent = GetRootParent(enemy.transform);

        if (rootParent != null && scriptType != null)
        {
            var enemyComponent = rootParent.GetComponent(scriptType) as MonoBehaviour;

            if (enemyComponent != null) 
            {
                var takeDamageMethod = scriptType.GetMethod("TakeDamage");

                if (takeDamageMethod != null)
                {
                    takeDamageMethod.Invoke(enemyComponent, new object[] { bulletDamage * damageMultiplier });
                }
            }
        }
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

    void DoHit(RaycastHit hit, Vector3 direction)
    {
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("EnergyWall"))
        {
            if (Vector3.Dot(hit.transform.right, direction) > 0) return;
        }
        else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            hit.transform.GetComponent<PlayerHealth>().DealDamage(bulletDamage, -direction);
        }
        else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            EnemyHitBox enemy = hit.transform.gameObject.GetComponent<EnemyHitBox>();

            if (enemy != null)
            {
                if (enemy.doubleDamage)
                    ApplyDamage(enemy, 2f);
                else if (enemy.lessDamage)
                    ApplyDamage(enemy, 1.5f);
                else if (enemy.leastDamage)
                    ApplyDamage(enemy, 0.5f);
                else
                    ApplyDamage(enemy, 1f);
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

        if (sparksPrefab != null || HitFX.Length > 0)
        {
            if (!SpawnSpecialHitFX(hit, direction)) SpawnHitFX(hit, direction, sparksPrefab);
        }

        bulletMesh.transform.position = hit.point;

        foreach (GameObject s in spawnOnHit)
        {
            GameObject o = Instantiate(s);
            o.transform.position = hit.point;
        }

        //Destroy(gameObject);
        lifeTime = Time.time - startTime + 0.5f;

        MeshRenderer view = bulletMesh.GetComponentInChildren<MeshRenderer>();
        if (view != null) Destroy(view.gameObject);
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

    void SpawnHitFX(RaycastHit hit, Vector3 direction, GameObject prefab)
    {
        Vector3 d = direction;
        Vector3 n = hit.normal;

        Vector3 r = d - 2 * Vector3.Dot(d, n) * n;

        Vector3 facing = r;

        Transform sparks = Instantiate(prefab).transform;
        sparks.SetPositionAndRotation(hit.point, Quaternion.LookRotation(facing));
    }

    bool SpawnSpecialHitFX(RaycastHit hit, Vector3 direction)
    {
        for(int i = 0; i < HitFX.Length; i++)
        {
            if (HitFXLayers[i] != (HitFXLayers[i] | (1 << hit.transform.gameObject.layer))) continue;

            SpawnHitFX(hit, direction, HitFX[i]);

            return true;
        }

        return false;
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
