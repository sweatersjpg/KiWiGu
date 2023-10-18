using UnityEngine;
using FMODUnity;

public class Music : MonoBehaviour
{
    // this entire thing is a total placeholder until there is a proper battle theme
    // we can decide how to do the transitions later, likely in FMOD itself with magnet regions but we'll see

    [Range(0, 1)] [SerializeField] internal int Violence;
    [SerializeField] float transitionTime = 1.2f;
    [SerializeField] StudioEventEmitter musicEmitter;

    float displayPercent;
    float startPercent;
    float timeLeft;
    float prevTarget;

    void Update()
    {
        if (prevTarget != Violence)
        {
            timeLeft = transitionTime;
            startPercent = displayPercent;
        }
        prevTarget = Violence;

        if (timeLeft - Time.deltaTime < 0f)
        {
            timeLeft = 0f;
            displayPercent = Violence;
        }
        timeLeft -= Time.deltaTime;

        float progress = 1f - timeLeft / transitionTime;
        if (progress > 1f) progress = 1f;
        progress = progress * progress * (3f - 2f * progress); // hermite easing

        float deltaPercent = Violence - startPercent;
        displayPercent = startPercent + deltaPercent * progress;

        musicEmitter.SetParameter("Violence", displayPercent);
    }
}