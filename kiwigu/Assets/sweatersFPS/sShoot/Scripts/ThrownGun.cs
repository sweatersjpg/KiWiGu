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

    public bool hasView = true;
    public bool explodeOnContact = true;
    public bool customCollider = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        rb.AddTorque(transform.right * 20);

        rb.AddForce(sweatersController.instance.GetRelativity(), ForceMode.VelocityChange);

        if(ammo.capacity == 0) ammo = new Ammunition(info.capacity);

        if (!hasView) return;

        Transform gunView = info.gunPrefab.transform.Find("GunView");

        if (gunView)
        {
            GameObject gun = Instantiate(gunView.gameObject, transform);
            gun.transform.localPosition = new();

            if(customCollider)
            {
                Collider[] colliders = gun.GetComponentsInChildren<Collider>();
                for(int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = true;
                    colliders[i].gameObject.layer = LayerMask.NameToLayer("PhysicsObject");
                }
            }

            SetLayerRecursively(gun);

            gun.transform.localScale *= 1.2f;
        }

    }

    void SetLayerRecursively(GameObject a)
    {
        a.layer = gameObject.layer;
        
        for (int i = 0; i < a.transform.childCount; i++)
        {
            SetLayerRecursively(a.transform.GetChild(i).gameObject);
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

    public void TakeDamage(object[] args)
    {
        float damage = (float)args[2];

        Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log(ammo.count / ammo.capacity + " " + ammo.count + " " + ammo.capacity);

        //if (ammo.count / ammo.capacity < 0.2f)
        //{
        if(explodeOnContact) Explode();

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

    void Explode()
    {        
        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
