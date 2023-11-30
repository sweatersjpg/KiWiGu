using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject prefabToSpawn;

    private bool hasSpawned = false;

    private void Start()
    {
        SpawnPrefab();
    }

    private void Update()
    {
        if (!hasSpawned && transform.childCount == 0)
        {
            SpawnPrefab();
        }
    }

    private void SpawnPrefab()
    {
        if (!hasSpawned)
        {
            GameObject spawnedPrefab = Instantiate(prefabToSpawn, transform.position, transform.rotation);
            spawnedPrefab.transform.parent = transform;

            hasSpawned = true;
            StartCoroutine(ResetSpawnFlag());
        }
    }

    private IEnumerator ResetSpawnFlag()
    {
        yield return new WaitForSeconds(1f);
        hasSpawned = false;
    }
}
