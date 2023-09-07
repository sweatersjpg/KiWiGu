using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class sViewModel : MonoBehaviour
{
    public Camera gameCamera;
    [HideInInspector] public MiniRenderer R;

    [SerializeField] Animation[] animations;

    float bob = 0;
    public float bobIntensity = 1;

    sweatersController sc;

    Vector2 viewPosition;
    Vector2 targetPosition;

    [System.Serializable]
    struct Frame
    {
        public int frame;
        public Vector2 position;
    }

    [System.Serializable]
    struct Animation
    {
        public string title;
        public List<Frame> frames;
    }

    // Start is called before the first frame update
    void Start()
    {
        sc = sweatersController.instance;
    }

    // Update is called once per frame
    void Update()
    {
        AdjustViewPlane();

        //Vector2 v = new(sc.velocity.x, sc.velocity.z);

        //if (sc.isGrounded) bob += Vector3.ClampMagnitude(v * Time.deltaTime, 0.017f).magnitude;
        //else bob = 0;

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentFrame.frame == 16) AddAnimation(animations[2]);
            AddAnimation(animations[4]);
            AddAnimation(animations[0]);
            AddAnimation(animations[5]);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (currentFrame.frame == 1) AddAnimation(animations[1]);
            AddAnimation(animations[6]);
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (currentFrame.frame == 16) AddAnimation(animations[2]);
            if (currentFrame.frame == 1) AddAnimation(animations[1]);
        }
    }

    void AdjustViewPlane()
    {
        float frustumHeight = 2.0f * transform.localPosition.z * Mathf.Tan(gameCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        float height = (240f / Screen.height) * frustumHeight;
        height *= (int)Screen.height / 240;

        transform.localScale = new Vector3(height * 320f / 240f, height, 1);
        transform.localPosition = new Vector3(transform.localPosition.x, -frustumHeight / 2f + height / 2, transform.localPosition.z);
    }

    List<Animation> animationBuffer;
    Animation currentAnimation;
    Frame currentFrame;

    public void Init(MiniRenderer mr)
    {
        R = mr;

        animationBuffer = new List<Animation>();
        currentAnimation = new Animation();
        currentAnimation.frames = new List<Frame>();
        AddAnimation(animations[6]);
        AddAnimation(animations[2]);
    }

    public void FrameUpdate()
    {
        if (PauseSystem.paused) return;

        Vector2 v = new Vector2(sc.velocity.x, sc.velocity.z) * (1f/R.frameRate);

        if (sc.isGrounded) bob += v.magnitude;
        else bob = 0;

        float bobx = 8 * Mathf.Sin(bob * bobIntensity);
        float boby = 2 * Mathf.Abs(Mathf.Sin(bob * bobIntensity));

        targetPosition = new(bobx, 8 - boby + Mathf.Max(sc.velocity.y * (1f / R.frameRate) * 32, -16));
        //Debug.Log(bob);

        currentFrame = NextFrame();
        targetPosition += currentFrame.position;

        int fx = (currentFrame.frame % 4) * 320;
        int fy = (currentFrame.frame / 4) * 240;

        viewPosition += (targetPosition - viewPosition) / 2;
        if (viewPosition.y < 0) viewPosition.y = 0;

        R.spr(fx, fy, viewPosition.x, viewPosition.y, 320, 240);

        R.Display();
    }

    void AddAnimation(Animation anim)
    {
        Animation a = new Animation();

        a.frames = new List<Frame>();
        foreach (Frame f in anim.frames) a.frames.Add(f);

        animationBuffer.Add(a);
    }

    Frame NextFrame()
    {
        if(currentAnimation.frames.Count > 0)
        {
            Frame frame = currentAnimation.frames[0];
            currentAnimation.frames.RemoveAt(0);
            return frame;
        }

        if(animationBuffer.Count > 0)
        {
            currentAnimation = animationBuffer[0];
            animationBuffer.RemoveAt(0);

            return NextFrame();
        }

        return currentFrame;
    }
}
