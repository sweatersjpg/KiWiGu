using UnityEngine;

public class EnemyCoverDetection : MonoBehaviour
{
    [HideInInspector]
    public GameObject coverObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cover"))
        {
            GameObject[] arrayOfObjects = GameObject.FindGameObjectsWithTag("Cover");

            if (arrayOfObjects.Length > 0)
            {
                int randomIndex = Random.Range(0, arrayOfObjects.Length);
                coverObject = arrayOfObjects[randomIndex];
            }
        }
    }
}
