using System.Collections;
using UnityEngine;

public class GrenadeSpawner : MonoBehaviour
{
    public GameObject grenadePrefab;
    public float rotationSpeed = 60f;
    public float spawnRadius = 5f;
    public int numGrenades = 12;

    private void Start()
    {
        StartCoroutine(SpawnGrenades());
    }

    private IEnumerator SpawnGrenades()
    {
        float angleIncrement = 360f / numGrenades;

        for (int i = 0; i < numGrenades; i++)
        {
            SpawnGrenadeWithForce(i * angleIncrement);
            yield return new WaitForSeconds(1f / rotationSpeed);
        }
    }

    private void SpawnGrenadeWithForce(float angle)
    {
        float radians = Mathf.Deg2Rad * angle;
        float x = spawnRadius * Mathf.Cos(radians);
        float z = spawnRadius * Mathf.Sin(radians);

        Vector3 spawnPosition = new Vector3(x, transform.position.y, z) + transform.position;

        GameObject grenade = Instantiate(grenadePrefab, spawnPosition, Quaternion.identity);
        Vector3 forceDirection = (grenade.transform.position - transform.position).normalized;

        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
        grenadeRb.AddForce(forceDirection * 10f, ForceMode.Impulse);
    }
}
