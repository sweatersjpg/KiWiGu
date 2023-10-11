using UnityEngine;
using UnityEngine.UI;

class EnergyBar : MonoBehaviour
{

    [Range(0, 1)]
    [SerializeField] internal float TargetPercent = 1f; // call from any other class and the script will behave
                                                        // e.g. EnergyBar energyBar = GetComponent<EnergyBar>();
                                                        // energyBar.TargetPercent = currentEnergy / totalEnergy;
                                                        // note: do Not call this every frame. only call this when the energy changes

    [SerializeField] float animTime = 0.5f;

    Image barImage;
    float displayPercent;
    float startPercent;
    float timeLeft;
    float prevTarget;

    void Start()
    {
        barImage = GetComponent<Image>();
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

        barImage.fillAmount = displayPercent;
    }
}
