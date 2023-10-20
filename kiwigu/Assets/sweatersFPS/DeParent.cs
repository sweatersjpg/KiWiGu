using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeParent : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        transform.DetachChildren();

        Destroy(gameObject);
    }
}
