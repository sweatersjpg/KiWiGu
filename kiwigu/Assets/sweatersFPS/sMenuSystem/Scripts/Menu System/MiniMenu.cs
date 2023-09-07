using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New MiniMenu", menuName = "MiniMenu")]
public class MiniMenu : ScriptableObject
{
    public enum SettingsType
    {
        Slider,
        Checkbox,
        Dropdown,
        Info,
        Input
    }

    public string title;
    public string versionNumber;
    public List<Page> pages;

    [System.Serializable]
    public class Page
    {
        public string title;
        public string onClick;
        public List<Settings> settings;
    }

    [System.Serializable]
    public class Settings
    {
        public string title;
        public SettingsType type;

        [TextArea(4, 20)]
        public string[] content;
        public string callBack;
    }
}
