using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDecay : MonoBehaviour
{
    public float lifeSpan = 5;
    public AnimationCurve positon;

    float time;

    // Start is called before the first frame update
    void Start()
    {
        time = Time.time;
    }

    // Update is called once per frame
    void Update()
    {

        transform.localPosition = new Vector3(0, 0, positon.Evaluate((Time.time - time) / lifeSpan));

        if (Time.time - time > lifeSpan) Destroy(transform.parent.gameObject);
    }
}
