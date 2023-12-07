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
    public AnimationCurve intensityOverLifetime;

    [Space]
    public BlackHoleVFX blackHole;
    public Transform bubble;
    public GameObject explosion;

    float size = 0;

    [Space]
    public float intensity;
    public float maxSaturation;

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

        bullet.radius = scale * 0.8f;

        transform.localScale = new(size * 2, size * 2, size * 2);

        int saturation = bubble.childCount;

        intensity = saturation / maxSaturation + intensityOverLifetime.Evaluate(time / bullet.lifeTime) * maxSaturation;

        blackHole.SetDamagePercent(intensity);

        if(intensity > 1)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
