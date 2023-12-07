using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glitch : MonoBehaviour
{
    [SerializeField] Material mat;
    [Range(0f, 0.3f)][SerializeField] float glitchStrength;
    [Range(0f, 10f)][SerializeField] float noiseAmount;

    static Animator animator;

    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        mat.SetFloat("_GlitchStrength", glitchStrength);
        mat.SetFloat("_NoiseAmount", noiseAmount);
    }

    public static void TriggerFX()
    {
        animator.SetTrigger("Glitch");
    }
}
