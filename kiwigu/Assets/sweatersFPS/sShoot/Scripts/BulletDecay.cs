using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDecay : MonoBehaviour
{
    public float lifeSpan = 5;
    public AnimationCurve positon;

    float time;

    void Start()
    {
        time = Time.time;
    }

    void Update()
    {
        transform.localPosition = new Vector3(0, 0, positon.Evaluate((Time.time - time) / lifeSpan));

        if (Time.time - time > lifeSpan) Destroy(transform.parent.gameObject);
    }
}
