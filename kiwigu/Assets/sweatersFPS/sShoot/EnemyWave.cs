using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyWave
{

    public float startDelay;
    public Transform SpawnPoints;

    [Space]
    public Spawn[] enemySpawns;

    [System.Serializable]
    public struct Spawn
    {
        public float startDelay;
        public float spawnDelay;
        public int[] stages;
        public bool endless;
        public GameObject enemyPrefab;
        public GunInfo weaponType;
    }
}
