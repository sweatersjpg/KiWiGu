using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingWire : MonoBehaviour
{

    [SerializeField] int segments = 20;
    // [SerializeField] float drop = 3;

    [SerializeField] float tension = 0.5f;
    [SerializeField] float gravity = 1;
    [SerializeField] float dampener = 0.8f;

    [Space]

    [SerializeField] Transform ATarget;
    [SerializeField] Transform BTarget;

    LineRenderer wire;

    float[] velocities;
    
    // Start is called before the first frame update
    void Start()
    {
        wire = GetComponent<LineRenderer>();

        wire.useWorldSpace = ATarget && BTarget;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int count = wire.positionCount;

        while (wire.positionCount < segments) wire.positionCount++;
        while (wire.positionCount > segments && segments >= 2) wire.positionCount--;

        if(count != wire.positionCount)
        {
            velocities = new float[wire.positionCount];
            for (int i = 0; i < wire.positionCount; i++) velocities[i] = 0;
        }

        if (ATarget) wire.SetPosition(wire.positionCount - 1, ATarget.position);
        else wire.SetPosition(wire.positionCount - 1, new Vector3());

        if (BTarget) wire.SetPosition(0, BTarget.position);

        for (int i = 1; i < wire.positionCount-1; i++)
        {
            float y = wire.GetPosition(i).y;
            float nHeights = (wire.GetPosition(i - 1).y-y) + (wire.GetPosition(i + 1).y-y);

            velocities[i] += nHeights * tension;
            velocities[i] -= gravity;

            velocities[i] *= dampener;

            Vector3 start = wire.GetPosition(0);
            Vector3 v = wire.GetPosition(wire.positionCount - 1) - start;

            Vector3 newP = start + v.normalized * v.magnitude / (wire.positionCount- 1) * i;
            newP.y = y + velocities[i] * Time.fixedDeltaTime;

            // Vector3 p = new Vector3(0, velocities[i] * Time.fixedDeltaTime, 0);

            wire.SetPosition(i, newP);
        }
    }
}
