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
    public GameObject blackHoleExplosion;
    bool hasExploded;

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

        float lifeTime = Mathf.Max(time / bullet.lifeTime, saturation / maxSaturation);
        startTime = Time.time - (lifeTime * bullet.lifeTime);

        intensity = intensityOverLifetime.Evaluate(lifeTime);

        blackHole.SetDamagePercent(intensity);

        if (lifeTime > 0.8 && !hasExploded)
        {
            bullet.dead = true;
            GameObject newBlackHoleExplosion = Instantiate(blackHoleExplosion, transform.position, Quaternion.identity);
            newBlackHoleExplosion.transform.localScale = Vector3.one * size;
            hasExploded = true;
        }

        if (lifeTime >= 0.98)
        {
            GameObject newExplosion = Instantiate(explosion, transform.position, Quaternion.identity);
            newExplosion.transform.localScale = Vector3.one * size;
            Destroy(gameObject);
        }
    }
}
