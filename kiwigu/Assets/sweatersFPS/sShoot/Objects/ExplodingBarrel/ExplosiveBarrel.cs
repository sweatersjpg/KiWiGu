using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ExplosiveBarrel : MonoBehaviour
{
    public float health;
    public GameObject explosionPrefab;

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
        Vector3 point = (Vector3)args[0];
        Vector3 direction = (Vector3)args[1];
        float damage = (float)args[2];

        // front.material.SetColor("_Color", Color.Lerp(endColor, startColor, health / maxHealth));

        health -= damage;

        if (health <= 0)
        {
            Instantiate(explosionPrefab, point, Quaternion.LookRotation(Vector3.up, -direction));
            Destroy(gameObject);
        }
    }
}
