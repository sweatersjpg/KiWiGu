using UnityEngine;

public class WallGrenade : MonoBehaviour
{
    private Rigidbody rb;
    public GameObject wallPrefab;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        if (rb != null)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision collision)
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

    private void SpawnWall()
    {
        GameObject wall = Instantiate(wallPrefab, transform.position, Quaternion.identity);
        SetWallRotation(wall);
    }

    private void SetWallRotation(GameObject wall)
    {
        Quaternion grenadeRotation = transform.rotation;
        wall.transform.rotation = Quaternion.Euler(0f, grenadeRotation.eulerAngles.y - 90, 0f);
    }
}