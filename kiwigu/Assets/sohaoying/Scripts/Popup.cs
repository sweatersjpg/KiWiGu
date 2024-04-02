using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{
    [SerializeField] GameObject popupObject;
    private void OnTriggerEnter(Collider collision)
    {
        popupObject.SetActive(true);
    }

    private void OnTriggerExit(Collider collision)
    {
        popupObject.SetActive(false);
    }
}