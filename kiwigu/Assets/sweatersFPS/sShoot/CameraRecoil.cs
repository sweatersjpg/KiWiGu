using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    List<float> recoilRequests;

    // Start is called before the first frame update
    void Start()
    {
        recoilRequests = new List<float>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.localEulerAngles = new(-FindMax(recoilRequests), 0, 0);

        recoilRequests.Clear();
    }

    float FindMax(List<float> angles)
    {
        float max = 0;
        foreach(float a in angles) max = Mathf.Max(max, a);
        return max;
    }

    public void RequestRecoil(float recoilAngle)
    {
        recoilRequests.Add(recoilAngle);
    }
}
