using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{

    public GameObject thingPrefab;

    public int numberOfThings = 1;
    public float spawnDelay = 4;

    public List<GameObject> things;
    
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < numberOfThings; i++)
            Invoke(nameof(SpawnThing), spawnDelay * (i));
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject thing in things)
        {
            if (thing != null) continue;
            things.Remove(thing);
            Invoke(nameof(SpawnThing), spawnDelay);
        }
    }

    void SpawnThing()
    {
        GameObject thing = Instantiate(thingPrefab, transform.position, Quaternion.identity);
        things.Add(thing);
    }
}
