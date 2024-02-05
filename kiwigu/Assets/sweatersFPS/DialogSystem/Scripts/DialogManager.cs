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
    [SerializeField] TextMeshProUGUI dialog;
    [SerializeField] GameObject moreIcon;
    // [SerializeField] GameObject interactPrompt;

    bool showInteractPrompt = false;

    Queue<string> sentences;

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
        dialogBox.SetActive(false);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.anyKeyDown)
        {
            if(textComplete) NextDialog();
            else
            {
                StopAllCoroutines();
                textComplete = true;
                dialog.text = currentText;
            }
        }

        moreIcon.SetActive(textComplete);

        active = dialogBox.activeSelf;

        // interactPrompt.SetActive(showInteractPrompt);
        showInteractPrompt = false;
    }

    IEnumerator DisplayText(string text)
    {
        currentText = text;
        dialog.text = "";
        textComplete = false;

        for(int i = 0; i < text.Length; i++)
        {
            dialog.text += text[i];
            yield return new WaitForSecondsRealtime(1/textSpeed);
        }

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

        // play dialog

        StartCoroutine(nameof(DisplayText), s);

        // dialog.text = s;
    }

    public void StartDialog(List<string> lines, bool startDialogAsap)
    {
        sentences.Clear();
        foreach(string s in lines) sentences.Enqueue(s);

        // enable dialog box
        dialogBox.SetActive(true);

        if (startDialogAsap) NextDialog(); 
    }

    public void ShowInteractPrompt()
    {
        showInteractPrompt = true;
    }
}
