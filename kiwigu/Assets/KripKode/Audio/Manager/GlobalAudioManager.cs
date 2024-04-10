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
    public AudioClip hookThrowSFX;
    public AudioClip hookSnatchedSFX;
    public AudioClip hookTugSFX;
    public AudioClip[] hookHitSFXs;
    public AudioClip hookBounceSFX;
    public AudioClip hookWhipBackSFX;
    public AudioClip grappleLaunchSFX;
    public AudioClip[] kickSFX;
    public AudioClip interceptorExplosionSFX;
    public AudioClip bulwarkExplosionSFX;
    public AudioClip bulwarkExtendSFX;
    public AudioClip bulwarkRetractSFX;

    public AudioClip[] bulletHitSFX;
    public AudioClip[] bulletHitFleshSFX;
    public AudioClip[] bulletHitArmorSFX;

    // Variables
    [Space(10)]
    public AudioSource battleSourceA;
    public AudioClip battleStemOne;
    public AudioSource battleSourceB;
    public AudioClip battleStemTwo;
    public bool battleTrigger = false;

    public AudioSource voiceLineSource;

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
        audioSource.rolloffMode = AudioRolloffMode.Custom;
        audioSource.dopplerLevel = 0;

        audioSource.Play();

        Destroy(soundObject, clip.length);
    }

    public void PlayHeadshotSFX(Transform location)
    {
        PlaySound(location, headshotSFX, 1, 1, 50, "headshotSFX");
    }

    public void PlayGunFire(Transform location, GunInfo info)
    {
        if (info.shootSound == null) return;
        PlaySound(location, info.shootSound, 0.5f, Random.Range(0.9f, 1.05f), 50, "shootSFX");
    }

    public void PlayGunEmpty(Transform location, GunInfo info)
    {
        if (info.emptyMagSFX == null) return;
        PlaySound(location, info.emptyMagSFX, 1, 0.5f, 25, "emptyGunSFX");
    }

    public void PlayExplosion(Transform location, string explosionType)
    {
        switch (explosionType)
        {
            case "Retract":
                PlaySound(location, bulwarkRetractSFX, 1, 1, 50, "explosionSFX");
                break;
            case "Bulwark":
                PlaySound(location, bulwarkExplosionSFX, 1, 1, 50, "explosionSFX");
                PlaySound(location, bulwarkExtendSFX, 1, 1, 50, "explosionSFX");
                break;
            case "Interceptor":
                PlaySound(location, interceptorExplosionSFX, 1, 1, 50, "explosionSFX");
                break;
            case "Normal":
                AudioClip explosionSFX = explosionsSFX[Random.Range(0, explosionsSFX.Length)];
                PlaySound(location, explosionSFX, 1, 1, 50, "explosionSFX");
                break;
        }
    }

    public void PlayHook(Transform location, string actionReference)
    {
        switch (actionReference)
        {
            case "Throw":
                PlaySound(location, hookThrowSFX, 1, 1, 50, "hookThrowSFX");
                break;
            case "Snatched":
                PlaySound(location, hookSnatchedSFX, 1, 1, 50, "hookSnatchedSFX");
                break;
            case "Tug":
                PlaySound(location, hookTugSFX, 1, 1, 50, "hookTugSFX");
                break;
            case "Bounce":
                PlaySound(location, hookBounceSFX, 1, Random.Range(0.9f, 1.05f), 50, "hookBounceSFX");
                break;
            case "Whip Back":
                PlaySound(location, hookWhipBackSFX, 1, 1, 50, "hookWhipBackSFX");
                break;
            case "Launch":
                PlaySound(location, grappleLaunchSFX, 1, 1, 50, "grappleLaunchSFX");
                break;
            case "Hit":
                AudioClip hookHitSFX = hookHitSFXs[Random.Range(0, hookHitSFXs.Length)];
                PlaySound(location, hookHitSFX, 1, 1, 50, "hookHitSFX");
                break;
        }
    }

    public void PlayKick(Transform location)
    {
        PlaySound(location, kickSFX[Random.Range(0, kickSFX.Length)], 1, 1, 50, "kickSFX");
    }

    public void PlayBulletHit(Transform location, string type)
    {
        switch (type)
        {
            case "Flesh":
                PlaySound(location, bulletHitFleshSFX[Random.Range(0, bulletHitFleshSFX.Length)], 0.4f, 1, 50, "hitFleshSFX");
                break;
            case "Armor":
                PlaySound(location, bulletHitArmorSFX[Random.Range(0, bulletHitArmorSFX.Length)], 1, 1, 50, "hitArmorSFX");
                break;
            case "Headshot":
                PlayHeadshotSFX(location);
                break;
            default:
                PlaySound(location, bulletHitSFX[Random.Range(0, bulletHitSFX.Length)], 1, 1, 30, "bulletHitSFX");
                break;
        }
    }

    public void PlayVoiceLine(AudioClip clip)
    {
        voiceLineSource.Stop();
        voiceLineSource.clip = clip;
        voiceLineSource.Play();
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
}
