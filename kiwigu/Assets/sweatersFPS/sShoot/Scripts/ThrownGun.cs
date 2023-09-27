using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownGun : MonoBehaviour
{
    // Start is called before the first frame update

    Rigidbody rb;
    public GunInfo info;

    public float throwForce = 10;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        rb.velocity += sweatersController.instance.velocity;
    }

    public void SetMesh(Mesh mesh)
    {
        GetComponent<MeshFilter>().mesh = mesh;
        // GetComponent<MeshCollider>().sharedMesh = mesh;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
