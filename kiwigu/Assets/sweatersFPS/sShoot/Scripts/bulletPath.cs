using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletPath : MonoBehaviour
{
    public float speed;
    public float gravity;

    Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        //velocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        velocity = transform.forward * speed;

        DrawPath();
    }

    void DrawPath()
    {
        for(int i = 0; i < 1000; i++)
        {
            Debug.DrawLine(EvaluateLocation(i/500), EvaluateLocation(i/500 + 1));
        }
    }

    Vector3 EvaluateLocation(float time)
    {
        // y = v * t + 0.5 * gravity * t * t

        float x = velocity.x * time;
        float y = velocity.y * time + 0.5f * gravity * time * time;
        float z = velocity.z * time;

        return new Vector3(x, y, z) + transform.position;
    }
}
