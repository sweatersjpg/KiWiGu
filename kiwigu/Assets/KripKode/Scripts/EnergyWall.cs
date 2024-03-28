using System.Collections;
using UnityEngine;

public class EnergyWall : MonoBehaviour
{
    public float targetScale;
    public float scaleSpeed;

    private Vector3 initialPosition;

    private bool isScalingUp = false;
    private bool isDoneTime;

    [SerializeField] float health = 100;
    float maxHealth;

    [SerializeField] Renderer front;

    [ColorUsage(true, true)]
    [SerializeField] Color endColor;
    Color startColor;

    [SerializeField] GameObject explosionPrefab;

    [SerializeField] string explosionType;

    private void Start()
    {
        GlobalAudioManager.instance.PlayExplosion(transform, explosionType);

        InitializePositionAndScale();
        StartCoroutine(ScaleUpAndDestroy());

        startColor = front.material.GetColor("_Color");
        maxHealth = health;
    }

    private void Update()
    {
        UpdateScaling();

        float dmg = Mathf.Lerp(front.material.GetFloat("_DamagePercent"), 1 - (health / maxHealth), Time.deltaTime * 5);
        front.material.SetFloat("_DamagePercent", dmg);
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

    public void TakeDamage(object[] args)
    {
        Vector3 point = (Vector3) args[0];
        Vector3 direction = (Vector3) args[1];
        float damage = (float) args[2];

        // if (Vector3.Dot(transform.right, direction) > 0) return;
        // front.material.SetColor("_Color", Color.Lerp(endColor, startColor, health / maxHealth));

        health -= damage;

        if(health <= 0)
        {
            Instantiate(explosionPrefab, point, Quaternion.LookRotation(Vector3.up, -direction));
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
