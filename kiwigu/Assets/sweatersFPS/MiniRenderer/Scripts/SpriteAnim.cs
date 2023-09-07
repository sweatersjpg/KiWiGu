using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnim : MonoBehaviour
{
    [HideInInspector]
    public MiniRenderer R;

    public enum AnimType
    {
        Random,
        PingPong,
        Forward
    }

    public AnimType type;

    public float nextFrameChance;

    public string[] frames;

    public int variants = 1;
    public int variantSpacing = 16;

    int spry = 0;

    int[][] fs;

    int t = 0;
    int f = 0;

    void Init(MiniRenderer mr) // called from MiniRenderer
    {
        R = mr;

        fs = new int[frames.Length][];

        for(int i = 0; i < frames.Length; i++)
        {
            frames[i].Replace(" ", "");
            string[] p = frames[i].Split(",");

            fs[i] = new int[2];
            fs[i][0] = int.Parse(p[0]);
            fs[i][1] = int.Parse(p[1]);

        }

        spry = Random.Range(0, variants) * variantSpacing;

        nextFrame();
    }

    void FrameUpdate() // called from MiniRenderer
    {

        //if (!fresh && (Camera.main.transform.position - transform.position).magnitude > 10) return;
        //fresh = false;

        if (Random.Range(0, 1f) < nextFrameChance) nextFrame();

        R.spr(fs[f][0], fs[f][1] + spry, 0, 0, R.width, R.height);

        R.Display();
    }

    void nextFrame()
    {
        switch (type)
        {
            case AnimType.Random:
                t = Random.Range(0, fs.Length);
                f = t;
                break;
            case AnimType.PingPong:
                t = (t + 1) % (fs.Length * 2);
                f = t;
                if (f >= fs.Length) f = fs.Length - (f - fs.Length) - 1;
                break;
            case AnimType.Forward:
                t = (t + 1) % fs.Length;
                f = t;
                break;
        }
    }
}