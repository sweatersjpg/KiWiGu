using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[Serializable]
public class Ammunition
{
    public float capacity;
    public float count;

    public Ammunition(float capacity)
    {
        count = capacity;
        this.capacity = capacity;
    }
}
