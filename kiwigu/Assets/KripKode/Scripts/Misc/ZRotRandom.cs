using UnityEngine;

public class ZRotRandom : MonoBehaviour
{
    private void Awake()
    {
        float randomRotation = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0f, 0f, randomRotation);
    }
}
