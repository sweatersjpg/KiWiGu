using UnityEngine;

public class PulseVFX : MonoBehaviour
{
    [SerializeField] GameObject plasma;
    Material plasmaMat;

    [SerializeField] Vector3 PistonPosition;
    static float offsetMultiplier = 0.151f; // changes the pulse delay

    void Start()
    {
        plasmaMat = plasma.GetComponent<Renderer>().material;

        SetPulseOffset(Vector3.Distance(transform.position, PistonPosition));
    }

    void SetPulseOffset(float pistonDistance)
    {
        plasmaMat.SetFloat("_PulseOffset", pistonDistance * offsetMultiplier);
    }
}
