using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class MoveHook : MonoBehaviour
{
    // Start is called before the first frame update

    public ThrowHook home;

    public float startingSpeed = 8;
    public float trackingAcceleration = 6;
    // public float deceleration = 32;

    public float catchDistance = 0.4f;

    bool canCatch = false;

    public float speed;
    public Vector3 velocity;

    float deltaTime = 0;

    Vector3 pPosition; // past position

    void Start()
    {
        speed = -startingSpeed;
        velocity = transform.forward * startingSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseSystem.paused)
        {
            deltaTime = 0;
            return;
        }
        else deltaTime = Time.deltaTime;

        pPosition = transform.position;

        Vector3 heading = home.transform.position - transform.position;

        if(canCatch && heading.magnitude < catchDistance)
        {
            home.CatchHook();
            Destroy(gameObject);
        } else if(heading.magnitude > catchDistance) {
            canCatch = true;
        }

        if(speed > 0) velocity = heading.normalized;

        speed += trackingAcceleration * deltaTime * 0.5f;
        velocity = velocity.normalized * Mathf.Abs(speed);
        transform.position += velocity * deltaTime;
        speed += trackingAcceleration * deltaTime * 0.5f;

        DoPhysics();
    }

    void DoPhysics()
    {
        // raycast from ppos to pos

        bool hasHit = Physics.Raycast(pPosition, transform.position - pPosition, 
            out RaycastHit hit, (transform.position - pPosition).magnitude, ~LayerMask.GetMask("GunHand", "Player"));

        if(hasHit)
        {
            ResolveCollision(hit);
        }

    }

    void ResolveCollision(RaycastHit hit)
    {
        if (speed > 0) return;

        transform.position = hit.point;

        Vector3 d = velocity;
        Vector3 n = hit.normal;

        Vector3 r = d - 2 * Vector3.Dot(d, n) * n;

        velocity = r;
    }
}
