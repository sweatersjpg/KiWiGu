using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakables : MonoBehaviour
{
    public float health;
    public GameObject particles;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(object[] args)
    {
        if (health <= 0) return;

        float damage = (float)args[2];

        // front.material.SetColor("_Color", Color.Lerp(endColor, startColor, health / maxHealth));

        health -= damage;

        if (health <= 0)
        {
            GlobalAudioManager.instance.PlayAmbianceSound(transform, "Rock");
            particles.transform.parent = null;
            particles.SetActive(true);

            Destroy(gameObject);
            Destroy(particles, 2);
        }
    }
}
