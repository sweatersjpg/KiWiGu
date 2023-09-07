using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class catDotExe : ScreenProgram
{
    int t = 0;

    Vector2 catPos;
    Vector2 offset;
    bool catGrabbed = false;

    float lastFrameTime;
    float[] lastFrameRates;

    public override void Setup()
    {
        mouseIcon = HAND_OPEN;

        catPos = new(R.width / 2, R.height / 2);

        lastFrameRates = new float[60];
    }

    public override void Resume()
    {
        //throw new System.NotImplementedException();
    }

    public override void Draw()
    {

        if((mouse - catPos).magnitude < 8)
        {
            mouseIcon = HAND_OPEN;
            if (mouseButtonDown)
            {
                offset = (mouse - catPos);
                catGrabbed = true;
            }
        } else
        {
            mouseIcon = ARROW;
        }

        if (mouseButton)
        {
            mouseIcon = HAND_CLOSED;
            if (catGrabbed) catPos = mouse - offset;
        }
        else
        {
            catGrabbed = false;
        }

        t += 1;

        float s = push(lastFrameRates, 1 / (Time.unscaledTime - lastFrameTime));

        put("FPS: " + (int) lastFrameRates[lastFrameRates.Length-1] + "\n - low: " + (int)s + "\n - avg: " + (int) avg(lastFrameRates), 2, 2);
        lastFrameTime = Time.unscaledTime;

        put("frames: " + t, 2, 5);

        int[] frames = { 0, 16, 32, 48, 32, 16 };

        R.spr(frames[(int)(t/10f) % frames.Length], 0, catPos.x - 8, catPos.y - 8 - (catGrabbed ? 4:0), 16, 16);
    }

    float push(float[] a, float b)
    {
        float smallest = 10000;

        for(int i = 0; i < a.Length-1; i++)
        {
            a[i] = a[i + 1];
            if (a[i] < smallest) smallest = a[i];
        }
        a[a.Length - 1] = b;

        return smallest;
    }

    float avg(float[] a)
    {
        float sum = 0;
        for (int i = 0; i < a.Length - 1; i++)
        {
            sum += a[i];
        }

        return sum / a.Length;
    }
}
