using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance;

    [HideInInspector] public bool active;

    [SerializeField] float textSpeed = 20;

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

    public void StartDialog(List<string> dialogLines, List<float> durations, bool startDialogAsap)
    {
        if (startDialogAsap) {
            sentences.Clear();
            sentenceDurations.Clear();
        }
        foreach(string s in dialogLines) sentences.Enqueue(s);
        foreach (float d in durations) sentenceDurations.Enqueue(d);

        // enable dialog box
        dialogBox.SetActive(true);

        if (startDialogAsap)
        {
            NextDialog();
        }
    }

    // CHANGE LATER TO START THIS WITH DIALOG AND AUDIOSOURCE TO PLAY FROM, IF ANY)
    public void TriggerDialog(Dialog dialog, bool startDialogAsap)
    {
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
                print(dialog.displayText.Length);
                lines.Add(dialog.displayText[randomIndex]);
                lineDurations.Add(dialog.lineDurations[randomIndex]);
                break;
            case Dialog.DialogType.Repeat:
                lines.Add(dialog.displayText[dialogIndex++ % dialog.displayText.Length]);
                lineDurations.Add(dialog.lineDurations[dialogIndex++ % dialog.lineDurations.Count]);
                break;
        }

        StartDialog(lines, lineDurations, startDialogAsap);
    }

    /*public void ShowInteractPrompt()
    {
        showInteractPrompt = true;
    }*/
}
