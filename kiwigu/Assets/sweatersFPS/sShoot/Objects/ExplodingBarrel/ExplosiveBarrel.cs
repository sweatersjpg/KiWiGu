using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ExplosiveBarrel : MonoBehaviour
{
    public float health;
    public GameObject explosionPrefab;
    public GameObject blackHoleFX;
    public GameObject view;

    HookTarget ht;

    // Start is called before the first frame update
    void Start()
    {
        ht = GetComponentInChildren<HookTarget>();
    }

    // Update is called once per frame
    void Update()
    {
        if(ht.transform.parent != transform)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(object[] args)
    {
        if (health <= 0) return;
        
        float damage = (float)args[2];

        // front.material.SetColor("_Color", Color.Lerp(endColor, startColor, health / maxHealth));

        health -= damage;

        if (health <= 0)
        {
            Invoke(nameof(StartExplosion), Random.Range(0, 0.2f));
        }
    }

    void StartExplosion()
    {
        blackHoleFX.SetActive(true);
        Invoke(nameof(Explode), Random.Range(0.2f, 0.4f));

        view.tag = "RigidTarget";
        gameObject.tag = "RigidTarget";
        gameObject.AddComponent<BoxCollider>();
        gameObject.AddComponent<Rigidbody>();
        PhysicsHit hit = gameObject.AddComponent<PhysicsHit>();
        hit.keepUp = true;
        hit.maxForce = 10;
    }

    void Explode()
    {
        MoveHook mh = GetComponentInChildren<MoveHook>();
        if (mh)
        {
            mh.transform.parent = null;
            mh.PullbackWithForce(0, 1);
        }

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
