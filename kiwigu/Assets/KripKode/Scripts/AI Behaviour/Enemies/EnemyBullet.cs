using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [HideInInspector] public float BulletDamage;

    [HideInInspector] public float BulletSpeed;
    [HideInInspector] public float BulletGravity = -9.8f;

    [SerializeField] GameObject TrailMesh;
    [SerializeField] GameObject bulletHolePrefab;
    [SerializeField] float BulletLifeTime = 5f;

    Vector3 velocity;
    float startTime;
    bool dead = false;

    private void Start()
    {
        startTime = Time.time;
        velocity = transform.forward * BulletSpeed;
        Destroy(gameObject, BulletLifeTime);
    }

    private void Update()
    {
        float time = Time.time - startTime;

        if (!dead) CastRay(time);

        if (time > BulletLifeTime) Destroy(gameObject);
    }

    void CastRay(float time)
    {
        Vector3 origin = EvaluateLocation(time - Time.deltaTime);
        Vector3 direction = EvaluateLocation(time) - origin;

        TrailMesh.transform.position = origin;

        int layerMask = ~(1 << LayerMask.NameToLayer("Enemy"));

        bool hasHit = Physics.Raycast(origin, direction, out RaycastHit hit, direction.magnitude, layerMask);

        if (hasHit)
        {
            if (hit.transform.gameObject.CompareTag("Player"))
            {
                Debug.Log("Hit Player");
            }
            else
            {
                Transform hole = Instantiate(bulletHolePrefab).transform;
                hole.SetPositionAndRotation(hit.point, Quaternion.LookRotation(-hit.normal));
                hole.parent = hit.transform;
            }
            BulletLifeTime = Time.time - startTime + 0.5f;
            dead = true;
        }
    }

    Vector3 EvaluateLocation(float time)
    {
        float x = velocity.x * time;
        float y = velocity.y * time + 0.5f * BulletGravity * time * time;
        float z = velocity.z * time;

        return new Vector3(x, y, z) + transform.position;
    }
}
