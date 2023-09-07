using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDisplay : MonoBehaviour
{

    public ItemTemplate template;

    public float force = 300;

    public ItemData item;

    MiniRenderer R;

    Rigidbody rb;

    // Start is called before the first frame update
    void Awake()
    {
        //item = new ItemData("hatchet", "it's a bit dull", 0, spriteSheet, 32, 64);

        rb = GetComponent<Rigidbody>();

        if (template != null) item = new ItemData(template); // if in level use template and don't move
        else
        {
            Vector3 dir = Random.insideUnitSphere;
            dir.y = Mathf.Abs(dir.y);

            rb.AddForce(dir * force);
            rb.AddTorque(dir * force);
        }
    }

    void Update()
    {
        //if(Input.GetMouseButtonDown(0))
        //{
        //    Vector3 dir = Random.insideUnitSphere;
        //    dir.y = Mathf.Abs(dir.y);

        //    rb.AddForce(dir * force);
        //    rb.AddTorque(dir * force);
        //}
    }

    void Init(MiniRenderer mr)
    {
        R = mr;
        //R.spriteSheet = item.spriteSheet;
    }

    void FrameUpdate()
    {
        R.spr(item.spriteLocation.x, item.spriteLocation.y, 0, 0, 32, 32);

        R.Display();
    }
}
