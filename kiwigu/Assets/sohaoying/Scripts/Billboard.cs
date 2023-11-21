using UnityEngine;

public class Billboard : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }
}