using System.Collections;
using UnityEngine;

public class EnergyWall : MonoBehaviour
{
    public float targetScale;
    public float scaleSpeed;

    private Vector3 initialPosition;
    private Vector3 initialScale;
    private bool isScalingUp = false;

    private void Start()
    {
        InitializePositionAndScale();
        StartCoroutine(ScaleUpAndDestroy());
    }

    private void Update()
    {
        if (isScalingUp)
        {
            UpdateScaling();
        }
    }

    private void InitializePositionAndScale()
    {
        initialPosition = transform.position;
        initialScale = transform.localScale;
    }

    private void UpdateScaling()
    {
        Vector3 currentScale = transform.localScale;
        currentScale.y += Time.deltaTime * scaleSpeed;
        transform.localScale = currentScale;

        Vector3 newPosition = initialPosition + Vector3.up * (currentScale.y - initialScale.y) / 2;
        transform.position = newPosition;

        if (currentScale.y >= targetScale)
        {
            isScalingUp = false;
        }
    }

    private IEnumerator ScaleUpAndDestroy()
    {
        isScalingUp = true;
        yield return new WaitForSeconds(4);
        //Destroy(gameObject);
    }
}
