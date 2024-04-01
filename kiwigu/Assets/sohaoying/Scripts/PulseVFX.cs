using UnityEngine;

public class PulseVFX : MonoBehaviour
{
    [SerializeField] GameObject plasma;
    Material plasmaMat;

    static Vector3 PistonPosition = new Vector3(-15f, 0f, -600f);
    static float offsetMultiplier = 0.151f; // changes the pulse delay

    void Start()
    {
        //if (plasma == null) plasma = gameObject;
        plasmaMat = plasma.GetComponent<Renderer>().material;

        SetPulseOffset(Vector3.Distance(transform.position, PistonPosition));
    }

    void SetPulseOffset(float pistonDistance)
    {
        plasmaMat.SetFloat("_PulseOffset", pistonDistance * offsetMultiplier);
    }
}
