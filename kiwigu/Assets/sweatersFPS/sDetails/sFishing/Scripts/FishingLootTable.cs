using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New FishingLootTable", menuName = "FishingLootTable")]
public class FishingLootTable : ScriptableObject
{
    public Loot[] lootTable;

    [System.Serializable]
    public class Loot
    {
        public GunInfo prefab;
        public float weight = 1;

        public Loot(GunInfo prefab, float weight)
        {
            this.prefab = prefab;
            this.weight = weight;
        }
    }

    public GunInfo GetLoot()
    {
        Loot[] tempLoot = new Loot[lootTable.Length];
        
        // get cumulative weight
        float cum = 0;
        for(int i = 0; i < tempLoot.Length; i++)
        {
            cum += lootTable[i].weight;
            tempLoot[i] = new Loot(lootTable[i].prefab, cum);
        }

        float weightedIndex = Random.Range(0, cum);
        Loot pickedLoot = null;

        // get the loot with the smallest cumulative weight that is greater than the weighted index
        for (int i = tempLoot.Length-1; i >= 0; i--)
        {
            if (tempLoot[i].weight >= weightedIndex) pickedLoot = tempLoot[i];
            else break;
        }

        return pickedLoot.prefab;
    }
}
