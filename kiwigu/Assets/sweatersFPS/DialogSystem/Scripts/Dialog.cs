using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Dialog
{
    public enum DialogType
    {
        Repeat,
        Sequence,
        Random
    }

    public DialogType type;

    [TextArea(2,3)]
    public string[] sentences;

}
