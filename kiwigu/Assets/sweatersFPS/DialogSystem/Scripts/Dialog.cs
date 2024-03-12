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

    public int[] characterIDs;
    public static string[] characters = { "DANNY", "THE CHOSEN", "GRUNT", "MECHENEMY", "APOSTLE"};

    [TextArea(2,3)]
    public string[] lines;

    public List<float> lineDurations;
    static float noVoiceMinimumPauseDuration = 1.5f;
    static float noVoicePauseDurationPerLetter = 0.05f;

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

        for (int i = 0; i < lines.Length; i++)
        {
            if ((i < characterIDs.Length) && (characterIDs[i] != 1))    // if character ID not THE CHOSEN, add character name to beginning of text
            {
                displayText[i] = characters[characterIDs[i]] + ": " + lines[i];

                if (singleVoiceLine)
                {
                    if (i < audioClips.Length && audioClips[i] != null)
                    {
                        lineDurations.Add(audioClips[i].length);
                    }
                    else    // if no voice for this line, set to default duration
                    {
                        lineDurations.Add(noVoiceMinimumPauseDuration + lines[i].Length * noVoicePauseDurationPerLetter);
                    }
                }
            }
            else    // if THE CHOSEN, format differently
            {
                // displayText[i] = "";
                displayText[i] = "(" + lines[i] + ")";

                if (singleVoiceLine)
                {
                    lineDurations.Add(noVoiceMinimumPauseDuration + lines[i].Length * noVoicePauseDurationPerLetter);
                }
            }
        }
    }
}
