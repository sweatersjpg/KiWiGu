using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class HealthBar : MonoBehaviour
{

    /*
    health bar credit: @null-machine <3 thank you for the help!! 
    
    to adjust masks and bar size
    set local x on LeftBar and RightBar to 0
    set local x on LeftZeroOffset and RightZeroOffset to some value until the health bar looks correct for 0 HP
    set local x on LeftBar and RightBar to some value until the health bar looks correct for full HP
    */

    [Range(0f, 1f)]
    [SerializeField] internal float TargetPercent = 1f; // call from any other class and the script will behave
                                                        // e.g. [SerializeField] HealthBar healthBar;
                                                        // healthBar.TargetPercent = currentHealth / totalHealth;
                                                        // note: do Not call this every frame. only call this when the health changes

    [Range(0f, 1f)]
    [SerializeField] float pulseThresholdPercent = 0.3f;   // percent of HP at which the bar collapses and starts pulsing

    bool Pulsing = false;
    [SerializeField] float pulseRate = 3.5f;
    [SerializeField] float minSize = 1f;
    [SerializeField] float maxSize = 1.02f;
    [SerializeField] float collapseDist = 40f; // for the "wings"
    [SerializeField] float collapseBarDist = 4.2f; // for the "triangle"
    float fullDist;
    float fullBarDist;
    /* while true, fades out the health bar (regardless of how much of it is filled), brings the two halves together and pulses them
     * while false, fades back in the health bar and expands the halves again
     * this can be toggled as often as required
     * if more smoothness is desired, switch implementation from lerp to hermite
    */
    float transitionSpeed = 0.7f; // lerp

    [SerializeField] float animTime = 0.5f;
    [SerializeField] RectTransform leftBar, rightBar; // leftBar has a negative local x. drag in the LeftBar and RightBar transforms here
    float displayPercent; // [0, 1]
    float dist;
    float startPercent;
    float timeLeft;
    float prevTarget;

    [SerializeField] Image healthBarBackground;
    [SerializeField] List<Image> healthBarImages;
    // some code in this class assumes LBar, RBar, LBorder, RBorder
    [SerializeField] List<Material> healthBarMaterials;


    void Start()
    {
        fullBarDist = dist = leftBar.anchoredPosition3D.x;
        fullDist = ((RectTransform)healthBarImages[3].transform).anchoredPosition3D.x;
        foreach (Image i in healthBarImages)
        {
            healthBarMaterials.Add(i.GetComponent<Image>().material);
        }
    }

    void Update()
    {
        if (prevTarget != TargetPercent)
        {
            timeLeft = animTime;
            startPercent = displayPercent;
        }
        prevTarget = TargetPercent;

        if (timeLeft - Time.deltaTime < 0f)
        {
            timeLeft = 0f;
            displayPercent = TargetPercent;
        }
        timeLeft -= Time.deltaTime;

        float progress = 1f - timeLeft / animTime;
        if (progress > 1f) progress = 1f;
        progress = progress * progress * (3f - 2f * progress); // hermite easing

        float deltaPercent = TargetPercent - startPercent;
        displayPercent = startPercent + deltaPercent * progress;

        ApplyPercent(displayPercent);
    }

    void FixedUpdate()
    {
        // used for lerp, technically should be in reg update with delta mult but whatever
        float progress = healthBarBackground.color.a;
        if (Pulsing) {
            progress *= transitionSpeed;
            float scale = Mathf.PingPong(Time.time * pulseRate, 1f);
            scale = 1f - scale * scale * scale;
            ((RectTransform)transform).localScale = Vector3.one * (minSize + scale * (maxSize - minSize));
            // recttransform hates having its scale set
            // this is why the anchor of this health bar is on the rect bottom instead of the top
            // a cleaner solution probably exists
        } else {
            progress = progress + (1f - progress) * (1f - transitionSpeed);
            ((RectTransform)transform).localScale = Vector3.one;
        }
        // healthBarImages[0].color = new Color(1f, 1f, 1f, progress);
        // healthBarImages[1].color = new Color(1f, 1f, 1f, progress);
        healthBarBackground.color = new Color(1f, 1f, 1f, progress);

        Vector3 buffer = ((RectTransform)healthBarImages[2].transform).anchoredPosition3D;
        ((RectTransform)healthBarImages[2].transform).anchoredPosition3D = new Vector3(-(fullDist - collapseDist) * progress - collapseDist, buffer.y, buffer.z);
        buffer = ((RectTransform)healthBarImages[3].transform).anchoredPosition3D;
        ((RectTransform)healthBarImages[3].transform).anchoredPosition3D = new Vector3((fullDist - collapseDist) * progress + collapseDist, buffer.y, buffer.z);

        buffer = ((RectTransform)healthBarImages[0].transform).anchoredPosition3D;
        ((RectTransform)healthBarImages[0].transform).anchoredPosition3D = new Vector3(-(fullBarDist - collapseBarDist) * progress - collapseBarDist, buffer.y, buffer.z);
        buffer = ((RectTransform)healthBarImages[1].transform).anchoredPosition3D;
        ((RectTransform)healthBarImages[1].transform).anchoredPosition3D = new Vector3((fullBarDist - collapseBarDist) * progress + collapseBarDist, buffer.y, buffer.z);

        
    }

    void ApplyPercent(float percent) // range 0f, 1f
    {
        foreach (Material m in healthBarMaterials)
        {
            m.SetFloat("_HPPercent", percent);
        }

        if (percent <= pulseThresholdPercent)
        {
            Pulsing = true;
            return;
        } else
        {
            Pulsing = false;
        }

        Vector3 buffer = leftBar.anchoredPosition3D;
        leftBar.anchoredPosition3D = new Vector3(dist * ((percent - pulseThresholdPercent) / (1f - pulseThresholdPercent)), buffer.y, buffer.z);
        buffer = rightBar.anchoredPosition3D;
        rightBar.anchoredPosition3D = new Vector3(-dist * ((percent - pulseThresholdPercent) / (1f - pulseThresholdPercent)), buffer.y, buffer.z);
    }

    void OnApplicationQuit()
    {
        ApplyPercent(1f);   // reset materials
    }
}
