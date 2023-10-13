using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class MiniMenuSystem : ScreenProgram
{
    public MiniMenu menu;
    public GameObject listener;

    List<Button> buttons;
    Window window;

    public override void Setup() // called from MiniRenderer
    {
        buttons = new();

        foreach (MiniMenu.Page page in menu.pages)
        {
            buttons.Add(new(this, page));
        }
    }

    public override void Resume()
    {
        // throw new System.NotImplementedException();
    }

    public override void Draw() // called from MiniRenderer
    {
        mouseIcon = ARROW;

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].Draw(2 + i * 3);
        }

        drawBox(14, 1, 24, 29);

        if (window != null) window.Draw();
    }

    public class Window
    {
        MiniMenuSystem M;
        Vector2 pos;
        Vector2 size;
        public string title;
        public List<MiniMenu.Settings> settings;

        public List<Adjustable> adjustables;

        public Window(MiniMenuSystem M, List<MiniMenu.Settings> settings, string title, int x, int y, int w, int h)
        {
            this.settings = settings;
            this.title = title;
            this.M = M;
            pos = new Vector2(x, y);
            size = new Vector2(w, h);

            adjustables = new();
            foreach (MiniMenu.Settings s in settings) adjustables.Add(SpawnAdjustable(s));
        }

        public void Draw()
        {
            for (int i = 0; i < adjustables.Count; i++) adjustables[i].Draw((int)(pos.x + size.x / 2) + 1, 3 + i * 2);

            M.drawBox(pos.x, pos.y, size.x, size.y, true);
            M.put(title, pos.x + 2, pos.y);
            M.put("x", pos.x + size.x - 2, pos.y);

            if (M.MouseOver(pos.x + size.x - 2, pos.y)) M.mouseIcon = HAND_POINTER;

            if (M.MouseOver(pos.x + size.x - 2, pos.y) && M.mouseButtonDown) M.window = null;
        }

        public Adjustable SpawnAdjustable(MiniMenu.Settings setting)
        {
            switch (setting.type)
            {
                case MiniMenu.SettingsType.Slider:
                    return new Slider(M, setting);
                case MiniMenu.SettingsType.Checkbox:
                    return new Checkbox(M, setting);
                case MiniMenu.SettingsType.Dropdown:
                    return new Dropdown(M, setting);
                case MiniMenu.SettingsType.Info:
                    return new Info(M, setting);
                case MiniMenu.SettingsType.Input:
                    break;
                default:
                    break;
            }
            return null;
        }
    }

    public class Button
    {
        MiniMenuSystem M;
        MiniMenu.Page page;

        float titleX = 0;

        public Button(MiniMenuSystem M, MiniMenu.Page page)
        {
            this.M = M;
            this.page = page;
        }

        public void Draw(int y)
        {
            if (MouseOver(y)) titleX += (1 - titleX) / 4;
            else titleX -= titleX / 4;

            M.drawBox(2, y, (M.window != null && M.window.title == page.title) ? 12 : 11, 2);
            M.put(page.title, 3 + titleX, y + 1);

            if (MouseOver(y)) M.mouseIcon = HAND_POINTER;
            if (MouseOver(y) && M.mouseButton) M.mouseIcon = HAND_CLOSED;

            if (M.mouseButtonDown && MouseOver(y))
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (M.window == null || M.window.title != page.title)
            {
                if (page.settings.Count > 0) M.window = new(M, page.settings, page.title, 14, 1, 24, 29);
                if (page.onClick.Length > 0) M.listener.SendMessage(page.onClick);
            }
            else M.window = null;
        }

        public bool MouseOver(int y)
        {
            return M.mouse.x > 8 && M.mouse.x < 14 * 8 && M.mouse.y > y * 8 && M.mouse.y < (y + 3) * 8;
        }
    }

    public abstract class Adjustable
    {
        public string title;
        public string callback;
        public MiniMenu.Settings setting;
        public MiniMenuSystem M;

        public Adjustable(MiniMenuSystem M, MiniMenu.Settings setting)
        {
            this.M = M;
            this.setting = setting;
        }

        public abstract void Draw(int x, int y);
    }

    public class Slider : Adjustable
    {
        float value = 0; // 0 - 1
        bool holding = false;

        public Slider(MiniMenuSystem M, MiniMenu.Settings setting) : base(M, setting)
        {
            if (setting.title == "mouse")
            {
                value = Mathf.InverseLerp(
                    PauseSystem.pauseSystem.mouseSensitivityMin,
                    PauseSystem.pauseSystem.mouseSensitivityMax, PauseSystem.mouseSensitivity);
            }
            else if (setting.title == "FOV")
            {
                value = Mathf.InverseLerp(
                    PauseSystem.pauseSystem.FOVmin,
                    PauseSystem.pauseSystem.FOVmax, PauseSystem.FOV);
            }
            //else if (setting.title == "volume")
            //{
            //    PauseSystem.pauseSystem.masterMixer.GetFloat("volume", out value);
            //    value = Mathf.Pow(10, value / 10);
            //}
        }

        public override void Draw(int x, int y)
        {
            M.put(setting.title, x - setting.title.Length - 1, y);
            //M.put("----------", x, y);
            for (int i = 0; i < 10; i++) M.tile(24 + (value * 10 <= i ? 8 : 0), 0, x + i, y);

            if (MouseOver(x, y)) M.mouseIcon = HAND_OPEN;

            if (M.mouseButtonDown && MouseOver(x, y)) holding = true;

            if (M.mouseButton)
            {
                if (holding) M.mouseIcon = HAND_CLOSED;
            }
            else holding = false;

            if (holding)
            {
                value = Mathf.InverseLerp(x * 8, (x + 10) * 8, M.mouse.x);
                if (value < 0) value = 0;
                if (value > 1) value = 1;

                M.listener.SendMessage(setting.callBack, value);
                //if (setting.callBack == "UpdateFOV") M.gameObject.SendMessage(setting.callBack, value);
            }
        }

        bool MouseOver(int x, int y)
        {
            return M.mouse.x / 8 > x && M.mouse.x / 8 < x + 10 && M.mouse.y / 8 > y - 0.5f && M.mouse.y / 8 < y + 1.5f;
        }
    }

    public class Checkbox : Adjustable
    {
        bool value = false; // true / false

        public Checkbox(MiniMenuSystem M, MiniMenu.Settings setting) : base(M, setting)
        {
            if (setting.title == "fullscreen") value = Screen.fullScreen;
        }

        public override void Draw(int x, int y)
        {
            M.put(setting.title, x - setting.title.Length - 1, y);

            M.tile(24 + (value ? 8 : 0), 8, x, y);

            if (M.MouseOver(x, y)) M.mouseIcon = HAND_POINTER;
            if (M.MouseOver(x, y) && M.mouseButton) M.mouseIcon = HAND_CLOSED;

            if (M.mouseButtonDown && M.MouseOver(x, y))
            {
                value = !value;
                M.listener.SendMessage(setting.callBack, value);
            }
        }
    }

    public class Dropdown : Adjustable
    {
        //string[] options;
        //float value = 0; // 0 - options length-1

        public Dropdown(MiniMenuSystem M, MiniMenu.Settings setting) : base(M, setting)
        {

        }

        public override void Draw(int x, int y)
        {
            M.put(setting.title, x - setting.title.Length - 1, y);
        }
    }

    public class Info : Adjustable
    {
        string[] options;

        public Info(MiniMenuSystem M, MiniMenu.Settings setting) : base(M, setting)
        {
            options = new string[setting.content.Length];

            for (int i = 0; i < options.Length; i++)
            {
                options[i] = FixParagraph(string.Copy(setting.content[i]), 21);
            }
        }

        public override void Draw(int x, int y)
        {
            M.put(setting.title, x - setting.title.Length - 1, y);

            y++;

            for (int i = 0; i < options.Length; i++)
            {
                M.put(options[i], x - 11, y);
                y += Count(options[i], '\n') + 2;
            }
        }

        int Count(string s, char c)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == c) count++;
            }

            return count;
        }

        string FixParagraph(string s, int w)
        {
            int count = 0;
            int lastSpace = 0;

            for (int i = 0; i < s.Length; i++)
            {
                count++;

                if (s[i] == '\n') count = 0;
                if (s[i] == ' ') lastSpace = i;

                if (count >= w)
                {
                    s = s[..lastSpace] + '\n' + s[(lastSpace + 1)..];
                    i = lastSpace;
                    count = 0;
                }
            }

            return s;
        }

    }
}
