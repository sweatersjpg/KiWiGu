using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public GameObject directionalHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        rb.AddTorque(transform.right * 20);

        //rb.velocity += sweatersController.instance.velocity;

        if(ammo.capacity == 0) ammo = new Ammunition(info.capacity);

        Transform gunView = info.gunPrefab.transform.Find("GunView");

        if (gunView)
        {
            GameObject gun = Instantiate(gunView.gameObject, transform);
            gun.transform.localPosition = new();
            gun.layer = gameObject.layer;

            for(int i = 0; i < gun.transform.childCount; i++)
            {
                gun.transform.GetChild(i).gameObject.layer = gameObject.layer;
            }

            gun.transform.localScale *= 1.2f;
        }

    }

    public void SetMesh(Mesh mesh, Material mat)
    {
        // MeshFilter mf = GetComponentInChildren<MeshFilter>();

        //mf.mesh = mesh;
        //mf.transform.GetComponent<MeshRenderer>().sharedMaterial = mat;
        
        // GetComponent<MeshCollider>().sharedMesh = mesh;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log(ammo.count / ammo.capacity + " " + ammo.count + " " + ammo.capacity);

        //if (ammo.count / ammo.capacity < 0.2f)
        //{
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);

        //}
        //else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        //{
            
        //    GameObject t = new GameObject("Target");
        //    t.transform.position = collision.GetContact(0).point;

        //    GameObject d = Instantiate(directionalHit, transform.position, Quaternion.identity);
        //    d.GetComponent<DirectionalAttack>().target = t.transform;
        //    d.GetComponent<DirectionalAttack>().ignoreList.Add(gameObject);
        //}
    }
}
