using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MiniMenuSystem;

public class HookEnsure : MonoBehaviour
{
    public GameObject hookPrefab;

    public GameObject debugGun;

    public static GunInfo storedGun;
    
    // Start is called before the first frame update
    void Start()
    {
        
        for(int i = 0; i < transform.childCount; i ++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeSelf) Destroy(child.gameObject);
        }

        if(storedGun)
        {
            Instantiate(storedGun.gunPrefab, transform);

            storedGun = null;
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

    public void StoreGuns()
    {
        GunHand hand = transform.GetComponent<GunHand>();

        if (hand == null)
        {
            storedGun = null;
            return;
        }

        storedGun = hand.info;

        Debug.Log("Guns Stored");
    }
}
