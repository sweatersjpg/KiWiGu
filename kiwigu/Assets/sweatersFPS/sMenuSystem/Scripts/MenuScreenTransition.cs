using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScreenTransition : MonoBehaviour
{

    public float transitionDuration;

    public GameObject screen;
    public SpriteRenderer backdrop;

    public AnimationCurve backdropTransition;

    public Color backdropStart;
    public Color backdropEnd;

    float transitonTimer = 0;

    //// Start is called before the first frame update
    //void Start()
    //{
        
    //}

    // Update is called once per frame
    void Update()
    {
        if (PauseSystem.paused) transitonTimer += 1 / transitionDuration * Time.deltaTime;
        else transitonTimer -= 1 / transitionDuration * Time.deltaTime;

        if (transitonTimer < 0) transitonTimer = 0;
        if (transitonTimer > transitionDuration) transitonTimer = transitionDuration;

        if (transitonTimer < transitionDuration * 0.5f && screen.activeSelf) screen.SetActive(false);
        if (transitonTimer > transitionDuration * 0.5f && !screen.activeSelf) screen.SetActive(true);

        // also hide UI here if thats needed

        backdrop.color = Color.Lerp(backdropStart, backdropEnd, backdropTransition.Evaluate(transitonTimer/transitionDuration));
    }
}
