using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class directionalBehaviour : MonoBehaviour
{
    [SerializeField] float fadeOutTime = 1;

    [SerializeField] Image spriteImage;

    Color initialColor;
    float fadeOutTimer;
    RectTransform rectTransform;
    Vector3 initialScale;

    private void Start()
    {
        initialColor = spriteImage.color;
        rectTransform = GetComponent<RectTransform>();
        initialScale = rectTransform.localScale;
    }

    private void Update()
    {   
        fadeOutTimer += Time.deltaTime;

        if (fadeOutTimer >= fadeOutTime) Destroy(gameObject);

        if (spriteImage != null)
            spriteImage.color = Color.Lerp(initialColor, Color.clear, fadeOutTimer / fadeOutTime);

        float scaleProgress = fadeOutTimer / fadeOutTime;
        rectTransform.localScale = Vector3.Lerp(initialScale, initialScale * 1.025f, scaleProgress);
    }
}
