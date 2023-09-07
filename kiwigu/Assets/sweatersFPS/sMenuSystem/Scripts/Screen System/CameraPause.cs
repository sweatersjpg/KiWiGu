using UnityEngine;

public class CameraPause : MonoBehaviour
{
    [SerializeField] private RenderTexture _cachedRenderTexture;
    public bool paused;

    Camera self;
    int cullingMask;

    public static CameraPause instance;

    private void Awake()
    {
        instance = this;
        self = GetComponent<Camera>();
        cullingMask = self.cullingMask;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (_cachedRenderTexture == null)
        {
            _cachedRenderTexture = new RenderTexture(src.width, src.height, src.depth);
        }

        if (paused)
        {
            Graphics.Blit(_cachedRenderTexture, dest);
            self.cullingMask = 0;
        }
        else
        {
            self.cullingMask = cullingMask;
            Graphics.CopyTexture(src, _cachedRenderTexture);
            Graphics.Blit(src, dest);
        }
    }
}