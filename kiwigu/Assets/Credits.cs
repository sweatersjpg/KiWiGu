using UnityEngine;

public class Credits : MonoBehaviour
{
    float timer = 0;

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer > 1 && Input.anyKeyDown)
        {
            gameObject.SetActive(false);
            timer = 0;
        }
    }
}
