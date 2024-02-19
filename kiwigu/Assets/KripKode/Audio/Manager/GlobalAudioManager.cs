using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager instance;

    // Sounds
    public AudioClip headshotSFX;
    public AudioClip shootHellfireSFX;

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
        PlaySound(location, headshotSFX, 1, 1, 15, "headshotSFX");
    }
}
