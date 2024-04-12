using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glitch : MonoBehaviour
{
    [SerializeField] Cyan.Blit glitchBlit;
    [SerializeField] HealthBar healthBar;
    [SerializeField] AnimationCurve vignetteIntensityCurve;
    [Range(0f, 0.3f)][SerializeField] float glitchStrength = 0f;
    [Range(0f, 10f)][SerializeField] float noiseAmount = 0f;
    [Range(0.9f, 1f)][SerializeField] float scanLinesStrength = 1f;
    [Range(0f, 0.04f)][SerializeField] float vignetteIntensity = 0f;

    static Animator animator;

    void Start()
    {
        animator = gameObject.GetComponent<Animator>();

        scanLinesStrength = 1f;
        vignetteIntensity = 0f;
    }

    void FixedUpdate()
    {
        vignetteIntensity = Mathf.Max(vignetteIntensity, (0.04f * vignetteIntensityCurve.Evaluate(1f - healthBar.TargetPercent)));
        glitchBlit.UpdateValues(glitchStrength, noiseAmount, scanLinesStrength, vignetteIntensity);
    }

    public static void TriggerFX()
    {
        animator.SetTrigger("Glitch");
    }
}
