using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Radar")]
    [SerializeField][Range(50, 200)] float radarSpeed;
    [SerializeField][Range(5, 100)] float radarRadius = 50f;
    [SerializeField] RectTransform radarCone;
    [SerializeField] GameObject enemyDotPrefab;
    [SerializeField] RectTransform radarEnemySpots;
    [SerializeField] Color enemyDotColor;

    [Header("Shared Variables")]
    [HideInInspector] public Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        ScanEnemySnapshot();

        radarCone.Rotate(0f, 0f, -radarSpeed * Time.deltaTime);
    }

    void ScanEnemySnapshot()
    {
        Vector3 offset = new Vector3(player.position.x, player.position.y + 1, player.position.z);

        Ray ray = new Ray(offset, player.forward * radarRadius);
        ray.direction = Quaternion.Euler(0, -radarCone.rotation.eulerAngles.z + 45, 0) * ray.direction;
        Debug.DrawRay(ray.origin, ray.direction * radarRadius, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, radarRadius))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                Vector3 radarPos = (hit.transform.position - player.position) / radarRadius;
                radarPos = Quaternion.Euler(0, -player.eulerAngles.y, 0) * radarPos;

                Vector3 UIpos = new(radarPos.x, radarPos.z, 0f);

                GameObject enemyDot = Instantiate(enemyDotPrefab, radarEnemySpots);


                enemyDot.GetComponent<RectTransform>().anchoredPosition = UIpos * ActualSize(radarEnemySpots, GetComponent<Canvas>());
                enemyDot.GetComponent<Image>().color = enemyDotColor;
                StartCoroutine(DeleteRatActivity(enemyDot.GetComponent<Image>()));
            }
        }
    }

    public Vector2 ActualSize(RectTransform trans, Canvas can)
    {
        var v = new Vector3[4];
        trans.GetWorldCorners(v);
        return RectTransformUtility.PixelAdjustRect(trans, can).size;
    }

    IEnumerator DeleteRatActivity(Image spot)
    {
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            spot.color = new Color(spot.color.r, spot.color.g, spot.color.b, Mathf.Lerp(0f, 1f, t));
            yield return null;
        }
        t = 1.25f;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            spot.color = new Color(spot.color.r, spot.color.g, spot.color.b, Mathf.Lerp(0f, 1f, t));
            yield return null;
        }

        Destroy(spot.gameObject);
    }

}
