using UnityEngine;
using UnityEngine.UI;

public class HookHUD : MonoBehaviour
{
    public Image reticleIcon;

    public float maxDistance = 12f;
    public float lerpSpeed = 5f;

    private void Update()
    {
        UpdateHookIcon();

        if (!CheckForHookTarget(out _))
        {
            LerpIconToCenter();
        }
    }

    private void UpdateHookIcon()
    {
        bool hitHookTarget = CheckForHookTarget(out RaycastHit hit);

        if (hitHookTarget)
        {
            UpdateIconProperties(hit);
        }
    }

    private void LerpIconToCenter()
    {
        Vector3 targetLocalPosition = new Vector3(0f, 0f, 0f);

        reticleIcon.rectTransform.localPosition = Vector3.Lerp(reticleIcon.rectTransform.localPosition, targetLocalPosition, Time.deltaTime * lerpSpeed);

        //float targetScale = 0.5f;
        //hookIcon.rectTransform.localScale = Vector3.Lerp(hookIcon.rectTransform.localScale, new Vector3(targetScale, targetScale, 1f), Time.deltaTime * lerpSpeed);
    }

    private bool CheckForHookTarget(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, transform.forward, out hit, maxDistance) &&
               hit.collider.CompareTag("HookTarget");
    }

    private void UpdateIconProperties(RaycastHit hit)
    {
        float distance = Vector3.Distance(transform.position, hit.transform.position);

        //float scale = 1.75f - Mathf.Clamp01(distance / maxDistance);
        //hookIcon.rectTransform.localScale = new Vector3(scale, scale, 1f);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(hit.transform.position);
        reticleIcon.rectTransform.position = screenPos + new Vector3(0, 10f, 0);
    }
}
