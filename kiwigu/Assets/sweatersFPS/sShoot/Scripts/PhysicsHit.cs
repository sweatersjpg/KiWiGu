using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsHit : MonoBehaviour
{
    // Start is called before the first frame update

    Rigidbody rb;

    public float maxForce;

    public Mesh altMesh;
    Mesh startMesh;

    Vector3 startPos;

    public bool keepUp;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        startPos = transform.position;

        startMesh = GetComponent<MeshFilter>().mesh;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            transform.position = startPos;
            rb.velocity = new();
            transform.rotation = Quaternion.identity;
            GetComponent<MeshFilter>().mesh = startMesh;
        }
    }

    public void Hit(Vector3 point, Vector3 velocity)
    {
        Vector3 force = Vector3.ClampMagnitude(velocity, maxForce);

        if (keepUp) force = new(0, maxForce, 0);

        rb.AddForceAtPosition(force, point, ForceMode.Impulse);

        GetComponent<MeshFilter>().mesh = altMesh;
    }
}
