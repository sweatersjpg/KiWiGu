using UnityEngine;

public class Billboard : MonoBehaviour
{
    void FixedUpdate()
    {
        //transform.forward = Camera.main.transform.forward;
        transform.LookAt(Camera.main.transform.position, Vector3.up);
    }
}