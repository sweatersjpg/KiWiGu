using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyWave
{

    public float startDelay;
    public Transform SpawnPoints;

    public bool ignoreWaveForTesting = false;

    [Space]
    public Spawn[] enemySpawns;

    [System.Serializable]
    public struct Spawn
    {
        public float startDelay;
        public float spawnDelay;
        public Transform[] customSpawnPoints;
        public int[] stages;
        public bool endless;
        public bool notRequired;
        public GameObject enemyPrefab;
        public GunInfo weaponType;
    }
}
