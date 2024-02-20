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
            yield return new WaitForSecondsRealtime(1/textSpeed);
        }
        yield return new WaitForSecondsRealtime(textDuration);
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

        // play dialog

        StartCoroutine(DisplayText(s, sd));

        // dialogTextMesh.text = s;
    }

    public void StartDialog(List<string> lines, List<float> durations, bool startDialogAsap)
    {
        sentences.Clear();
        sentenceDurations.Clear();
        foreach(string s in lines) sentences.Enqueue(s);
        foreach (float d in durations) sentenceDurations.Enqueue(d);

        // enable dialog box
        dialogBox.SetActive(true);

        //if (startDialogAsap) NextDialog(); // SOHA REENABLE LATER
    }

    public void TriggerDialog(Dialog dialog, bool displayOnContact)
    {
        List<string> lines = new List<string>();
        List<float> lineDurations = new List<float>();
        int dialogIndex = 0;

        switch (dialog.type)
        {
            case Dialog.DialogType.Sequence:
                for (int i = dialogIndex; i < dialog.sentences.Length; i++)
                {
                    lines.Add(dialog.sentences[i]);
                    lineDurations.Add(dialog.sentenceDurations[i]);
                    print(i);
                }
                dialogIndex = dialog.sentences.Length - 1;
                break;
            case Dialog.DialogType.Random:
                int randomIndex = Random.Range(0, dialog.sentences.Length);
                lines.Add(dialog.sentences[randomIndex]);
                lineDurations.Add(dialog.sentenceDurations[randomIndex]);
                break;
            case Dialog.DialogType.Repeat:
                lines.Add(dialog.sentences[dialogIndex++ % dialog.sentences.Length]);
                lineDurations.Add(dialog.sentenceDurations[dialogIndex++ % dialog.sentenceDurations.Length]);
                break;
        }

        StartDialog(lines, lineDurations, displayOnContact);
    }

    public void ShowInteractPrompt()
    {
        showInteractPrompt = true;
    }
}
