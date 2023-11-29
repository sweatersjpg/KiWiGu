using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Radar")]
    [SerializeField] GameObject radar;
    [SerializeField][Range(50, 1000)] float radarSpeed;
    [SerializeField][Range(50, 500)] float radarRadius = 50f;
    [SerializeField] RectTransform radarCone;
    [SerializeField] GameObject enemyDotPrefab;
    [SerializeField] RectTransform radarEnemySpots;
    [SerializeField] Color enemyDotColor;

    [Header("Shared Variables")]
    [HideInInspector] public Transform player;

    private Dictionary<Transform, GameObject> spawnedEnemyDots = new Dictionary<Transform, GameObject>();


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) radar.SetActive(!radar.activeSelf);

        if (!radar.activeSelf) return;
        Radar();
        radarCone.Rotate(0f, 0f, -radarSpeed * Time.deltaTime);
    }

    void Radar()
    {
        Vector3 radarDirection = Quaternion.Euler(0, -radarCone.rotation.eulerAngles.z, 0) * player.forward;

        int enemyLayerMask = 1 << LayerMask.NameToLayer("Enemy");

        Collider[] colliders = Physics.OverlapSphere(player.position, radarRadius, enemyLayerMask);

        foreach (var collider in colliders)
        {
            Transform enemyTransform = collider.transform;

            Vector3 directionToEnemy = (enemyTransform.position - player.position).normalized;

            float dotProduct = Vector3.Dot(directionToEnemy, radarDirection);

            if (dotProduct >= Mathf.Cos(Mathf.Deg2Rad * 45))
            {
                if (!spawnedEnemyDots.ContainsKey(enemyTransform))
                {
                    Vector3 radarPos = (enemyTransform.position - player.position) / radarRadius;
                    radarPos = Quaternion.Euler(0, -player.eulerAngles.y, 0) * radarPos;

                    Vector3 UIpos = new Vector3(radarPos.x, radarPos.z, 0f);

                    GameObject enemyDot = Instantiate(enemyDotPrefab, radarEnemySpots);
                    enemyDot.GetComponent<RectTransform>().anchoredPosition = UIpos * ActualSize(radarEnemySpots, GetComponent<Canvas>());
                    enemyDot.GetComponent<Image>().color = enemyDotColor;

                    spawnedEnemyDots.Add(enemyTransform, enemyDot);

                    StartCoroutine(SpotBehaviour(enemyDot.GetComponent<Image>(), enemyTransform));
                }
            }
        }
    }

    public Vector2 ActualSize(RectTransform trans, Canvas can)
    {
        var v = new Vector3[4];
        trans.GetWorldCorners(v);
        return RectTransformUtility.PixelAdjustRect(trans, can).size;
    }

    IEnumerator SpotBehaviour(Image spot, Transform enemyTransform)
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

        spawnedEnemyDots.Remove(enemyTransform);
        Destroy(spot.gameObject);
    }
}
