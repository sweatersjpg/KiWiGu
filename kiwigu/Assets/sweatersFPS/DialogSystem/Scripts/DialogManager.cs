using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// SEE: PLACEHOLDER, FOR LATER

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance;

    [HideInInspector] public bool active;

    [SerializeField] float textSpeed = 20;
    [SerializeField] int currentDialogPriority = 0;
    
    [Space]
    [SerializeField] List<AudioSource> currentAudioSources;
    [SerializeField] AudioSource playerAudioSource;
    [SerializeField] AudioSource apostleAudioSource;

    [Space]
    [SerializeField] GameObject dialogBox;
    [SerializeField] TextMeshProUGUI dialogTextMesh;
    [SerializeField] GameObject moreIcon;
    // [SerializeField] GameObject interactPrompt;

    bool showInteractPrompt = false;

    Queue<string> sentences;
    Queue<float> sentenceDurations;

    bool textComplete = true;
    string currentText;

    IEnumerator textDisplayCo;

    private void Awake()
    {
        if (instance != null) Destroy(instance.gameObject);

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
        sentenceDurations = new Queue<float>();
        dialogBox.SetActive(false);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //if (Input.anyKeyDown)
        //{
            if(textComplete) NextDialog();
            /*else
            {
                StopAllCoroutines();
                textComplete = true;
                dialogTextMesh.text = currentText;
            }
        }*/

        //moreIcon.SetActive(textComplete);

        active = dialogBox.activeSelf;

        // interactPrompt.SetActive(showInteractPrompt);
        showInteractPrompt = false;
    }

    // FOR LATER: disable on death
    void OnDisable()
    {
        currentAudioSources.Clear();
    }

    IEnumerator DisplayText(string text, float textDuration)
    {
        currentText = text;
        dialogTextMesh.text = "";
        textComplete = false;

        for(int i = 0; i < text.Length; i++)
        {
            dialogTextMesh.text += text[i];
            yield return new WaitForSeconds(1/textSpeed);
        }
        yield return new WaitForSeconds(textDuration);
        textComplete = true;
    }
    
    public void NextDialog()
    {
        if(sentences.Count == 0)
        {
            // hide dialog box
            dialogBox.SetActive(false);
            return;
        }

        string s = sentences.Dequeue();
        float sd = sentenceDurations.Dequeue();

        // remove dialog queue if interrupting
        if (textDisplayCo != null)
        {
            StopCoroutine(textDisplayCo);
        }

        // play dialog
        textDisplayCo = DisplayText(s, sd);
        StartCoroutine(textDisplayCo);

        // dialogTextMesh.text = s;
    }

    public void StartDialog(List<string> dialogLines, List<float> durations)
    {
        //if (startDialogAsap) {
            sentences.Clear();
            sentenceDurations.Clear();
        //}
        foreach(string s in dialogLines) sentences.Enqueue(s);
        foreach (float d in durations) sentenceDurations.Enqueue(d);

        // enable dialog box
        dialogBox.SetActive(true);

        //if (startDialogAsap) {
            NextDialog();
        //}
    }

    public void TriggerDialog(Dialog dialog)
    {
        if (dialog.priority <= currentDialogPriority) return;
        currentDialogPriority = dialog.priority;

        List<string> lines = new List<string>();
        List<float> lineDurations = new List<float>();
        int dialogIndex = 0;

        switch (dialog.type)
        {
            case Dialog.DialogType.Sequence:
                for (int i = dialogIndex; i < dialog.displayText.Length; i++)
                {
                    lines.Add(dialog.displayText[i]);
                    lineDurations.Add(dialog.lineDurations[i]);
                }
                dialogIndex = dialog.displayText.Length - 1;
                break;
            case Dialog.DialogType.Random:
                int randomIndex = Random.Range(0, dialog.displayText.Length);
                lines.Add(dialog.displayText[randomIndex]);
                lineDurations.Add(dialog.lineDurations[randomIndex]);
                break;
            case Dialog.DialogType.Repeat:
                lines.Add(dialog.displayText[dialogIndex++ % dialog.displayText.Length]);
                lineDurations.Add(dialog.lineDurations[dialogIndex++ % dialog.lineDurations.Count]);
                break;
        }

        /* PLACEHOLDER. depends how audio will be done
         * 
         * YOU STILL HAVE TO DRAG IN THE PLAYER AND APOSTLE AUDIOSOURCES; THERE AREN'T ANY RN.
         * or follow a different system. ask later
        */
        switch (dialog.characterIDs[0])
        {
            case 0: // Danny radio speaking
                currentAudioSources.Add(playerAudioSource);
                break;
            case 4: // Apostle speaking
                currentAudioSources.Add(apostleAudioSource);
                break;
        }
        
        if (dialog.audioClips != null && dialog.audioClips.Length > 0)
        {
            currentAudioSources[currentAudioSources.Count - 1].clip = dialog.audioClips[0];
            currentAudioSources[currentAudioSources.Count -1].Play();
        }

        StartDialog(lines, lineDurations);
    }

    /* FOR LATER
       THERE SHOULD BE ONLY ONE AUDIOCLIP PER DIALOG OBJECT... THEORETICALLY. but i left it as an array just in case...
       should avoid putting danny beats near other dialog triggers

    ALSO FOR LATER: consider whether to stop audio or just leave it overlapping.
    it's currently overlapping and you'll probably have to stop it later. do this when the voice lines are in
     
     * Priority Brainstorming?? set this later
     * apostle: 21
     * danny: 11
     * hook blocked: 9
     * 
     * mech hooked: 5
     * grunt hooked: 5
     * alternatively, consider not making the "hooked" events dialog at all, just having the grunting sounds that play wiithout subs
     * 
     * mech stolen: 4
     * grunt stolen: 4
     * 
     * mech mandown: 4
     * grunt mandown: 4
     * 
     * mech spotted: 1
     * grunt spotted: 1
     * 
     * mech idle: 0
     * grunt idle: 0
     * 
     * 
     

    only enemies should need to specify an AudioSource, Danny's radio is from the player and there's only one Apostle
    */
    public void TriggerDialog(Dialog dialog, AudioSource audioSource)
    {
        // PLACEHOLDER. depends how audio will be done
        currentAudioSources.Add(audioSource);

        TriggerDialog(dialog);
    }

    /*public void ShowInteractPrompt()
    {
        showInteractPrompt = true;
    }*/
}
