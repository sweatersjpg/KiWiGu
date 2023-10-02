using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchingBag : MonoBehaviour
{
    LineRenderer Line;
    public Transform target;

    private void Start()
    {
        Line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        Line.SetPosition(0, transform.position);
        Line.SetPosition(1, target.position);
    }
}
