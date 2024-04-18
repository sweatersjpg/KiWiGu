using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowMo : MonoBehaviour
{

    float slowmoTimer = 0;
    float targetTimeScale = 1;
    public float timeScaleSpeed = 20;

    public static SlowMo instance;

    // Start is called before the first frame update
    void Awake()
    {
        if(instance != null) Destroy(instance);
        else
        {
            instance = this;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (PauseSystem.paused) return;

        if(slowmoTimer > 0)
        {
            slowmoTimer -= Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, Time.unscaledDeltaTime * timeScaleSpeed);
        } else
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, Time.unscaledDeltaTime * timeScaleSpeed);
        }
    }

    public static void SlowTime(float duration, float timeScale)
    {
        instance.slowmoTimer = duration;
        instance.targetTimeScale = timeScale;
    }
}
