using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ScreenSystem : MonoBehaviour
{
    [HideInInspector]
    public MiniRenderer R;
    Texture RspriteSheet;

    [Header("Programs")]
    public ScreenProgram[] programs;
    public int programIndex;

    ScreenProgram program
    {
        get { return programs[programIndex]; }
    }

    [Header("Transition")]

    public Camera menuCamera;
    public Camera gameCamera;
    PostProcessLayer gamePostProcessing;
    CameraPause gameCameraPause;

    public static Vector2 mouse;
    [HideInInspector] public bool pmouseButton = false;
    [HideInInspector] public bool mouseButton = false;
    [HideInInspector] public bool mouseButtonDown = false;
    [HideInInspector] public bool mouseButtonUp = false;
    [HideInInspector] public float mouseScrollDelta;

    bool willResume = false;

    public PostProcessVolume effects;
    LensDistortion distortionEffect;
    bool distortionEffectActive = false;

    public SpriteRenderer backdrop;
    public bool pauseGameCamera = false;

    [Space]
    public float transitionDuration;
    public AnimationCurve distortionTransition;
    public AnimationCurve backdropTransition;

    public Color backdropStart;
    public Color backdropEnd;

    float transition = 0;

    private void Awake()
    {
        distortionEffectActive = effects.sharedProfile.TryGetSettings(out distortionEffect);

        gameCameraPause = gameCamera.GetComponent<CameraPause>();
        gamePostProcessing = gameCamera.GetComponent<PostProcessLayer>();
    }

    private void Update()
    {
        if (PauseSystem.paused && Input.GetKeyDown(KeyCode.Tab)) Resume(2);

        // open pause menu
        if (!PauseSystem.paused && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))) Resume(0);
        // open inventory
        if (!PauseSystem.paused && Input.GetKeyDown(KeyCode.E)) Resume(1);

        mouseButton = Input.GetMouseButton(0);
        mouseScrollDelta += Input.mouseScrollDelta.y;
    }

    void Init(MiniRenderer mr) // called from MiniRenderer
    {
        R = mr;
        RspriteSheet = R.spriteSheet;

        for(int i = 0; i < programs.Length; i++)
        {
            programs[i].R = R;
            programs[i].Setup();
        }
    }

    void FrameUpdate() // called from MiniRenderer
    {
        mouseButtonDown = mouseButton && !pmouseButton;
        mouseButtonUp = pmouseButton && !mouseButton;

        DoTransition();

        if (!PauseSystem.paused) return;

        GetMouse();
        if (mouse.x > 0 && mouse.x < R.width && mouse.y > 0 && mouse.y < R.height) Cursor.visible = false;
        else Cursor.visible = true;

        foreach(ScreenProgram p in programs)
        {
            p.mouse = mouse;
            p.mouseButton = mouseButton;
            p.mouseButtonDown = mouseButtonDown;
            p.mouseButtonUp = mouseButtonUp;
            p.mouseScrollDelta = mouseScrollDelta * 4;
        }
        mouseScrollDelta = 0;

        if(program.spriteSheet != null) R.spriteSheet = program.spriteSheet;

        if(willResume)
        {
            program.Resume();
            willResume = false;
        }

        program.Draw();

        R.spriteSheet = RspriteSheet;

        program.DrawWindow();

        //R.spr(112, 0, mouse.x, mouse.y, 7, 10);

        R.Display();

        pmouseButton = mouseButton;
    }

    void DoTransition()
    {
        if (PauseSystem.paused)
        {
            if (transition < transitionDuration) transition += 0.016f;
            else
            {
                transition = transitionDuration;
                if (pauseGameCamera) gameCameraPause.paused = true;
            }
            if (transition > transitionDuration / 2 && !menuCamera.gameObject.activeSelf)
            {
                menuCamera.gameObject.SetActive(true);
                gamePostProcessing.enabled = false;
            }
        }
        else
        {
            if (transition > 0) transition -= 1f / R.frameRate;
            else transition = 0;
            if (transition < transitionDuration / 2 && menuCamera.gameObject.activeSelf)
            {
                menuCamera.gameObject.SetActive(false);
                gamePostProcessing.enabled = true;
            }
            if (gameCameraPause.paused) gameCameraPause.paused = false;
        }

        if (distortionEffectActive) distortionEffect.intensity.Override(distortionTransition.Evaluate(transition / transitionDuration) * 100);
        backdrop.color = Color.Lerp(backdropStart, backdropEnd, backdropTransition.Evaluate(transition / transitionDuration));
    }

    void GetMouse()
    {
        Vector3 worldPosition = menuCamera.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * transform.localPosition.z);
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        mouse = new((localPosition.x + 0.5f) * R.width, (-localPosition.y + 0.5f) * R.height);
    }

    void Resume(int index)
    {
        programIndex = index;
        willResume = true;
    }

    public void UpdateFOV(float value)
    {
        gameCameraPause.paused = false;
    }
}
