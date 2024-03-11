using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Dialog : ScriptableObject
{
    public enum DialogType
    {
        Sequence,
        Random,
        Repeat,
    }

    public bool singleVoiceLine = false;

    public DialogType type;

    public float defaultLineDuration = 6f;

    public int[] characterIDs;
    public static string[] characters = { "DANNY", "THE CHOSEN", "GRUNT", "MECHENEMY", "APOSTLE"};

    [TextArea(2,3)]
    public string[] lines;

    public List<float> lineDurations;

    public AudioClip[] audioClips;

    [TextArea(2, 3)]
    public string[] displayText;

    public void OnValidate()
    {
        displayText = new string[lines.Length];

        if ((singleVoiceLine))
        {
            lineDurations = new List<float>();
        }

        for (int j = 0; j < lines.Length; j++)
        {
            if ((j < characterIDs.Length) && (characterIDs[j] != 1))    // if character ID not THE CHOSEN, add character name to beginning of text
            {
                displayText[j] = characters[characterIDs[j]] + ": " + lines[j];

                if (singleVoiceLine)
                {
                    if (j < audioClips.Length && audioClips[j] != null)
                    {
                        lineDurations.Add(audioClips[j].length);
                    }
                    else    // if no voice for this line, set to default duration
                    {
                        lineDurations.Add(defaultLineDuration);
                    }
                }
            }
            else    // if THE CHOSEN, format differently
            {
                // displayText[j] = "";
                displayText[j] = "(" + lines[j] + ")";

                if (singleVoiceLine)
                {
                    lineDurations.Add(defaultLineDuration);
                }
            }
        }
    }
}
