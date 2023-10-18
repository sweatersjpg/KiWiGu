using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pixelPerfectView : MonoBehaviour
{
    public float menuWidth = 320;
    public float menuHeight = 256;

    Camera gameCamera;

    // Start is called before the first frame update
    void Start()
    {
        gameCamera = sweatersController.instance.playerCamera;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        AdjustViewPlane();
    }

    void AdjustViewPlane()
    {
        float frustumHeight = 2.0f * transform.localPosition.z * Mathf.Tan(gameCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        float height = (menuHeight / Screen.height) * frustumHeight;
        height *= (int)Screen.height / menuHeight;

        transform.localScale = new Vector3(height * menuWidth / menuHeight, height, 1);
        //transform.localPosition = new Vector3(transform.localPosition.x, -frustumHeight / 2f + height / 2, transform.localPosition.z);
    }
}
