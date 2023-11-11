using System.Collections;
using UnityEngine;

public class GrenadeSpawner : MonoBehaviour
{
    public GameObject grenadePrefab;
    public float rotationSpeed = 60f;
    public float spawnInterval = 25f;

    void Start()
    {
        StartCoroutine(SpawnGrenades());
    }

    IEnumerator SpawnGrenades()
    {
        float currentRotation = 0f;

        while (currentRotation < 360f)
        {
            GameObject grenade = Instantiate(grenadePrefab, transform.position, Quaternion.identity);

            Vector3 forceDirection = Quaternion.Euler(0, currentRotation, 0) * Vector3.forward;

            Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
            grenadeRb.AddForce(forceDirection * 10f, ForceMode.Impulse);

            currentRotation += spawnInterval;

            yield return new WaitForSeconds(1f / rotationSpeed);
        }
    }
}
