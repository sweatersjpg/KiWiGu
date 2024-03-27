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

    public Camera mainCamera;

    [Header("Settings")]

    [Range(40, 80)]
    public static float FOV = 70;
    public float FOVmin = 40;
    public float FOVmax = 80;

    [Range(2, 6)]
    public static float mouseSensitivity = 4;
    public float mouseSensitivityMin = 1;
    public float mouseSensitivityMax = 7;

    public static float musicVol = 0.1f;
    public static float sfxVol = 0.1f;
    public static float masterVol = 0.1f;

    private void Awake()
    {
        if (pauseSystem == null)
        {
            pauseSystem = this;
            //DontDestroyOnLoad(gameObject);
        }
        // pauseSystem = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // mainCamera = sweatersController.instance.playerCamera;
        mainCamera = Camera.main;

        LoadSettings();
        // add mixers
    }

    // Update is called once per frame
    void Update()
    {
        //if (isMainMenu) return;
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.I)) TogglePaused();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GotoMainMenu()
    {
        if (paused) TogglePaused();
        SceneManager.LoadScene(0);
    }

    public void ReloadScene()
    {
        if (paused) TogglePaused();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void TogglePaused()
    {
        Cursor.lockState = !paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = !paused;
        Time.timeScale = !paused ? 0 : 1;

        paused = !paused;
    }

    public void SetActivePause(bool state)
    {
        //settingsPanel.SetActive(false);
        //pausePanel.SetActive(state);
    }

    void SaveSetting(string setting)
    {
        float value = (float) GetType().GetField(setting).GetValue(this);
        PlayerPrefs.SetFloat(setting, value);
    }

    void SaveSettings()
    {
        SaveSetting(nameof(FOV));
        SaveSetting(nameof(mouseSensitivity));
        SaveSetting(nameof(masterVol));
        SaveSetting(nameof(musicVol));
        SaveSetting(nameof(sfxVol));
    }

    void LoadSetting(string setting)
    {
        float value = PlayerPrefs.GetFloat(setting, (float)GetType().GetField(setting).GetValue(this));
        GetType().GetField(setting).SetValue(this, value);
    }

    void LoadSettings()
    {
        LoadSetting(nameof(FOV));
        LoadSetting(nameof(mouseSensitivity));
        LoadSetting(nameof(masterVol));
        LoadSetting(nameof(musicVol));
        LoadSetting(nameof(sfxVol));
    }

    // ---- settings ----

    // toggle

    public void SetFullscreen(bool value)
    {
        Resolution res = Screen.resolutions[Screen.resolutions.Length - 1];
        if(!value) res = Screen.resolutions[Screen.resolutions.Length - 2];

        Screen.SetResolution(res.width, res.height, value);

        // Screen.fullScreen = value;
    }

    public void SetResolution(float value)
    {
        if (value == 1) value = 0.999f;
        
        Resolution[] resolutions = Screen.resolutions;
        Resolution res = resolutions[(int)(value * resolutions.Length)];

        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    // sliders

    //public void UpdateVolume(float value) => pauseSystem.masterMixer.SetFloat("volume", 10*Mathf.Log10(value));
    public void UpdateSfxVolume(float value)
    {
        sfxVol = value;
        GlobalAudioManager.instance.globalMixer.FindMatchingGroups("SFX")[0].audioMixer.SetFloat("volume", Mathf.Lerp(-80, 0, value));

        SaveSetting(nameof(sfxVol));
    }

    public void UpdateMusicVolume(float value)
    {
        musicVol = value;
        // set mixer volume
        GlobalAudioManager.instance.globalMixer.FindMatchingGroups("Music")[0].audioMixer.SetFloat("volume", Mathf.Lerp(-80, 0, value));

        SaveSetting(nameof(musicVol));
    }

    public void UpdateMasterVolume(float value)
    {
        masterVol = value;
        // set mixer volume
        GlobalAudioManager.instance.globalMixer.FindMatchingGroups("Master")[0].audioMixer.SetFloat("volume", Mathf.Lerp(-80, 0, value));

        SaveSetting(nameof(masterVol));
    }

    public void UpdateSensitivity(float value)
    {
        mouseSensitivity = Mathf.Lerp(pauseSystem.mouseSensitivityMin, pauseSystem.mouseSensitivityMax, value);
        if(sweatersController.instance) sweatersController.instance.lookSpeed = mouseSensitivity;

        SaveSetting(nameof(mouseSensitivity));
    }

    public void UpdateFOV(float value)
    {
        FOV = Mathf.Lerp(pauseSystem.FOVmin + 0.1f, pauseSystem.FOVmax, value);
        pauseSystem.mainCamera.fieldOfView = FOV;

        SaveSetting(nameof(FOV));
    }

}
