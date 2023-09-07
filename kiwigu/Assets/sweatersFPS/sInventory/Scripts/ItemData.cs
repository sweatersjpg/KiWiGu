using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "New ItemData", menuName = "ItemData")]
public class ItemData
{
    public string itemName = "Basic Item";
    public string itemDescription = "This is a basic item";

    public List<string> tags;

    public Vector2 spriteLocation;
    public Texture spriteSheet;

    public int itemMax = 0;
    public List<ItemData> inventory;

    public List<string> blacklist;

    public int itemIndex = -1; // if in inventory stay at a specific index

    public ItemData (string name, string description, int capacity, Texture spriteSheet, int spritex, int spritey)
    {
        inventory = new List<ItemData>();

        itemMax = capacity;
        itemName = name;
        itemDescription = description;
        this.spriteSheet = spriteSheet;
        spriteLocation = new Vector2(spritex, spritey);

        itemIndex = -1;
    }

    public ItemData(ItemTemplate data) // new data from scriptable object
    {
        itemName = string.Copy(data.itemName);
        itemDescription = string.Copy(data.itemDescription);
        spriteSheet = data.spriteSheet;
        spriteLocation = new Vector2(data.spriteLocation.x, data.spriteLocation.y);

        tags = new List<string>();
        foreach(string s in data.tags)
        {
            tags.Add(s);
        }

        blacklist = new List<string>();
        foreach (string s in data.blacklist)
        {
            blacklist.Add(s);
        }

        inventory = new List<ItemData>();
        foreach (ItemTemplate i in data.inventory) inventory.Add(new ItemData(i));

        itemMax = data.itemMax;

    }

    public bool AddItem(ItemData item)
    {
        if (inventory.Count < itemMax)
        {
            inventory.Add(item);
            return true;
        } else return false;
    }

    public void RemoveItem(ItemData item)
    {
        inventory.Remove(item);
    }

    public bool CompareTag(string tag)
    {
        return tags.Contains(tag);
    }

    public bool Blacklisted(List<string> tags)
    {
        foreach (string tag in tags)
        {
            if (blacklist.Contains(tag)) return true;
        }
        return false;
    }
}
