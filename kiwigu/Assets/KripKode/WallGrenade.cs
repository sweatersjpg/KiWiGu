using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WallGrenade : MonoBehaviour
{
    private Rigidbody rb;

    public GameObject wallPrefab;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (rb != null)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Vector3 collisionNormal = collision.contacts[0].normal;

        float angleThreshold = 5f;
        float angle = Vector3.Angle(Vector3.up, collisionNormal);

        if (angle < angleThreshold)
        {
            SpawnWall();
            Destroy(gameObject);
        }
    }

    void SpawnWall()
    {
        GameObject wall = Instantiate(wallPrefab, transform.position, Quaternion.identity);

        // Get the rotation of the grenade
        Quaternion grenadeRotation = transform.rotation;

        // Set the rotation of the wall to face the grenade's x-axis
        wall.transform.rotation = Quaternion.Euler(0f, grenadeRotation.eulerAngles.y - 90, 0f);

    }
}
