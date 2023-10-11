using UnityEngine;

class HealthBar : MonoBehaviour
{

    /*
    to adjust masks and bar size
    set local x on LeftBar and RightBar to 0
    set local x on LeftZeroOffset and RightZeroOffset to some value until the health bar looks correct for 0 HP
    set local x on LeftBar and RightBar to some value until the health bar looks correct for full HP
    */

    [Range(0, 1)]
    [SerializeField] internal float TargetPercent = 1f; // call from any other class and the script will behave
                                                        // e.g. HealthBar healthBar = GetComponent<HealthBar>();
                                                        // healthBar.TargetPercent = currentHealth / totalHealth;
                                                        // note: do Not call this every frame. only call this when the health changes

    [SerializeField] float animTime = 0.5f;
    [SerializeField] RectTransform leftBar, rightBar; // leftBar has a negative local x. drag in the LeftBar and RightBar transforms here
    float displayPercent;
    float dist;
    float startPercent;
    float timeLeft;
    float prevTarget;

    void Start()
    {
        dist = leftBar.anchoredPosition3D.x;
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

    void ApplyPercent(float percent)
    {
        Vector3 buffer = leftBar.anchoredPosition3D;
        leftBar.anchoredPosition3D = new Vector3(dist * percent, buffer.y, buffer.z);
        buffer = rightBar.anchoredPosition3D;
        rightBar.anchoredPosition3D = new Vector3(-dist * percent, buffer.y, buffer.z);
    }
}
