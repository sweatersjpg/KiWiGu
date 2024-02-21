using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveSystem : MonoBehaviour
{
    public Transform requiredCheckPoint;
    public GameObject spawnFX;

    public bool isEnding = false;

    [Space]
    [SerializeField] GameObject[] toEnable;

    [Space]
    [SerializeField] EnemyWave[] waves;

    [Space]
    [SerializeField] int currentWave = -1;
    int activeSpawners = 0;

    List<Transform> freeSpawnPoints;

    List<GameObject> enemyMasterList;
    
    // Start is called before the first frame update
    void Start()
    {
        ResetSpawnPoints();

        enemyMasterList = new List<GameObject>();

        InvokeRepeating(nameof(ResetSpawnPoints), 1, 1f);

        if (!requiredCheckPoint)
        {
            currentWave = 0;
            StartCoroutine(nameof(StartWave));
        }
        else currentWave = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!requiredCheckPoint) return;
        
        if(CheckPointSystem.spawnPoint == requiredCheckPoint.position && currentWave < 0)
        {
            currentWave = 0;
            StartCoroutine(nameof(StartWave));
        }

        if(CheckPointSystem.spawnPoint != requiredCheckPoint.position && currentWave >= 0)
        {
            //StopAllCoroutines();
            //currentWave = -1;
            ResetWaves();
        }

        if (currentWave >= waves.Length) Destroy(gameObject);
    }

    public void ResetWaves()
    {
        StopAllCoroutines();
        ResetSpawnPoints();

        currentWave = -1;

        for(int i = enemyMasterList.Count-1; i >= 0; i--)
        {
            
            
            if (enemyMasterList[i])
            {
                
                Destroy(enemyMasterList[i]);
            }
        }

        enemyMasterList = new List<GameObject>();
    }

    void ResetSpawnPoints()
    {
        if (currentWave < 0) return;

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

        // Debug.Log(activeSpawners);

        yield return new WaitUntil(() => { return activeSpawners == 0; });

        for(int i = 0; i < spawners.Length; i++)
        {
            StopCoroutine(spawners[i]);
        }

        // execute next wave
        currentWave++;
        if (currentWave < waves.Length) StartCoroutine(nameof(StartWave));
        else if(isEnding) SceneManager.LoadScene(1);
        else
        {
            for (int i = 0; i < toEnable.Length; i++) toEnable[i].SetActive(true);
        }
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

                enemies.Add(StartEnemySpawn(spawner.enemyPrefab, spawner.weaponType));
            }

            yield return new WaitUntil(() => { return CountNull(enemies) == enemies.Count; });

            // if endless, keep spawing forever :3
            if (spawner.endless && i == spawner.stages.Length - 1) i = -1;
        }

        activeSpawners--;
    }

    Transform StartEnemySpawn(GameObject prefab, GunInfo gunType)
    {
        Vector3 spawn = FetchRandomSpawnPoint().position;

        Instantiate(spawnFX, spawn, Quaternion.identity);

        GameObject enemy = Instantiate(prefab, spawn, Quaternion.identity);

        if (gunType != null) enemy.GetComponentInChildren<HookTarget>().info = gunType;

        // StartCoroutine(nameof(EnableEnemy), enemy);
        enemyMasterList.Add(enemy);

        return enemy.transform;
    }

    IEnumerable EnableEnemy(GameObject enemy)
    {
        yield return new WaitForSeconds(0.2f);
        enemy.SetActive(true);
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
