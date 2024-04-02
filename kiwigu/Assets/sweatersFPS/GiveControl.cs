using UnityEngine;
using UnityEngine.SceneManagement;

public class GiveControl : MonoBehaviour
{
    [SerializeField] int sceneIndex;
    float timer = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer > 3 && Input.anyKeyDown) SceneManager.LoadScene(sceneIndex);
    }
}
