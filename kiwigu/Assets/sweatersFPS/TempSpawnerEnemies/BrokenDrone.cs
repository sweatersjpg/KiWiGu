using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenDrone : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] float gravity;
    [SerializeField] float fanForce;
    [SerializeField] float spin;
    [SerializeField] float push;

    [SerializeField] GameObject explosion;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.AddForce(-transform.right * push, ForceMode.Impulse);
        rb.AddTorque(Vector3.up * spin, ForceMode.Impulse);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravity);
        rb.AddForce(transform.up * fanForce);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount > 0)
        {
            if (Vector3.Angle(collision.contacts[0].normal, Vector3.up) < 45)
            {
                SpawnExplosion();
            }
        }
    }

    void SpawnExplosion()
    {
        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
