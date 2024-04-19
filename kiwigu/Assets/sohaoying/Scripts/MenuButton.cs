using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    Button button;
    [SerializeField] GameObject credits;

    void Start()
    {
        button = GetComponent<Button>();
    }

    public void ButtonPressed()
    {
        if (PauseSystem.paused) return;
        //credits.SetActive(true);
        SceneManager.LoadScene(4);
    }

    public void Quit()
    {
        if (PauseSystem.paused) return;
        Application.Quit();
    }
}
