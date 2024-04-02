using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerFootsteps : MonoBehaviour
{
    public AudioClip[] footsteps;
    public AudioClip[] landSounds;

    AudioSource audioSource;
    public float timeScale;
    public float pitchRange = 0.1f;

    float time;
    float airBorneTimer;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = GlobalAudioManager.instance.globalMixer.FindMatchingGroups("SFX")[0];
        audioSource.spatialBlend = 0;
        // audioSource.clip = clip;
        audioSource.volume = 1;
        audioSource.maxDistance = 10;
        audioSource.rolloffMode = AudioRolloffMode.Custom;
        audioSource.dopplerLevel = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseSystem.paused) return;
        
        sweatersController player = sweatersController.instance;
        Vector3 velocity = player.velocity;
        time += Time.deltaTime * new Vector2(velocity.x, velocity.z).magnitude;

        if (airBorneTimer >= 0.2f && player.isGrounded) PlayLandSound();

        if (!player.isGrounded)
        {
            airBorneTimer += Time.deltaTime;
        }
        else airBorneTimer = 0;
        if (Input.GetButtonDown("Jump"))
        {
            airBorneTimer = 0.2f;
            PlayJumpSound();
        }

        if (velocity.magnitude < 0.5f || !player.isGrounded || player.isSliding || player.isGrappling) return;

        float x = Mathf.Sin(time * timeScale);
        if (x < 0.01f && !audioSource.isPlaying) PlayFootstep();
    }

    void PlayFootstep()
    {
        audioSource.clip = footsteps[Random.Range(0, footsteps.Length)];

        // audioSource.pitch = Random.Range(-pitchRange, pitchRange);
        audioSource.Play();
    }

    void PlayLandSound()
    {
        audioSource.clip = landSounds[Random.Range(1, landSounds.Length)];
        audioSource.volume = 1f;
        // audioSource.pitch = Random.Range(-pitchRange, pitchRange);
        audioSource.Play();
    }

    void PlayJumpSound()
    {
        audioSource.clip = landSounds[0];
        audioSource.pitch = 0.8f + Random.Range(-pitchRange, pitchRange);
        audioSource.volume = 0.2f;
        audioSource.Play();
    }
}
