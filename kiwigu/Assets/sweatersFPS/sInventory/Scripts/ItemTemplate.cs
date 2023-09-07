using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ItemTemplate", menuName = "ItemTemplate")]
public class ItemTemplate : ScriptableObject
{
    public string itemName = "Basic Item";
    public string itemDescription = "This is a basic item";

    public List<string> tags;

    [Space]
    public Vector2 spriteLocation;
    public Texture spriteSheet;

    [Space]
    public int itemMax = 0;
    public List<ItemTemplate> inventory;

    public List<string> blacklist;

}
