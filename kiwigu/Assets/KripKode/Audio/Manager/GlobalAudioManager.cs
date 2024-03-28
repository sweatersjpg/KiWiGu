using UnityEngine;
using UnityEngine.Audio;
using static MiniMenuSystem;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager instance;
    public AudioMixer globalMixer;

    // Sounds
    public AudioClip headshotSFX;
    public AudioClip[] explosionsSFX;

    // Variables
    [Space(10)]
    public AudioSource battleSourceA;
    public AudioClip battleStemOne;
    public AudioSource battleSourceB;
    public AudioClip battleStemTwo;
    public bool battleTrigger = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        battleSourceA.clip = battleStemOne;
        battleSourceB.clip = battleStemTwo;
        battleSourceA.Play();
        battleSourceB.Play();
        battleSourceA.volume = 0.5f;
        battleSourceB.volume = 0;
    }

    private void Update()
    {
        PlayBattleMusic();
    }

    private void PlaySound(Transform location, AudioClip clip, float volume, float pitch, float range, string reference)
    {
        GameObject soundObject = new GameObject(reference);
        soundObject.transform.position = location.position;
        soundObject.transform.parent = transform;

        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = globalMixer.FindMatchingGroups("SFX")[0];
        audioSource.spatialBlend = 1;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.maxDistance = range;

        audioSource.Play();

        Destroy(soundObject, clip.length);
    }

    public void PlayHeadshotSFX(Transform location)
    {
        PlaySound(location, headshotSFX, 1, 1, 75, "headshotSFX");
    }

    public void PlayGunFire(Transform location, GunInfo info)
    {
        if (info.shootSound == null) return;
        PlaySound(location, info.shootSound, 0.5f, Random.Range(0.9f, 1.05f), 75, "shootSFX");
    }

    public void PlayGunEmpty(Transform location, GunInfo info)
    {
        if (info.emptyMagSFX == null) return;
        PlaySound(location, info.emptyMagSFX, 1, 0.5f, 25, "emptyGunSFX");
    }

    public void PlayBattleMusic()
    {
        if (battleTrigger)
        {
            battleSourceA.volume = Mathf.Clamp(battleSourceA.volume - Time.deltaTime * 0.5f, 0, 0.5f);
            battleSourceB.volume = Mathf.Clamp(battleSourceB.volume + Time.deltaTime * 0.5f, 0, 0.5f);
        }
        else
        {
            battleSourceA.volume = Mathf.Clamp(battleSourceA.volume + Time.deltaTime * 0.15f, 0, 0.5f);
            battleSourceB.volume = Mathf.Clamp(battleSourceB.volume - Time.deltaTime * 0.15f, 0, 0.5f);
        }

        Transform playerTransform = sweatersController.instance.transform;
        Collider[] enemies = Physics.OverlapSphere(playerTransform.position, 25, 1 << LayerMask.NameToLayer("Enemy"));
        if (enemies.Length > 0)
            battleTrigger = true;
        else
            battleTrigger = false;
    }

    public void PlayExplosion(Transform location)
    {
        AudioClip explosionSFX = explosionsSFX[Random.Range(0, explosionsSFX.Length)];
        PlaySound(location, explosionSFX, 1, 1, 25, "explosionSFX");
    }
}
