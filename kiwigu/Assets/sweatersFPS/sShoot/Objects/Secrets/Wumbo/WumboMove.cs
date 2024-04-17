using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WumboMove : MonoBehaviour
{
    Rigidbody rb;
    public float gravityScale = 0.5f;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();

        // rb.useGravity = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (transform.childCount > 0) rb.velocity = Vector3.zero;

        // rb.AddForce(Vector3.down * gravityScale);
    }
}
