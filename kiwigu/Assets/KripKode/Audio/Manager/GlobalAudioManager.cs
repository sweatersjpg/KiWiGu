using UnityEngine;
using UnityEngine.Audio;
using static MiniMenuSystem;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager instance;
    public AudioMixer globalMixer;

    // Sounds
    public AudioClip headshotSFX;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
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
        PlaySound(location, headshotSFX, 1, 1, 25, "headshotSFX");
    }

    public void PlayGunFire(Transform location, GunInfo info)
    {
        if (info.shootSound == null) return;
        PlaySound(location, info.shootSound, 1, Random.Range(0.9f, 1.05f), 25, "shootSFX");
    }

    public void PlayGunEmpty(Transform location, GunInfo info)
    {
        if (info.emptyMagSFX == null) return;
        PlaySound(location, info.emptyMagSFX, 1, 1, 25, "emptyGunSFX");
    }
}
