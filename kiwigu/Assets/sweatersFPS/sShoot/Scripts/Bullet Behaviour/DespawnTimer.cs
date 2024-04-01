using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnTimer : MonoBehaviour
{

    public float lifetime = 1f;

    float blinkTimer;

    float start;

    MeshRenderer[] mrs;
    
    // Start is called before the first frame update
    void Start()
    {
        start = Time.time;
        blinkTimer = Time.time;

        mrs = GetComponentsInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Time.time - start > lifetime-0.5f && Time.time - blinkTimer > 0.02f)
        {
            blinkTimer = Time.time;

            for (int i = 0; i < mrs.Length; i++)
            {
                if (mrs[i]) mrs[i].enabled = !mrs[i].enabled;
            }
        }

        

        if (Time.time - start > lifetime) Destroy(gameObject);
    }
}
