using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Terrain terrain;
    TerrainData data;
    int width;

    public GameObject startPoint;
    public Vector3 searchArea;
    public float stepSize = 1;

    [Header("Triggers")]
    public bool applyChanges = false;
    public bool ResetMap = false;
    public bool scanHeights = false;

    float[,] heightMap;
    
    // Start is called before the first frame update
    void Start()
    {
        data = terrain.terrainData;
        width = data.heightmapResolution;

        heightMap = new float[width, width];

        // ScanHeights();
    }

    // Update is called once per frame
    void Update()
    {
        if(scanHeights)
        {
            scanHeights = false;
            ScanHeights();
        }

        ResetHeightMap();
        ApplyChanges();
    }

    void ScanHeights()
    {
        Debug.Log("Starting scan...");

        for(float x = 0; x < searchArea.x / stepSize; x += stepSize)
        {
            for (float z = 0; z < searchArea.z / stepSize; z += stepSize)
            {
                Vector3 start = startPoint.transform.position;
                Vector3 point = new Vector3(start.x + x, start.y, start.z + z);

                bool hasHit = Physics.Raycast(new Ray(point, Vector3.down), out RaycastHit hit, searchArea.y);

                if(hasHit)
                {
                    MatchHeight(hit.point);
                }
            }
        }

        applyChanges = true;

        Debug.Log("Scan complete");
    }

    void MatchHeight(Vector3 point)
    {
        point = WorldToMapPoint(point);
        
        heightMap[Mathf.RoundToInt(point.z), Mathf.RoundToInt(point.x)] = point.y / (float) width;
    }
    
    Vector3 WorldToMapPoint(Vector3 point)
    {
        point -= terrain.transform.position; // world to local

        return point / 1000 * width;
    }

    void ApplyChanges()
    {
        if (!applyChanges) return;
        applyChanges = false;

        data.SetHeights(0, 0, heightMap);
    }

    void ResetHeightMap()
    {
        if (!ResetMap) return;
        ResetMap = false;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                heightMap[i, j] = 0;
            }
        }

        data.SetHeights(0, 0, heightMap);
    }
}
