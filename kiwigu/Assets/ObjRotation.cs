using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjRotation : MonoBehaviour
{
    public bool rotateX;
    public bool rotateY;
    public bool rotateZ;
    public float xSpeed;
    public float ySpeed;
    public float zSpeed;

    private void Update()
    {
        if (rotateX)
        {
            transform.Rotate(xSpeed * Time.deltaTime, 0, 0);
        }
        if (rotateY)
        {
            transform.Rotate(0, ySpeed * Time.deltaTime, 0);
        }
        if (rotateZ)
        {
            transform.Rotate(0, 0, zSpeed * Time.deltaTime);
        }
    }
}
