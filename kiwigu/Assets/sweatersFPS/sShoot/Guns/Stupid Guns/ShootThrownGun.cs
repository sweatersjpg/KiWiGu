using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootThrownGun : MonoBehaviour
{
    GameObject thrownObject;
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject thrown = Instantiate(thrownObject, transform);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
