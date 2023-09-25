using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] float spawnDelay = 5f;

    private void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        WaitForSeconds delay = new WaitForSeconds(spawnDelay);

        while (true)
        {
            yield return delay;

            if (enemyPrefabs.Length > 0)
            {
                int enemyIndex = Random.Range(0, enemyPrefabs.Length);
                Instantiate(enemyPrefabs[enemyIndex], transform.position, transform.rotation, transform);
            }
        }
    }
}