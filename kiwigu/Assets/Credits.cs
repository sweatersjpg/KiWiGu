using UnityEngine;

public class Credits : MonoBehaviour
{
    void Update()
    {
        if (Input.anyKeyDown) gameObject.SetActive(false);
    }
}
