using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetScene : MonoBehaviour
{
    // Start is called before the first frame update

    public static ResetScene instance;

    [SerializeField] GameObject playerDummy;

    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerDeath(bool fromFall)
    {
        sweatersController.instance.gameObject.SetActive(false);

        Instantiate(playerDummy, sweatersController.instance.transform.position, 
            sweatersController.instance.playerCamera.transform.rotation);

        Invoke(nameof(Reload), fromFall ? 2f : 4f);
    }

    public void Reload()
    {
        sweatersController.instance.gameObject.SetActive(true);
        sweatersController.instance.ResetPlayer();

        sweatersController.instance.transform.GetComponent<PlayerHealth>().ResetHealth();

        // delete death cams
        DeathCam[] deathCams = FindObjectsByType<DeathCam>(FindObjectsSortMode.None);
        for (int i = 0; i < deathCams.Length; i++) Destroy(deathCams[i].gameObject);

        // reset waves
        WaveSystem[] waveSystems = FindObjectsByType<WaveSystem>(FindObjectsSortMode.None);
        for (int i = 0; i < waveSystems.Length; i++) waveSystems[i].ResetWaves();

        // reset player hands
        GunHand[] guns = FindObjectsByType<GunHand>(FindObjectsSortMode.None);
        for (int i = 0; i < guns.Length; i++) guns[i].SwapToHook();

        ThrowHook[] hookShots = FindObjectsByType<ThrowHook>(FindObjectsSortMode.None);
        for (int i = 0; i < hookShots.Length; i++) hookShots[i].CatchHook(null, null);

        MoveHook[] hooks = FindObjectsByType<MoveHook>(FindObjectsSortMode.None);
        for (int i = 0; i < hooks.Length; i++) Destroy(hooks[i].gameObject);
    }
}
