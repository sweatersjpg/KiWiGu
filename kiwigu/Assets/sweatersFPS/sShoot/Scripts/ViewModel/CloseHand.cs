using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseHand : MonoBehaviour
{

    public Transform[] pointerPivots;
    public Transform[] middlePivots;
    public Transform[] pinkyPivots;

    [Header("Angles")]

    public float closedness = 1;
    [Space]
    public float pointer = 0;
    public float middle = 0;
    public float pinky = 0;

    [Space]
    public float closed = 90;
    public float[] jointWeights;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AngleFinger(pointerPivots, pointer);
        AngleFinger(middlePivots, middle);
        AngleFinger(pinkyPivots, pinky);
    }

    void AngleFinger(Transform[] finger, float scale)
    {
        for(int i = 0; i < 3; i++)
        {
            finger[i].localEulerAngles = new(0, closed * scale * closedness * jointWeights[i], 0);
        }
    }
}
