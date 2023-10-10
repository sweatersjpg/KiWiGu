using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCameraFX : MonoBehaviour
{
    List<float> recoilRequests;
    Camera playerCamera;

    public AnimationCurve fovTransition;
    public float transitionDuration = 0.5f;

    float fovTimer = 0;
    bool scopeIn = false;
    float scopeFOV = 30;

    // Start is called before the first frame update
    void Start()
    {
        recoilRequests = new List<float>();
        playerCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.localEulerAngles = new(-FindMax(recoilRequests), 0, 0);

        recoilRequests.Clear();

        if (scopeIn) fovTimer += 1 / transitionDuration * Time.deltaTime;
        else fovTimer -= 1 / transitionDuration * Time.deltaTime;

        if (fovTimer < 0) fovTimer = 0;
        if (fovTimer > 1) fovTimer = 1;

        playerCamera.fieldOfView = Mathf.Lerp(PauseSystem.FOV, scopeFOV, fovTransition.Evaluate(fovTimer));
        sweatersController.instance.lookSpeed
            = Mathf.Lerp(PauseSystem.mouseSensitivity, PauseSystem.mouseSensitivity / 2, fovTransition.Evaluate(fovTimer)); ;

        scopeIn = false;
    }

    float FindMax(List<float> angles)
    {
        float max = 0;
        foreach (float a in angles) max = Mathf.Max(max, a);
        return max;
    }

    public void RequestRecoil(float recoilAngle)
    {
        recoilRequests.Add(recoilAngle);
    }

    public bool ScopedIn()
    {
        return fovTimer > 0;
    }

    public void RequestFOV(float FOV)
    {
        scopeFOV = FOV;
        scopeIn = true;
        return;
    }
}
