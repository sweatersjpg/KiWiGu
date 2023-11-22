using UnityEngine;
using UnityEngine.VFX;

public class BlackHoleVFX : MonoBehaviour
{
    [SerializeField] GameObject bubble;
    Material bubbleMat;

    [SerializeField] GameObject core;
    Material coreMat;

    [SerializeField] GameObject debris;
    VisualEffect debrisVFX;
    [SerializeField] float debrisMinScale = 0.8f;
    [SerializeField] float debrisMaxScale = 5f;
    float debrisScale;

    //[Range(0, 1)][SerializeField] float damagePercent;    // for testing only

    void Start()
    {
        bubbleMat = bubble.GetComponent<Renderer>().material;
        coreMat = core.GetComponent<Renderer>().material;
        debrisVFX = debris.GetComponent<VisualEffect>();
    }

    /*void FixedUpdate()
    {
        SetDamagePercent(damagePercent);    // for testing only
    }*/

    internal void SetDamagePercent(float newDamagePercent)    // newDamagePercent ranges from 0f to 1f (harmless -> about to pop)
    {
        bubbleMat.SetFloat("_DamagePercent", newDamagePercent);
        coreMat.SetFloat("_DamagePercent", newDamagePercent);
        debrisVFX.SetFloat("DamagePercent", newDamagePercent);
        debrisScale = Mathf.Lerp(debrisMinScale, debrisMaxScale, newDamagePercent);
        debris.transform.localScale = new Vector3(debrisScale, debrisScale, debrisScale);
    }
}
