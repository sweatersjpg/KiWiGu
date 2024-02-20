using UnityEngine;
using UnityEngine.Audio;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager instance;
    public AudioMixer globalMixer;

    // Sounds
    public AudioClip headshotSFX;
    public AudioClip emptyMagSFX;
    public AudioClip[] shootingSounds;

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
        for (int i = 0; i < shootingSounds.Length; i++)
        {
            if (info.gunName == shootingSounds[i].name)
            {
                PlaySound(location, shootingSounds[i], 1, 1, 25, "shootSFX");
                return;
            }
        }
    }

    public void PlayGunEmpty(Transform location)
    {
        PlaySound(location, emptyMagSFX, 1, 1, 25, "emptyMagSFX");
    }
}
