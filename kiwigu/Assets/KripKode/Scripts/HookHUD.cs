using UnityEngine;
using UnityEngine.UI;

public class HookHUD : MonoBehaviour
{
    public RectTransform reticleIcon;
    public Image leftHook;
    public Image rightHook;

    public Sprite hook;
    public Sprite noHook;

    public float maxDistance = 12f;
    public float lerpSpeed = 5f;

    public GameObject hookIconLeft;
    public GameObject hookIconRight;

    private void Update()
    {
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

        return Vector3.Distance(transform.position, hit) < maxDistance;
    }

    private void UpdateIconProperties(Vector3 hit)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(hit);
        // reticleIcon.rectTransform.position = screenPos + new Vector3(0, 10f, 0);
        reticleIcon.position = Vector3.Lerp(reticleIcon.position,
            screenPos + new Vector3(0, 10f, 0), Time.deltaTime * lerpSpeed * 2);

        Color targetColor = new Color(1f, 1f, 1f, 1f);
        // reticleIcon.color = Color.Lerp(reticleIcon.color, targetColor, Time.deltaTime * (lerpSpeed * 2));
    }
}
