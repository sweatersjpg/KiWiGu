using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCam : MonoBehaviour
{
    // Start is called before the first frame update
    Transform target;

    Vector3 targetPosition;

    public float keepDistance = 5;

    public float speed = 10;

    void Start()
    {
        target = transform.parent;
        transform.parent = null;

        transform.position = target.position - transform.forward * 0.5f;
        targetPosition = transform.position - transform.forward * keepDistance;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(target);

        Vector3 newPos = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);

        bool hit = Physics.Raycast(transform.position, (targetPosition - transform.position), 
            Vector3.Distance(transform.position, newPos));

        if (!hit) transform.position = newPos;
    }
}
