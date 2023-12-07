using UnityEngine;
using UnityEngine.UI;

public class HookHUD : MonoBehaviour
{
    public Image reticleIcon;

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

        if (!hookIconLeft.activeInHierarchy && !hookIconRight.activeInHierarchy) 
            return;

        UpdateHookIcon();
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
        Color targetColor = new Color(1f, 1f, 1f, 0f);

        reticleIcon.rectTransform.localPosition = Vector3.Lerp(reticleIcon.rectTransform.localPosition, targetLocalPosition, Time.deltaTime * lerpSpeed);
        reticleIcon.color = Color.Lerp(reticleIcon.color, targetColor, Time.deltaTime * (lerpSpeed * 2));
    }

    private bool CheckForHookTarget(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, transform.forward, out hit, maxDistance) &&
               hit.collider.CompareTag("HookTarget");
    }

    private void UpdateIconProperties(RaycastHit hit)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(hit.transform.position);
        reticleIcon.rectTransform.position = screenPos + new Vector3(0, 10f, 0);

        Color targetColor = new Color(1f, 1f, 1f, 1f);
        reticleIcon.color = Color.Lerp(reticleIcon.color, targetColor, Time.deltaTime * (lerpSpeed * 4));
    }
}
