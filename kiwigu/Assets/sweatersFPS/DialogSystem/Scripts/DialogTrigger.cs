using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{

    [SerializeField] Dialog dialog;
    
    [SerializeField] bool displayOnContact = false;
    [SerializeField] bool destroyOnContact = false;
    [SerializeField] bool startDialogAsap = false;

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
        if(playerInBounds && displayOnContact)
        {
            TriggerDialog();
            if (destroyOnContact) Destroy(gameObject);
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

    public void TriggerDialog()
    {
        // enemy barks won't activate if a bark of the same or higher priority is already playing
        if ((((dialog.characterIDs[0] == 2) || (dialog.characterIDs[0] == 3)) && (dialog.priority <= DialogManager.instance.currentDialogPriority))
            //but danny and others can interrupt themselves even if it's of the same priority
            || (dialog.priority < DialogManager.instance.currentDialogPriority)) return;
        DialogManager.instance.currentDialogPriority = dialog.priority;

        List<string> lines = new List<string>();
        List<float> lineDurations = new List<float>();

        switch (dialog.type)
        {
            case Dialog.DialogType.Sequence:
                for (int i = 0; i < dialog.displayText.Length; i++) //dialog.dialogIndex; i < dialog.displayText.Length; i++)
                {
                    lines.Add(dialog.displayText[i]);
                    lineDurations.Add(dialog.lineDurations[i]);
                }
                //dialog.dialogIndex = dialog.displayText.Length - 1;
                break;
            case Dialog.DialogType.Random:
                int randomIndex = Random.Range(0, dialog.displayText.Length);
                lines.Add(dialog.displayText[randomIndex]);
                lineDurations.Add(dialog.lineDurations[randomIndex]);
                break;
            case Dialog.DialogType.Repeat:
                dialog.dialogIndex++;
                lines.Add(dialog.displayText[dialog.dialogIndex % dialog.displayText.Length]);
                lineDurations.Add(dialog.lineDurations[dialog.dialogIndex % dialog.lineDurations.Count]);
                print("DUDE" + dialog.dialogIndex % dialog.displayText.Length);
                break;
        }

        if(dialog.audioClips.Length > 0)
            GlobalAudioManager.instance.PlayVoiceLine(dialog.audioClips[dialog.dialogIndex]);

        DialogManager.instance.StartDialog(lines, lineDurations);
    }
}
