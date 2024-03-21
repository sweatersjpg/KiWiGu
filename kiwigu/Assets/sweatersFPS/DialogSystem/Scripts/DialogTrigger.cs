using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{

    [SerializeField] Dialog dialog;
    
    [SerializeField] bool noAudioSource = false;
    [SerializeField] bool displayOnContact = false;
    [SerializeField] bool destroyOnContact = false;
    [SerializeField] bool startDialogAsap = false;

    [Space]
    [SerializeField] AudioSource audio;


    // [SerializeField] GameObject[] enable;
    // [SerializeField] GameObject[] disable;

    bool playerInBounds;

    // SpriteRenderer sr;

    bool triggeredDialog = false;
    // bool doneChanges = false;

    private void Start()
    {
        // sr = GetComponent<SpriteRenderer>();

        // sr.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if (!playerInBounds) sr.color = new Color(1,1,1,0.2f);
        //else sr.color = new Color(1, 0, 0, 0.2f);

        //if (DialogManager.instance.active) return;

        //if (triggeredDialog && !doneChanges)
        //{
        //    UpdateChanges();
        //    doneChanges = true;
        //}

        //if (playerInBounds) DialogManager.instance.ShowInteractPrompt();

        //if(playerInBounds && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Z)))
        //{
        //    TriggerDialog();
        //}
        if(playerInBounds && noAudioSource)
        {
            DialogManager.instance.TriggerDialog(dialog);
        }

        else if(playerInBounds && displayOnContact)
        {
            DialogManager.instance.TriggerDialog(dialog, audio);//, startDialogAsap);
            displayOnContact = false;
            //if (destroyOnContact) Destroy(gameObject);

        }
    }



    //void UpdateChanges()
    //{
    //    for (int i = 0; i < enable.Length; i++) enable[i].SetActive(true);
    //    for (int i = 0; i < disable.Length; i++) disable[i].SetActive(false);
    //}

    private void OnTriggerEnter(Collider collision)
    {
        playerInBounds = true;
    }

    private void OnTriggerExit(Collider collision)
    {
        playerInBounds = false;
    }
}
