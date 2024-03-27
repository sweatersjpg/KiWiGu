using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCheckPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        CheckPointSystem.spawnPoint = new Vector3();
        Destroy(gameObject);
    }

}
