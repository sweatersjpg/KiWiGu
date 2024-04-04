using System.Collections.Generic;
using UnityEngine;

public class StressTest : MonoBehaviour
{
    public GameObject objectToCopy;
    public int amountToSpawn = 1000;
    public List<GameObject> objects = new List<GameObject>();

    private void Start()
    {
        objects = new List<GameObject>();
        for (int i = 0; i < amountToSpawn; i++)
        {
            GameObject obj = Instantiate(objectToCopy, new Vector3(Random.Range(-100, 100), 1, Random.Range(-100, 100)), Quaternion.identity);
            objects.Add(obj);
        }
    }
}
