using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager instance;
    AudioSource backgroundAudio;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        backgroundAudio = GetComponent<AudioSource>();
    }

    public void PlaySound(Transform location, AudioClip clip, float volume, float pitch, float range)
    {
        GameObject soundObject = new GameObject("sound");
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

    public void TransitionAudio()
    {

    }
}
