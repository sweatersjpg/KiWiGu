using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class directionalBehaviour : MonoBehaviour
{
    [SerializeField] float fadeOutTime = 1;

    float fadeOutTimer;

    private void Update()
    {
        fadeOutTimer += Time.deltaTime;

        if (fadeOutTimer >= fadeOutTime) Destroy(gameObject);

        if (GetComponent<SpriteRenderer>() != null)
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1 - fadeOutTimer / fadeOutTime);
    }
}
