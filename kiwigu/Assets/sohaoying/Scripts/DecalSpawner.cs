using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEditor.PlayerSettings;

public class DecalSpawner : MonoBehaviour
{
    //[SerializeField] Color color;

    [SerializeField] float minRadius = 0.05f;
    [SerializeField] float maxRadius = 1f;
    [SerializeField] float height = 0.5f;
    [Space]
    [SerializeField] float decalDuration = 60f;
    [Space]
    ParticleSystem part;
    List<ParticleCollisionEvent> collisionEvents;
    GameObject bloodDecalInstance;

    [SerializeField] GameObject bloodDecalPrefab;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();

        //bloodDecalPrefab.GetComponent<DecalProjector>().material.SetColor("_Color", color);
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numCollisionEvents; i++)
        {

            Vector3 pos = collisionEvents[i].intersection;
            float radius = Random.Range(minRadius, maxRadius);

            bloodDecalPrefab.GetComponent<DecalProjector>().size = new Vector3(radius, radius, height);
            bloodDecalInstance = Instantiate(bloodDecalPrefab, pos, bloodDecalPrefab.transform.rotation);
            Destroy(bloodDecalInstance, decalDuration);
        }
    }
}