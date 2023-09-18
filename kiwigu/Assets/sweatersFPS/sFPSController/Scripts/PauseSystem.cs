using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseSystem : MonoBehaviour
{
    public static PauseSystem pauseSystem;
    public static bool paused;

    public AudioMixer masterMixer;
    public Camera mainCamera;

    [Header("Settings")]

    [Range(40, 80)]
    public static float FOV = 60;
    public float FOVmin = 40;
    public float FOVmax = 80;

    [Range(2, 6)]
    public static float mouseSensitivity = 4;
    public float mouseSensitivityMin = 2;
    public float mouseSensitivityMax = 6;

    private void Awake()
    {
        if (pauseSystem == null)
        {
            pauseSystem = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (isMainMenu) return;
        if(Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.E)) TogglePaused();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GotoMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void ReloadScene()
    {
        TogglePaused();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePaused()
    {
        Cursor.lockState = !paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = !paused;
        //Time.timeScale = !paused ? 0 : 1;

        paused = !paused;
    }

    public void SetActivePause(bool state)
    {
        //settingsPanel.SetActive(false);
        //pausePanel.SetActive(state);
    }

    // ---- settings ----

    // toggle

    public void SetFullscreen(bool value) => Screen.fullScreen = value;

    // sliders

    public void UpdateVolume(float value) => pauseSystem.masterMixer.SetFloat("volume", 10*Mathf.Log10(value));

    public void UpdateSensitivity(float value)
    {
        mouseSensitivity = Mathf.Lerp(pauseSystem.mouseSensitivityMin, pauseSystem.mouseSensitivityMax, value);
        sweatersController.instance.lookSpeed = mouseSensitivity;
    }

    public void UpdateFOV(float value)
    {
        FOV = Mathf.Lerp(pauseSystem.FOVmin, pauseSystem.FOVmax, value);
        pauseSystem.mainCamera.fieldOfView = FOV;
    }

}
