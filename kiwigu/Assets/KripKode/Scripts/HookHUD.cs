using UnityEngine;
using UnityEngine.UI;

public class HookHUD : MonoBehaviour
{
    public RectTransform reticleIcon;
    public Image leftHook;
    public Image rightHook;

    public Sprite hook;
    public Sprite noHook;

    private float maxDistance = 20f;
    private float lerpSpeed = 5f;

    public GameObject hookIconLeft;
    public GameObject hookIconRight;

    static bool shownBlockDialog = false;

    private void Update()
    {
        if (PauseSystem.paused)
        {
            LerpColor(new Color(1, 1, 1, 0));
            return;
        }

        if (!CheckForHookTarget(out _))
        {
            LerpIconToCenter();
        }

        //if (!hookIconLeft.activeInHierarchy && !hookIconRight.activeInHierarchy) 
        //    return;
        leftHook.gameObject.SetActive(hookIconLeft.activeInHierarchy);
        rightHook.gameObject.SetActive(hookIconRight.activeInHierarchy);

        UpdateHookIcon();

        // if(PauseSystem.paused) reticleIcon.color = new Color(1f, 1f, 1f, 0f);
    }

    private void UpdateHookIcon()
    {
        bool hitHookTarget = CheckForHookTarget(out Vector3 hit);

        if(hitHookTarget && !shownBlockDialog)
        {
            DoBlockDialog();
            shownBlockDialog = true;
        }

        if (hitHookTarget)
        {
            UpdateIconProperties(hit);
        }
    }

    private void LerpIconToCenter()
    {
        Vector3 targetLocalPosition = new Vector3(0f, 0f, 0f);
        Color targetColor = new Color(1f, 1f, 1f, 0f);

        reticleIcon.localPosition = Vector3.Lerp(reticleIcon.localPosition, targetLocalPosition, Time.deltaTime * lerpSpeed);
        // reticleIcon.color = Color.Lerp(reticleIcon.color, targetColor, Time.deltaTime * (lerpSpeed * 2));
        LerpColor(targetColor);
    }

    private bool CheckForHookTarget(out Vector3 hit)
    {
        //return Physics.Raycast(transform.position, transform.forward, out hit, maxDistance) &&
        //       hit.collider.CompareTag("HookTarget");
        hit = AcquireTarget.instance.GetJustHookTarget(out HookTarget ht);

        leftHook.sprite = hook;
        rightHook.sprite = hook;
        if (ht && ht.blockSteal)
        {
            leftHook.sprite = noHook;
            rightHook.sprite = noHook;
        }

        return Vector3.Distance(sweatersController.instance.playerCamera.transform.position, hit) < maxDistance;
    }

    private void DoBlockDialog()
    {
        Debug.Log("blocked dialog goes here");
    }

    private void UpdateIconProperties(Vector3 hit)
    {
        //Debug.Log("hook targetted!!!");

        Vector3 screenPos = Camera.main.WorldToScreenPoint(hit);
        // reticleIcon.rectTransform.position = screenPos + new Vector3(0, 10f, 0);
        reticleIcon.position = Vector3.Lerp(reticleIcon.position,
            screenPos + new Vector3(0, 10f, 0), Time.deltaTime * lerpSpeed * 2);

        Color targetColor = new Color(1f, 1f, 1f, 1f);

        if (leftHook.sprite == noHook) targetColor = Color.red;

        // reticleIcon.color = Color.Lerp(reticleIcon.color, targetColor, Time.deltaTime * (lerpSpeed * 2));
        LerpColor(targetColor);
    }

    void LerpColor(Color targetColor)
    {
        leftHook.color = Color.Lerp(leftHook.color, targetColor, Time.unscaledDeltaTime * (lerpSpeed * 2));
        rightHook.color = Color.Lerp(rightHook.color, targetColor, Time.unscaledDeltaTime * (lerpSpeed * 2));
    }
}
