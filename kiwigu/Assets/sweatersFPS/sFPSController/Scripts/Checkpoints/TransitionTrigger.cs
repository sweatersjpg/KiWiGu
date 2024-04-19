using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionTrigger : MonoBehaviour
{

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            HookEnsure[] es = sweatersController.instance.gameObject.GetComponentsInChildren<HookEnsure>();

            for(int i = 0; i < es.Length; i++)
            {
                es[i].StoreGuns();
            }
            
            CheckPointSystem.spawnPoint = new();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
