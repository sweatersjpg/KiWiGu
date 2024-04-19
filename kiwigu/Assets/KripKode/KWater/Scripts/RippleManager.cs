using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippleManager : MonoBehaviour
{
    // [Public Variables]
    public int resolution = 512;
    public float waterWaveSpeed = 0.1f;
    public float waterWaveDisplacement = 0.9f;

    public Camera waterCamera;
    public Transform waterObject;
    public GameObject ripplePrefab;
    public RenderTexture ObjectsRT;
    public Shader RippleShader, AddShader;

    // [Private Variables]
    private Dictionary<string, GameObject> rippleDictionary = new Dictionary<string, GameObject>();
    private float defaultRippleSize = 0.5f;
    private RenderTexture CurrRT, PrevRT, TempRT;
    private Material RippleMat, AddMat;

    private void OnDrawGizmos()
    {
        if (waterObject == null)
            return;

        float waterScale = waterObject.localScale.x * 5f;
        waterCamera.orthographicSize = waterScale;

        Material material = waterObject.GetComponent<MeshRenderer>().sharedMaterial;
        if (material != null)
        {
            material.SetFloat("_Wave", waterWaveSpeed);
            material.SetFloat("_WaveDisplacement", waterWaveDisplacement);
            material.SetFloat("_SurfaceDensity", waterScale);
            material.SetFloat("_FoamScale", waterScale * 0.5f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(waterObject.position, waterObject.localScale);
    }

    private void Awake()
    {
        if (waterObject == null)
            return;

        waterObject.localScale = new Vector3(waterObject.localScale.x, 1, waterObject.localScale.x);
    }

    void Start()
    {
        CurrRT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
        PrevRT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
        TempRT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
        RippleMat = new Material(RippleShader);
        AddMat = new Material(AddShader);

        GetComponent<Renderer>().material.SetTexture("_RippleTexture", CurrRT);

        StartCoroutine(RipplesCoroutine());
    }

    IEnumerator RipplesCoroutine()
    {
        AddMat.SetTexture("_ObjectsRT", ObjectsRT);
        AddMat.SetTexture("_CurrentRT", CurrRT);
        Graphics.Blit(null, TempRT, AddMat);

        RenderTexture temp = TempRT;
        TempRT = CurrRT;
        CurrRT = temp;

        RippleMat.SetTexture("_PrevRT", PrevRT);
        RippleMat.SetTexture("_CurrentRT", CurrRT);
        Graphics.Blit(null, TempRT, RippleMat);
        Graphics.Blit(TempRT, PrevRT);

        temp = PrevRT;
        PrevRT = CurrRT;
        CurrRT = temp;

        yield return null;

        StartCoroutine(RipplesCoroutine());
    }

    private void OnTriggerEnter(Collider other)
    {
        AddRipple(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        RemoveRipple(other.transform.GetInstanceID().ToString());
    }


    // These are public in case we want to add ripples from other scripts... rn it only works with collision trigger
    public void AddRipple(Transform location)
    {
        if (rippleDictionary.ContainsKey(location.GetInstanceID().ToString()))
            return;

        GameObject ripple = Instantiate(ripplePrefab, location.position, Quaternion.identity);
        ripple.transform.localScale = new Vector3(defaultRippleSize, defaultRippleSize, defaultRippleSize);
        ripple.GetComponent<RippleFollower>().objectReference = location.gameObject;
        rippleDictionary.Add(location.GetInstanceID().ToString(), ripple);
    }

    public void RemoveRipple(string rippleName)
    {
        if (rippleDictionary.ContainsKey(rippleName))
        {
            Destroy(rippleDictionary[rippleName]);
            rippleDictionary.Remove(rippleName);
        }
    }
}