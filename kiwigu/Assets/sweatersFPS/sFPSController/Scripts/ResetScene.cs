using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetScene : MonoBehaviour
{
    // Start is called before the first frame update

    public static ResetScene instance;

    [SerializeField] GameObject playerDummy;
    [SerializeField] GameObject barrelPrefab;

    Vector3[] barrelPositions;

    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {

        ExplosiveBarrel[] ebs = FindObjectsByType<ExplosiveBarrel>(FindObjectsSortMode.None);
        barrelPositions = new Vector3[ebs.Length];
        for(int i = 0; i < ebs.Length; i++)
        {
            barrelPositions[i] = ebs[i].transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerDeath(bool fromFall)
    {
        sweatersController.instance.gameObject.SetActive(false);

        GameObject dummy = Instantiate(playerDummy, sweatersController.instance.transform.position, 
            sweatersController.instance.playerCamera.transform.rotation);

        // dummy.GetComponentInChildren<Rigidbody>().AddForce(sweatersController.instance.velocity, ForceMode.VelocityChange);
        Rigidbody[] rbs = dummy.GetComponentsInChildren<Rigidbody>();
        for(int i = 0; i < rbs.Length; i++)
        {
            rbs[i].AddForce(sweatersController.instance.velocity * Random.Range(0,1f), ForceMode.VelocityChange);
        }

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

        // reset all explosive barrels
        ExplosiveBarrel[] ebs = FindObjectsByType<ExplosiveBarrel>(FindObjectsSortMode.None);
        for (int i = 0; i < ebs.Length; i++) Destroy(ebs[i].gameObject);
        for (int i = 0; i < barrelPositions.Length; i++) Instantiate(barrelPrefab, barrelPositions[i], Quaternion.identity);

    }

}
