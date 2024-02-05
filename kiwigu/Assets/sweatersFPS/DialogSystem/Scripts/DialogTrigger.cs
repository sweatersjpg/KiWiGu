using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{

    [SerializeField] Dialog dialog;
    [SerializeField] bool displayOnContact = false;

    [Space]
    // [SerializeField] GameObject[] enable;
    // [SerializeField] GameObject[] disable;

    // bool playerInBounds;

    // SpriteRenderer sr;

    int dialogIndex = 0;

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

        //if(playerInBounds && displayOnContact)
        //{
        //    TriggerDialog();
        //    displayOnContact = false;
        //}
    }

    public void TriggerDialog()
    {
        List<string> sentences = new List<string>();

        switch(dialog.type)
        {
            case Dialog.DialogType.Random:
                sentences.Add(dialog.sentences[Random.Range(0, dialog.sentences.Length)]);
                break;
            case Dialog.DialogType.Repeat:
                sentences.Add(dialog.sentences[dialogIndex++ % dialog.sentences.Length]);
                break;
            case Dialog.DialogType.Sequence:
                for(int i = dialogIndex; i < dialog.sentences.Length; i++)
                {
                    sentences.Add(dialog.sentences[i]);
                }
                dialogIndex = dialog.sentences.Length - 1;
                break;
        }

        DialogManager.instance.StartDialog(sentences, displayOnContact);
        triggeredDialog = true;
    }

    //void UpdateChanges()
    //{
    //    for (int i = 0; i < enable.Length; i++) enable[i].SetActive(true);
    //    for (int i = 0; i < disable.Length; i++) disable[i].SetActive(false);
    //}

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    playerInBounds = true;
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    playerInBounds = false;
    //}
}
