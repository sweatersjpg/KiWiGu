using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScreenProgram : MonoBehaviour
{
    [HideInInspector]
    public MiniRenderer R;

    [HideInInspector] public Vector2 mouse;
    [HideInInspector] public bool mouseButton;
    [HideInInspector] public bool mouseButtonDown;
    [HideInInspector] public bool mouseButtonUp;
    [HideInInspector] public float mouseScrollDelta;

    public const int ARROW = 112;
    public const int HAND_OPEN = 144;
    public const int HAND_CLOSED = 160;
    public const int HAND_POINTER = 128;
    public const int IBEAM = 176;
    [HideInInspector] public int mouseIcon = ARROW;

    public string title = "ScreenProgram";

    public Texture spriteSheet;

    [HideInInspector]
    public int WIDTH = 39;
    [HideInInspector]
    public int HEIGHT = 31;

    public bool MouseOver(float x, float y)
    {
        return (new Vector2((x + 0.5f) * 8, (y + 0.5f) * 8) - mouse).magnitude < 6;
    }

    public void drawBox(float x, float y, float w, float h) => drawBox(x, y, w, h, false);
    public void drawBox(float x, float y, float w, float h, bool hasTop)
    {
        int sx = hasTop ? 24 : 0;
        int sy = hasTop ? 16 : 0;

        tile(0+sx, 0+sy, x, y); // top left corner
        tile(16+sx, 0+sy, x + w, y); // top right corner
        for (int i = (int) x + 1; i < x + w; i++)
        {
            tile(8+sx, 0+sy, i, y);
            tile(8, 16, i, y + h);
        }
        for (int i = (int) y + 1; i < y + h; i++)
        {
            tile(0, 8, x, i);
            tile(16, 8, x + w, i);
        }
        tile(0, 16, x, y + h); // bottom left corner
        tile(16, 16, x + w, y + h); // bottom right corner
    }
    public void drawBox(float x, float y, float w, float h, bool hasTop, string title)
    {
        drawBox(x, y, w, h, hasTop);
        put(title, x + 1, y);
    }

    public void tile(float sx, float sy, float x, float y) => tile((int)sx, (int)sy, (int)x, (int)y);
    public void tile(int sx, int sy, int x, int y) => R.spr(sx, sy, x * 8, y * 8, 8, 8);

    public void put(string s, float x, float y) => R.put(s, x * 8, y * 8);
    public void put(string s, int x, int y) => R.put(s, x * 8, y * 8);

    public void DrawWindow()
    {
        drawBox(0, 0, WIDTH, HEIGHT, true);
        put(title, 2, 0);

        put("x", WIDTH-1, 0);

        if (MouseOver(WIDTH - 1, 0)) mouseIcon = HAND_POINTER;
        if (MouseOver(WIDTH - 1, 0) && mouseButtonDown) PauseSystem.pauseSystem.SendMessage("TogglePaused");

        R.spr(mouseIcon, 0, mouse.x - 8, mouse.y - 8, 16, 16);
    }

    public abstract void Setup();

    public abstract void Resume();

    public abstract void Draw();
}
