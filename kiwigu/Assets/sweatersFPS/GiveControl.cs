using UnityEngine;
using UnityEngine.SceneManagement;

public class GiveControl : MonoBehaviour
{
    [SerializeField] int sceneIndex;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }
}
