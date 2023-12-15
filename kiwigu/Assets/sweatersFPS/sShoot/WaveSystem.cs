using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{

    public GameObject spawnFX;

    [Space]
    [SerializeField] EnemyWave[] waves;

    [Space]
    [SerializeField] int currentWave = 0;
    int activeSpawners = 0;

    List<Transform> freeSpawnPoints;
    
    // Start is called before the first frame update
    void Start()
    {
        ResetSpawnPoints();

        InvokeRepeating(nameof(ResetSpawnPoints), 1, 1f);

        StartCoroutine(nameof(StartWave));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ResetSpawnPoints()
    {
        freeSpawnPoints = new List<Transform>();

        for(int i = 0; i < waves[currentWave].SpawnPoints.childCount; i++)
        {
            freeSpawnPoints.Add(waves[currentWave].SpawnPoints.GetChild(i));
        }
    }

    IEnumerator StartWave()
    {
        Debug.Log("Starting Wave " + currentWave);
        
        yield return new WaitForSeconds(waves[currentWave].startDelay);

        activeSpawners = waves[currentWave].enemySpawns.Length;

        Coroutine[] spawners = new Coroutine[waves[currentWave].enemySpawns.Length];

        for (int i = 0; i < waves[currentWave].enemySpawns.Length; i++)
        {
            spawners[i] = StartCoroutine(nameof(SpawnEnemies), i);
        }

        Debug.Log(activeSpawners);

        yield return new WaitUntil(() => { return activeSpawners == 0; });

        for(int i = 0; i < spawners.Length; i++)
        {
            StopCoroutine(spawners[i]);
        }

        // execute next wave
        currentWave++;
        if (currentWave < waves.Length) StartCoroutine(nameof(StartWave));
    }

    IEnumerator SpawnEnemies(int index)
    {
        EnemyWave.Spawn spawner = waves[currentWave].enemySpawns[index];

        // if endless, mark spawner as finished
        if (spawner.endless) activeSpawners--;

        List<Transform> enemies = new List<Transform>();

        // delay spawning
        yield return new WaitForSeconds(spawner.startDelay);

        for(int i = 0; i < spawner.stages.Length; i++)
        {            
            for(int j = 0; j < spawner.stages[i]; j++)
            {
                // wait for a free spawner
                yield return new WaitForSeconds(spawner.spawnDelay);

                yield return new WaitUntil(() => { return freeSpawnPoints.Count > 0; });

                enemies.Add(SpawnEnemy(spawner.enemyPrefab, spawner.weaponType));
            }

            yield return new WaitUntil(() => { return CountNull(enemies) == enemies.Count; });

            // if endless, keep spawing forever :3
            if (spawner.endless && i == spawner.stages.Length - 1) i = -1;
        }

        activeSpawners--;
    }

    Transform SpawnEnemy(GameObject prefab, GunInfo gunType)
    {
        GameObject enemy = Instantiate(prefab, FetchRandomSpawnPoint().position, Quaternion.identity);

        if (gunType != null) enemy.GetComponentInChildren<HookTarget>().info = gunType;

        return enemy.transform;
    }

    Transform FetchRandomSpawnPoint()
    {
        int i = Random.Range(0, freeSpawnPoints.Count);
        Transform sp = freeSpawnPoints[i];
        freeSpawnPoints.Remove(sp);

        return sp;
    }

    int CountNull(List<Transform> transforms)
    {
        int nullCount = 0;
        
        foreach(Transform t in transforms)
        {
            if (!t) nullCount++;
        }

        return nullCount;
    }

}
