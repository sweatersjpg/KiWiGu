using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class FPSDisplay : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI text;
    [SerializeField] GameObject lowFPSSprite;
    
    // Start is called before the first frame update
    void Start()
    {
        text.gameObject.SetActive(false);
        lowFPSSprite.SetActive(false);

        InvokeRepeating(nameof(ChangeFrameRate), 1, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if ((1 / Time.deltaTime) < 30 && !lowFPSSprite.activeSelf) lowFPSSprite.SetActive(true);
        if ((1 / Time.deltaTime) > 30 && lowFPSSprite.activeSelf) lowFPSSprite.SetActive(false);

        if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            text.gameObject.SetActive(!text.gameObject.activeSelf);
        }
    }

    void ChangeFrameRate()
    {
        text.text = "FPS: " + Mathf.Floor((1 / Time.deltaTime) * 100) / 100;
    }
}
