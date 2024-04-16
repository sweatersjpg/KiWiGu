using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookEnsure : MonoBehaviour
{
    public GameObject hookPrefab;

    public GameObject debugGun;
    
    // Start is called before the first frame update
    void Start()
    {
        
        for(int i = 0; i < transform.childCount; i ++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeSelf) Destroy(child.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount <= 0) Instantiate(hookPrefab, transform);

        if(Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.G) && transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
            Instantiate(debugGun, transform);
        }
    }
}
