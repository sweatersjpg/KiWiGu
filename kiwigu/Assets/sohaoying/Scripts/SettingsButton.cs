using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    Button button;
    [SerializeField] bool buttonPressed = false;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => ButtonPressed());
    }

    void ButtonPressed()
    {
        if (PauseSystem.paused) return;
        PauseSystem.pauseSystem.TogglePaused();
    }

    private void Update()
    {
        if(PauseSystem.paused)
        {
            // Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
