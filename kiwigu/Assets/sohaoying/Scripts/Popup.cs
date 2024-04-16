using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{
    [SerializeField] GameObject popupObject;
    private void OnTriggerEnter(Collider collision)
    {
        if(popupObject)
            popupObject.SetActive(true);
    }

    private void OnTriggerExit(Collider collision)
    {
        if (popupObject)
            popupObject.SetActive(false);
    }
}