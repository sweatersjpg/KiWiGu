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

    public float fovSpeedScale = 2;
    float targetFOV;

    public float tiltIntensity = 5;
    float tilt = 0;

    // Start is called before the first frame update
    void Start()
    {
        recoilRequests = new List<float>();
        playerCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        sweatersController player = sweatersController.instance;

        float targetTilt = Vector3.Dot(player.velocity, player.transform.right) * tiltIntensity;
        if (PauseSystem.paused) targetTilt = 0;

        tilt += (targetTilt - tilt) / 4 * Time.deltaTime * 50;
        
        transform.localEulerAngles = new(-FindMax(recoilRequests), 0, -tilt);

        recoilRequests.Clear();

        if (scopeIn) fovTimer += 1 / transitionDuration * Time.deltaTime;
        else fovTimer -= 1 / transitionDuration * Time.deltaTime;

        if (fovTimer < 0) fovTimer = 0;
        if (fovTimer > 1) fovTimer = 1;

        targetFOV = Mathf.Lerp(PauseSystem.FOV, scopeFOV, fovTransition.Evaluate(fovTimer));

        float speedFOV = Mathf.Max(Vector3.Dot(player.velocity, player.transform.forward), 0) * fovSpeedScale;
        targetFOV += speedFOV;

        playerCamera.fieldOfView += (targetFOV - playerCamera.fieldOfView) / 4 * Time.deltaTime * 50;

        SetMouseSensitivity();

        //player.lookSpeed
        //    = Mathf.Lerp(PauseSystem.mouseSensitivity, PauseSystem.mouseSensitivity / 2, fovTransition.Evaluate(fovTimer));

        scopeIn = false;
    }
    
    void SetMouseSensitivity()
    {
        float max = PauseSystem.FOV;
        float min = PauseSystem.pauseSystem.FOVmin;

        float fov = (targetFOV - min) / (max - min);

        float sens = Mathf.Lerp(2, 1, fov);

        sweatersController.instance.lookSpeed = PauseSystem.mouseSensitivity / sens;
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
