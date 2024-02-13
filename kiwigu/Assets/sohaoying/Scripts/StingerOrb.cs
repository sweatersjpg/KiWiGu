using UnityEngine;

public class StingerOrb : MonoBehaviour
{
    [SerializeField] private ShootBullet gun;
    private Material orbMat;
    
    [SerializeField] private AnimationCurve distortionCurve;
    private float curveDuration;
    private float startTime;

    void Start()
    {
        orbMat = GetComponent<Renderer>().material;
        curveDuration = distortionCurve[distortionCurve.length - 1].time;
    }

    void FixedUpdate()
    {
        float t = Time.time - startTime;
        if (t < curveDuration)
        {
            if (gun.ammo.count > 0) orbMat.SetFloat("_DistortionAmount", distortionCurve.Evaluate(t));
        }
    }

    public void TriggerOrbAnim()
    {
        startTime = Time.time;
        orbMat.SetFloat("_GlowStrength", Mathf.Max(gun.ammo.count / gun.ammo.capacity, 0.1f));
        if (gun.ammo.count <= 0) gameObject.SetActive(false);
    }
}
