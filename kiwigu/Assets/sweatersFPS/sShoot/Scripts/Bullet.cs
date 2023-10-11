using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Damage")]
    public float bulletDamage = 5;

    public float speed = 370;
    public float gravity = -9.8f;

    public float acceleration = 0;

    public float lifeTime = 3;

    public GameObject bulletMesh;
    public GameObject bulletHolePrefab;

    [Space]
    public GameObject sparksPrefab;

    bool dead = false;

    Vector3 velocity;

    float startTime;

    // Start is called before the first frame update
    void Start()
    {
        velocity = transform.forward * speed;
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.time - startTime;

        if(!dead) CastRay(time);

        if (time > lifeTime) Destroy(gameObject);
    }

    void CastRay(float time)
    {
        
        Vector3 origin = EvaluateLocation(time - Time.deltaTime);
        Vector3 direction = EvaluateLocation(time) - origin;

        bulletMesh.transform.position = origin;

        bool hasHit = Physics.Raycast(origin, direction, out RaycastHit hit, direction.magnitude, 
            ~LayerMask.GetMask("GunHand", "Player", "HookTarget"));

        if (hasHit)
        {
            SpawnSparks(hit, direction);
            bulletMesh.transform.position = hit.point;

            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                EnemyBase enemy = hit.transform.gameObject.GetComponentInChildren<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(bulletDamage);
                }
            }
            else if(hit.transform.gameObject.CompareTag("RigidTarget"))
            {
                hit.transform.gameObject.GetComponent<PhysicsHit>().Hit(hit.point, velocity);

                Transform hole = Instantiate(bulletHolePrefab).transform;
                hole.SetPositionAndRotation(hit.point, Quaternion.LookRotation(-hit.normal));
                hole.parent = hit.transform;
            }
            else
            {
                // Debug.Log(hit.transform.name);

                Transform hole = Instantiate(bulletHolePrefab).transform;
                hole.SetPositionAndRotation(hit.point, Quaternion.LookRotation(-hit.normal));
                hole.parent = hit.transform;
            }

            //Destroy(gameObject);
            lifeTime = Time.time - startTime + 0.5f;

            Destroy(bulletMesh.GetComponentInChildren<MeshRenderer>().gameObject);
            // bulletMesh.SetActive(false);
            dead = true;
        }
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

        Vector3 acc = transform.forward * acceleration;

        float x = velocity.x * time + 0.5f * acc.x * time * time;
        float y = velocity.y * time + (0.5f * gravity * time * time) + (0.5f * acc.y * time * time);
        float z = velocity.z * time + 0.5f * acc.z * time * time;

        return new Vector3(x, y, z) + transform.position;
    }
}
