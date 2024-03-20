using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryDotExe : ScreenProgram
{

    public GameObject itemPrefab;

    Container back;
    Container belt;
    Container pants;
    Container LHand;
    Container RHand;

    public Panel inventory;
    public Proximity proximity;

    public ItemData heldItem;
    public Container heldItemContainer;

    public override void Setup()
    {
        Inventory.I = this;
        Proximity.I = this;
        Container.I = this;
        Panel.I = this;

        back = new Container(null, new Vector2(16, 2));

        belt = new Container(null, new Vector2(16, 8));
        pants = new Container(null, new Vector2(16, 12));

        LHand = new Container(null, new Vector2(16, 22));
        RHand = new Container(null, new Vector2(16, 26));

        inventory = new Panel();
        proximity = new Proximity();

        proximity.GetItems();

        // temp items
        //ItemData canObeans = new ItemData("can-O-beans", "rhymes with tangerines", 0, spriteSheet, 96, 32);
        //ItemData drinkItem = new ItemData("drinky-drink", "yummy", 0, spriteSheet, 160, 32);
        //ItemData lighter = new ItemData("ligher", "it's almost empty", 0, spriteSheet, 64, 64);

        //ItemData gunItem = new ItemData("hunting rifle", "worn out hunting riffle", 0, spriteSheet, 64, 32);
        //ItemData bulletsItem = new ItemData("bullets", ".243 ammo for a hunting riffle", 0, spriteSheet, 192, 32);
        //ItemData hatchetItem = new ItemData("hatchet", "it's a bit dull", 0, spriteSheet, 32, 64);

        //ItemData backpackItem = new ItemData("backpack", "holds a number of items", 15, spriteSheet, 32, 32);
        //backpackItem.AddItem(canObeans);
        //backpackItem.AddItem(drinkItem);
        //backpackItem.AddItem(gunItem);

        //ItemData beltItem = new ItemData("belt", "holds a number of items", 6, spriteSheet, 224, 32);
        //beltItem.AddItem(hatchetItem);
        //beltItem.AddItem(bulletsItem);

        //ItemData pantsItem = new ItemData("jeans", "a pair of worn jeans", 4, spriteSheet, 0, 64);
        //pantsItem.AddItem(lighter);

        //back.item = backpackItem;
        //belt.item = beltItem;
        //pants.item = pantsItem;

    }

    public override void Resume()
    {
        foreach(Container c in proximity.containers)
        {
            Inventory inv = inventory.GetInventory(c.item);
            if (inv != null) inv.Close();
        }

        proximity.GetItems(); // get items in proximity

        //inventory.inventories = new List<Inventory>();
    }

    public override void Draw()
    {
        mouseIcon = ARROW;

        R.lset(2);

        drawBox(1, 1, 13, 29, true, "proximity"); // prox

        drawBox(21, 1, 17, 29, true, "inventory"); // inventories

        R.lset(1);

        // Containers
        drawBox(15, 1, 5, 5, true, "back");
        drawBox(15, 7, 5, 13, true, "pants");
        drawBox(15, 21, 5, 9, true, "hands");

        if(heldItem != null && mouseButtonUp)
        {
            // mouse is overtop the body inventories
            if(mouse.x > 124 && mouse.y > 12 && mouse.x < 164 && mouse.y < 164)
            {
                if (!TryEquip(heldItem)) heldItemContainer.AddItem(heldItem);
            }
        }

        back.Draw();
        belt.Draw();
        pants.Draw();
        LHand.Draw();
        RHand.Draw();

        inventory.Draw(22 * 8, 2 * 8);

        proximity.Draw(16, 16);

        if(heldItem != null)
        {
            R.lset(3);

            R.spr(heldItem.spriteLocation.x, heldItem.spriteLocation.y, mouse.x - 16, mouse.y - 16, 32, 32);
            mouseIcon = HAND_CLOSED;

            if(mouseButtonUp)
            {
                heldItemContainer.AddItem(heldItem);
            }
        }

        R.lset(3);
    }

    public bool TryEquip(ItemData item)
    {
        if (item.CompareTag("Backpack")) back.AddItem(heldItem);
        else if (item.CompareTag("Pants")) pants.AddItem(heldItem);
        else if (item.CompareTag("Belt")) belt.AddItem(heldItem);
        else return false;

        return true;
    }

    // put item where it 'should' go (if it fits)
    // tools should try the belt first before backpack
    // other items go into the backpack fist before the pants
    public void AutoDeposit(ItemData item)
    {

    }

    public class Proximity
    {
        public static InventoryDotExe I;

        public List<Container> containers;

        float scrollY;

        public void Draw(float x, float y)
        {

            if (I.mouse.x < 144) scrollY += I.mouseScrollDelta;

            float h = ((int)(containers.Count - 1) / 3 + 1) * 32;

            if (scrollY + h < 224) scrollY = 224 - h;
            if (scrollY > 0) scrollY = 0;

            y += scrollY;

            for(int i = containers.Count-1; i >= 0; i--)
            {
                int X = i % 3;
                int Y = i / 3;

                containers[i].Draw(x + X * 32, y + Y * 32);
                containers[i].Update(x + X * 32, y + Y * 32);
            }

            // if holding item that is not already real item
            if(I.heldItem != null && I.heldItemContainer.worldItem == null && I.mouseButtonUp && MouseOver())
            {
                AddItem(I.heldItem);

                I.heldItem = null;
                I.heldItemContainer = null;

                GetItems();
            }
        }

        public void GetItems() // gets items within proximity
        {
            // get gameObjects in 'items' layer
            // create required # of containers
            // populate containers w/ items
            if (!sweatersController.instance) return;
            Vector3 pos = sweatersController.instance.transform.position;

            Collider[] colliders = Physics.OverlapSphere(pos, 4, LayerMask.GetMask("Items"));

            System.Array.Sort(colliders,
                (Collider a, Collider b) =>
                (pos - a.transform.position).sqrMagnitude.CompareTo((pos - b.transform.position).sqrMagnitude));

            containers = new List<Container>();

            foreach(Collider collider in colliders)
            {
                containers.Add(new Container(collider.gameObject));
            }
        }

        public void RemoveItem(Container container) // removes item from world
        {
            Destroy(container.worldItem);
            containers.Remove(container);
        }

        public void AddItem(ItemData data) // adds item to world
        {
            // instantiate item
            // assign itemData

            GameObject item = Instantiate(I.itemPrefab, sweatersController.instance.transform);
            item.transform.parent = null;

            item.GetComponent<ItemDisplay>().item = data;
        }

        public bool MouseOver()
        {
            bool x = I.mouse.x > 8 && I.mouse.x < 120;
            bool y = I.mouse.y > 8 && I.mouse.y < 248;
            return x && y;
        }

    }

    public class Inventory // visual inventory (not inventory item type)
    {
        public static InventoryDotExe I;
        public List<Container> containers;

        public Panel parent;

        public ItemData data;

        float ypos = 0;
        bool minimized = false;

        public Inventory(Panel parent, ItemData data)
        {
            this.parent = parent;
            this.data = data;

            containers = new List<Container>();

            // create containers
            for(int i = 0; i < data.itemMax; i++)
            {
                Container c = new Container(this);
                //if (i < data.inventory.Count) c.item = data.inventory[i];
                containers.Add(c);
            }

            // add items to correct containers
            for (int i = 0; i < data.inventory.Count; i++)
            {
                // edge cases to worry about:
                // - index is already in use
                // - index out of bounds
                if (data.inventory[i].itemIndex >= 0)
                {
                    containers[data.inventory[i].itemIndex].item = data.inventory[i];
                }
            }

            // add un-indexed items into remaining slots
            for (int i = 0; i < data.inventory.Count; i++)
            {
                if (data.inventory[i].itemIndex != -1) continue;

                FitItem(data.inventory[i]);

            }
        }

        public float Draw(float x, float startY)
        {
            ypos += (startY - ypos) / 2;
            if (Mathf.Abs(startY - ypos) < 1.5) ypos = startY;

            float y = ypos;

            for (int i = 0; i < 16; i++) I.R.spr(8, 0, x + i * 8, y, 8, 8);
            I.R.put(data.itemName, (int) (x + 8), (int) y);

            I.R.spr(40, minimized ? 0 : 8, x+104, y, 9, 8);

            if ((I.mouse - new Vector2(x + 104 + 4, y + 4)).magnitude < 4)
            {
                I.mouseIcon = HAND_POINTER;
                if (I.mouseButtonDown)
                {
                    minimized = !minimized;
                    I.mouseIcon = HAND_CLOSED;
                }
            }

            I.R.put("x", x + 112, y);

            if ((I.mouse - new Vector2(x + 112 + 4, y + 4)).magnitude < 4)
            {
                I.mouseIcon = HAND_POINTER;
                if (I.mouseButtonDown)
                {
                    Close();
                    I.mouseIcon = HAND_CLOSED;
                    return y;
                }
            }

            y += 10;
            startY += 10;

            I.R.spr(48, 0, x, y - 2, 24, 24, false, 16 * 8, ((int)containers.Count / 4 + 1) * 32 + 2); // background (hides inventories behind it

            for (int i = containers.Count-1; i >= 0; i--)
            {
                int X = i % 4;
                int Y = i / 4;

                containers[i].Draw(x + X * 32, y + Y * 32);
                if (!minimized) containers[i].Update(x + X * 32, y + Y * 32);
            }

            if (minimized) return startY;
            startY += ((int) (containers.Count-1) / 4 + 1) * 32;

            return startY;
        }

        public void AddItem(ItemData item)
        {
            data.AddItem(item);
        }

        public bool Blacklisted(ItemData item)
        {
            return data.Blacklisted(item.tags);
        }

        public bool FitItem(ItemData item)
        {
            for (int i = 0; i < containers.Count; i++)
            {
                if (containers[i].item == null)
                {
                    item.itemIndex = i;
                    containers[i].item = item;

                    return true;
                }
            }

            return false;
        }

        public void RemoveItem(ItemData item)
        {
            data.RemoveItem(item);
        }

        public void Close()
        {
            parent.inventories.Remove(this);
        }

    }

    public class Container
    {
        public static InventoryDotExe I;

        public Inventory parent;
        public GameObject worldItem;

        public ItemData item = null;
        // public int quantity

        public Vector2 pos;

        public Container(GameObject worldItem)
        {
            this.worldItem = worldItem;
            this.item = worldItem.GetComponent<ItemDisplay>().item;
        }

        public Container(Inventory parent, Vector2 pos)
        {
            this.parent = parent;
            if (pos != null) this.pos = pos * 8;
        }

        public Container(Inventory parent)
        {
            this.parent = parent;
        }

        public void Draw()
        {
            Draw(pos.x, pos.y);
            Update(pos.x, pos.y);
        }

        public void Draw(float x, float y)
        {
            I.R.spr(0, 32, x, y, 32, 32);

            if (item == null) return;

            I.R.spr(item.spriteLocation.x, item.spriteLocation.y, x, y, 32, 32);

        }

        public void Update(float x, float y)
        {

            if (I.heldItem != null && MouseOver(x, y, 32) && I.mouseButtonUp) // drop item
            {
                AddItem(I.heldItem);
            }

            if (item == null) return;

            if (item.itemMax > 0)
            {
                Inventory inv = I.inventory.GetInventory(item);

                I.R.spr(40, (inv != null) ? 8 : 0, x + 23, y + 24, 9, 8);

                if ((I.mouse - new Vector2(x + 23 + 4, y + 24 + 4)).magnitude < 4)
                {
                    I.mouseIcon = HAND_POINTER;
                    if (I.mouseButtonDown)
                    {
                        if (inv != null) inv.Close();
                        else I.inventory.AddInventory(item);
                        I.mouseIcon = HAND_CLOSED;
                    }
                }
            }

            if (MouseOver(x, y, 26))
            {
                I.mouseIcon = HAND_OPEN;

                int layer = I.R.lget();
                I.R.lset(4);
                I.put(item.itemName, 2, 30);
                I.put(item.itemDescription, 1, 31);
                I.R.lset(layer);
            }

            if(MouseOver(x,y, 26) && I.mouseButtonDown) // pickup item
            {
                RemoveItem();

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    I.TryEquip(I.heldItem);
                }
            }

        }

        public bool MouseOver(float x, float y, float width)
        {
            bool X = I.mouse.x > x && I.mouse.x < x + width;
            bool Y = I.mouse.y > y && I.mouse.y < y + width;
            return X && Y;
        }

        public void RemoveItem()
        {
            I.heldItem = item;
            I.heldItemContainer = this;

            item.itemIndex = -1;

            if (parent != null) parent.RemoveItem(item);
            item = null;
        }

        public void AddItem(ItemData item)
        {
            if (this.item != null)
            {

                if(item.itemMax == 0 && this.item.itemMax > 0)
                {
                    if(this.item.AddItem(item))
                    {
                        Inventory inv = I.inventory.GetInventory(this.item);
                        if (inv != null) inv.FitItem(item);

                        if (I.heldItemContainer.worldItem != null && I.heldItemContainer != this)
                        {
                            I.proximity.RemoveItem(I.heldItemContainer);
                        }

                        I.heldItem = null;
                        I.heldItemContainer = null;
                    }
                }

                return; // can't put item onto another item
            }

            // if (I.heldItem.itemMax > 0 && parent != null) return; // can't put inventories inside inventories
            
            // if a tag is blacklisted don't deposit item
            if (parent != null && parent.Blacklisted(item)) return;            

            this.item = item;
            if (parent != null)
            {
                parent.AddItem(item);
                item.itemIndex = parent.containers.IndexOf(this);
            }

            if(I.heldItemContainer.worldItem != null && I.heldItemContainer != this)
            {
                I.proximity.RemoveItem(I.heldItemContainer);
            }

            I.heldItem = null;
            I.heldItemContainer = null;
        }
        
    }

    public class Panel
    {
        public static InventoryDotExe I;

        //public List<Item> items;
        public List<Inventory> inventories;

        float scrollY;

        float endY;
        float paddingY = 0;

        public Panel()
        {
            inventories = new List<Inventory>();
        }

        public void Draw(float x, float y)
        {
            if(I.mouse.x > 144) scrollY += I.mouseScrollDelta;

            if (endY + scrollY < 240) scrollY = 240 - (endY);
            if (scrollY > 0) scrollY = 0;

            y = y+scrollY;

            for (int i = inventories.Count - 1; i >= 0; i--)
            {
                y = inventories[i].Draw(x, y);
            }

            paddingY += (y - paddingY) / 2;
            if (Mathf.Abs(y - paddingY) < 2) paddingY = y;

            I.R.spr(48, 0, x, paddingY, 24, 24, false, 16 * 8, 31*8);

            //if (y < 240) scrollY = 240 - (y - scrollY);
            endY = y - scrollY;

        }

        public void AddInventory(ItemData item)
        {
            inventories.Add(new Inventory(this, item));
        }

        public Inventory GetInventory(ItemData item)
        {
            foreach (Inventory i in inventories)
            {
                if (i.data == item) return i;
            }

            return null;
        }
    }

}
