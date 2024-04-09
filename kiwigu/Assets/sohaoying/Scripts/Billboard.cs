using UnityEngine;

public class Billboard : MonoBehaviour
{
    void FixedUpdate()
    {
        //transform.forward = Camera.main.transform.forward;
        if (Camera.main != null)
        transform.LookAt(Camera.main.transform.position, Vector3.up);
    }
}