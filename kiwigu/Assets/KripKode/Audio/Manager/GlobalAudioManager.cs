using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager instance;
    AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(Transform location, AudioClip clip, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(clip, location.position, volume);
    }

    public void TransitionAudio()
    {

    }
}
