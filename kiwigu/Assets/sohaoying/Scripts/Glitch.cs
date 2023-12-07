using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glitch : MonoBehaviour
{
    [SerializeField] Material mat;
    [Range(0f, 0.3f)][SerializeField] float glitchStrength = 0f;
    [Range(0f, 10f)][SerializeField] float noiseAmount = 0f;
    [Range(0.9f, 1f)][SerializeField] float scanLinesStrength = 1f;
    [Range(0f, 0.02f)][SerializeField] float vignetteIntensity = 0f;

    static Animator animator;

    void Start()
    {
        animator = gameObject.GetComponent<Animator>();

        scanLinesStrength = 1f;
        vignetteIntensity = 0f;
    }

    void FixedUpdate()
    {
        mat.SetFloat("_GlitchStrength", glitchStrength);
        mat.SetFloat("_NoiseAmount", noiseAmount);
        mat.SetFloat("_ScanLinesStrength", scanLinesStrength);
        mat.SetFloat("_VignetteIntensity", vignetteIntensity);
    }

    public static void TriggerFX()
    {
        animator.SetTrigger("Glitch");
    }
}
