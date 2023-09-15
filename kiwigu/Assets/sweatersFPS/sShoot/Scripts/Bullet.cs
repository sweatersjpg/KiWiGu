using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public float speed = 370;
    public float gravity = -9.8f;

    public float lifeTime = 3;

    public GameObject bulletMesh;
    public GameObject bulletHolePrefab;

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

        CastRay(time);

        if (time > 3) Destroy(gameObject);
    }

    void CastRay(float time)
    {
        
        Vector3 origin = EvaluateLocation(time - Time.deltaTime);
        Vector3 direction = EvaluateLocation(time) - origin;

        bulletMesh.transform.position = origin;

        bool hasHit = Physics.Raycast(origin, direction, out RaycastHit hit, direction.magnitude, ~LayerMask.GetMask("GunHand", "Player"));

        if (hasHit)
        {
            Debug.Log(hit.transform.name);

            Transform hole = Instantiate(bulletHolePrefab).transform;
            hole.SetPositionAndRotation(hit.point, Quaternion.LookRotation(-hit.normal));

            Destroy(gameObject);
        }
    }

    Vector3 EvaluateLocation(float time)
    {
        // y = v * t + 0.5 * gravity * t * t

        float x = velocity.x * time;
        float y = velocity.y * time + 0.5f * gravity * time * time;
        float z = velocity.z * time;

        return new Vector3(x, y, z) + transform.position;
    }
}
