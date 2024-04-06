using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillVolume : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // SceneManager.LoadScene(0);
            ResetScene.instance.PlayerDeath(true);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // SceneManager.LoadScene(0);
            ResetScene.instance.PlayerDeath(true);
        }
    }
}
