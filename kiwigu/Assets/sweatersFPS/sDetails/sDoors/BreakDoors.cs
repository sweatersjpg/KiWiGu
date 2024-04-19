using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakDoors : MonoBehaviour
{
    
    // Start is called before the first frame update
    [SerializeField] float health = 35;
    [SerializeField] float breakForce = 40;

    [Space]
    [SerializeField] GameObject[] doors;

    float dhTimer;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (CheckDoubleHooked())
        {
            dhTimer = 0.3f;

            for (int i = 0; i < doors.Length; i++) doors[i].layer = LayerMask.NameToLayer("PhysicsObject");

        }
        else dhTimer = Mathf.Max(0, dhTimer - Time.deltaTime);

        if(dhTimer <= 0) for (int i = 0; i < doors.Length; i++) doors[i].layer = LayerMask.NameToLayer("Default");
    }

    void TakeDamage(object[] args)
    {
        Vector3 point = (Vector3)args[0];
        Vector3 direction = (Vector3)args[1];
        float damage = (float)args[2];

        if (dhTimer <= 0) return;

        health -= damage;

        if (health < 0) Break(point, direction);
    }

    bool CheckDoubleHooked()
    {
        MoveHook mh = GetComponentInChildren<MoveHook>();

        return mh && mh.childHook != null;
    }

    void Break(Vector3 point, Vector3 direction)
    {
        for(int i = 0; i < doors.Length; i++)
        {
            GameObject door = doors[i];

            door.layer = LayerMask.NameToLayer("PhysicsObject");
            door.tag = "RigidTarget";

            door.transform.parent = null;

            //door.tag = "Untagged";

            Rigidbody rb = door.AddComponent<Rigidbody>();
            door.AddComponent<DespawnTimer>().lifetime = 1;

            door.GetComponent<PhysicsHit>().enabled = true;

            rb.mass = 5;

            rb.AddForceAtPosition(direction.normalized * breakForce, point, ForceMode.Impulse);
        }

        MoveHook mh = GetComponentInChildren<MoveHook>();
        if (mh)
        {
            mh.transform.parent = null;
            mh.PullbackWithForce(0, 1);
        }

        Destroy(gameObject);
    }
}
