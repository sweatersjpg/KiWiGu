using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
        credits.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
