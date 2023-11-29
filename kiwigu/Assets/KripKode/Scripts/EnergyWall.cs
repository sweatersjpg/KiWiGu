using System.Collections;
using UnityEngine;

public class EnergyWall : MonoBehaviour
{
    public float targetScale;
    public float scaleSpeed;

    private Vector3 initialPosition;

    private bool isScalingUp = false;
    private bool isDoneTime;

    private void Start()
    {
        InitializePositionAndScale();
        StartCoroutine(ScaleUpAndDestroy());
    }

    private void Update()
    {
        UpdateScaling();
    }

    private void InitializePositionAndScale()
    {
        initialPosition = transform.position;

        Vector3 newPosition = initialPosition + new Vector3(0, -0.5f);
        transform.position = newPosition;
    }

    private void UpdateScaling()
    {
        Vector3 currentScale = transform.localScale;


        if(!isDoneTime && isScalingUp)
        {
            currentScale.y += Time.deltaTime * scaleSpeed;
            transform.localScale = currentScale;
        }
        else if(isDoneTime && !isScalingUp)
        {
            currentScale.y -= Time.deltaTime * scaleSpeed;
            transform.localScale = currentScale;
        }

        if (currentScale.y >= targetScale)
        {
            isScalingUp = false;
        }
        else if(currentScale.y <= 0)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator ScaleUpAndDestroy()
    {
        isScalingUp = true;
        yield return new WaitForSeconds(4);
        isDoneTime = true;
        isScalingUp = false;
    }
}
