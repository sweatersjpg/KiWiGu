using System.Collections;
using UnityEngine;

public class HitBoxSpawner : MonoBehaviour
{
    [SerializeField] private GunInfo gunInfo;
    [SerializeField] GameObject ht;


    private bool hasSpawned = false;

    private void Start()
    {
        SpawnPrefab();
    }

    private void Update()
    {
        if (!hasSpawned && transform.childCount == 0 && !IsInvoking("SpawnPrefab"))
        {
            Invoke("SpawnPrefab", 2f);
        }
    }

    private void SpawnPrefab()
    {
        if (!hasSpawned)
        {
            GameObject spawnedPrefab = Instantiate(ht, transform.position, transform.rotation);
            spawnedPrefab.GetComponent<HookTarget>().info = gunInfo;
            spawnedPrefab.transform.parent = transform;

            hasSpawned = true;
            StartCoroutine(ResetSpawnFlag());
        }
    }

    private IEnumerator ResetSpawnFlag()
    {
        yield return new WaitForSeconds(2);
        hasSpawned = false;
    }
}
