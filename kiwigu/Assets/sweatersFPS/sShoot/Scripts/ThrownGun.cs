using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ThrownGun : MonoBehaviour
{
    // Start is called before the first frame update

    Rigidbody rb;
    public GunInfo info;

    public float throwForce = 10;

    public Ammunition ammo;

    public GameObject explosion;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        rb.velocity += sweatersController.instance.velocity;

        if(ammo.capacity == 0) ammo = new Ammunition(info.capacity);
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

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
