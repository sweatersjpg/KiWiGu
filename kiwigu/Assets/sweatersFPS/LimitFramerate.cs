using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitFramerate : MonoBehaviour
{
    [SerializeField]
    int frameRate;

    void Start()
    {
        Application.targetFrameRate = frameRate;
    }
}