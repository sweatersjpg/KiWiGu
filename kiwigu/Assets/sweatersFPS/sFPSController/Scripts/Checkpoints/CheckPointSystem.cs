using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSystem : MonoBehaviour
{
    public bool startingSpawn = false;

    public static Vector3 spawnPoint;
    public static Vector3 spawnDirection;

    int index;
    float timer;

    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log(spawnPoint);

        if (spawnPoint.Equals(new Vector3()) && startingSpawn)
        {
            spawnPoint = transform.position;
            spawnDirection = transform.localEulerAngles;
            // DontDestroyOnLoad(this);
        }
    }


    private void Start()
    {
        index = FindIndex();
    }

    private void Update()
    {
        int i = index;

        if (Input.GetKey(KeyCode.Alpha0)) i -= 9;
        
        if (Input.GetKeyDown((KeyCode)49 + i))
        {
            if(timer > 0)
            {
                spawnPoint = transform.position;
                spawnDirection = transform.localEulerAngles;
                PauseSystem.pauseSystem.ReloadScene();
            }
            timer = 0.2f;
        }

        timer = Mathf.Max(timer - Time.unscaledDeltaTime, 0);
    }

    int FindIndex()
    {
        for(int i = 0; i < transform.parent.childCount; i++)
        {
            if (transform.parent.GetChild(i).Equals(transform)) return i;
        }

        return -1;
    }

    //private void OnDestroy()
    //{
    //    spawnPoint = new Vector3();
    //}

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Spawnpoint Set");

            spawnPoint = transform.position;
            spawnDirection = transform.localEulerAngles;
        }
    }
}
