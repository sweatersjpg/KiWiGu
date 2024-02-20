using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    Button button;
    [SerializeField] int sceneIndex;
    [SerializeField] bool buttonPressed = false;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => ButtonPressed());
        StartCoroutine(LoadScene());
    }

    void ButtonPressed()
    {
        buttonPressed = true;
    }

    IEnumerator LoadScene()
    {
        UnityEngine.AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            if (buttonPressed) asyncOperation.allowSceneActivation = true;

            yield return null;
        }
    }
}
