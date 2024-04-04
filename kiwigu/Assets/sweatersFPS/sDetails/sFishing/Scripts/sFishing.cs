using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class sFishing : MonoBehaviour
{
    public MoveHook hook;
    public GameObject splashParticles;

    public FishingLootTable lootTable;

    public float timeScale = 1;
    public float bobHeight = 0.5f;

    public float catchDuration = 2;
    public float fishChance = 0.5f;

    ParticleSystem splash;

    Vector3 startPoint;
    float bobTime = 0;
    float bob;

    bool fishCaught;
    
    // Start is called before the first frame update
    void Start()
    {
        hook = GetComponent<MoveHook>();

        splash = Instantiate(splashParticles, transform.position, Quaternion.Euler(-90, 0, 0)).GetComponent<ParticleSystem>();
        splash.Emit(30);

        startPoint = transform.position;

        StartCoroutine(FishChance());
        StartCoroutine(RandomSplashes());
    }

    // Update is called once per frame
    void Update()
    {
        Bob();
    }

    void Bob()
    {
        bobTime += Time.deltaTime * timeScale;

        float newBob = Mathf.PerlinNoise(bobTime, 0);
        if (fishCaught) newBob -= 2;

        float oldBob = bob;
        bob = Mathf.Lerp(bob, newBob, Time.deltaTime * 4);

        if (oldBob > 0.5f && bob <= 0.5f) splash.Emit(5);

        transform.position = startPoint + new Vector3(0,bob,0) * bobHeight;
    }

    IEnumerator FishChance()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);
            if (!fishCaught && Random.Range(0, 1f) < fishChance)
            {
                yield return FishCome();
                StartCoroutine(FishAppear());
                break;
            }
        }
    }

    IEnumerator FishCome()
    {
        Vector2 v2d = Random.insideUnitCircle;
        v2d = v2d.normalized * 6;

        Vector3 v = new Vector3(v2d.x, 0, v2d.y);

        float timeStep = 0.1f;
        float duration = 2f;
        float timer = 0;

        while(timer < duration)
        {
            timer += timeStep;
            
            Vector3 p = Vector3.Lerp(v, new Vector3(0, 0, 0), timer / duration);
            SpawnSplash(startPoint + p);

            yield return new WaitForSeconds(timeStep);
        }
    }

    IEnumerator FishAppear()
    {
        fishCaught = true;
        splash.Emit(30);

        int splashes = 5;
        for(int i = 0; i < splashes; i++)
        {
            splash.Emit(5);
            yield return new WaitForSeconds(catchDuration / splashes);
        }

        fishCaught = false;
        StartCoroutine(FishChance());
    }

    IEnumerator RandomSplashes()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(0.1f, 1f));
            RandomSplash();
        }
    }

    public void BeforeDestroy()
    {
        Destroy(splash, 1);
        splash.Emit(10);

        if (!fishCaught) return;

        GunInfo loot = lootTable.GetLoot();

        ThrownGun gun = Instantiate(loot.gunPrefab.GetComponent<GunHand>().thrownGunPrefab, 
            startPoint + Vector3.up * 1, Quaternion.Euler(-90, 0, 0)).GetComponent<ThrownGun>();
        gun.ammo = new Ammunition(loot.capacity);
        gun.info = loot;
        gun.throwForce = 8;
        gun.transform.localScale = new Vector3(4, 4, 4);

        GameObject p = Instantiate(hook.perfectHookFXprefab, transform);
        p.transform.parent = null;
        p.transform.localScale *= 4;
    }

    public void RandomSplash()
    {
        Vector2 v = Random.insideUnitCircle * 5;
        SpawnSplash(new Vector3(v.x, 0, v.y) + startPoint);
    }

    public void SpawnSplash(Vector3 pos)
    {
        pos.y = startPoint.y;
        ParticleSystem s = Instantiate(splashParticles, pos, Quaternion.Euler(-90, 0, 0)).GetComponent<ParticleSystem>();
        s.Emit(3);
        Destroy(s, 1);
    }
}
