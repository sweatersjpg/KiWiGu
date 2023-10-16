using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class SizeOverLifetime : MonoBehaviour
{
    public Bullet bullet;
    public Transform target;

    float startTime;

    public float maxSize;
    public float minSize;
    public AnimationCurve sizeOverLifetime;

    float size = 0;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        maxSize *= bullet.charge;
        if (maxSize < minSize) maxSize = minSize;

        bullet.lifeTime *= 1 + bullet.charge;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.transform.position;

        float time = Time.time - startTime;

        float scale = sizeOverLifetime.Evaluate(time / bullet.lifeTime) * maxSize;
        if (scale < 0) scale = 0;
        size += Time.deltaTime * 50 * (scale - size) / 2;

        transform.localScale = new(size * 2, size * 2, size * 2);
    }
}
