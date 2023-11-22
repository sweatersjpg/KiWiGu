using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fovSync : MonoBehaviour
{
    Camera cameraRef;

    private void Awake()
    {
        cameraRef = Camera.main;
    }

    private void LateUpdate()
    {
        gameObject.GetComponent<Camera>().fieldOfView = cameraRef.fieldOfView;
    }
}
