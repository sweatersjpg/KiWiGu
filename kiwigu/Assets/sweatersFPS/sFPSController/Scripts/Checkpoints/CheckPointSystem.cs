using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSystem : MonoBehaviour
{
    public bool startingSpawn = false;

    public static Vector3 spawnPoint;

    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log(spawnPoint);

        if (spawnPoint.Equals(new Vector3()) && startingSpawn)
        {
            spawnPoint = transform.position;
            DontDestroyOnLoad(this);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Spawnpoint Set");

            spawnPoint = transform.position;
        }
    }
}
