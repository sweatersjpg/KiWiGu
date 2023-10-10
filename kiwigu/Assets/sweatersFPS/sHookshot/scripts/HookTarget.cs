using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookTarget : MonoBehaviour
{
    public GunInfo info;

    public Transform gunView;

    public bool hasView = true;

    private void Start()
    {        
        Mesh mesh = info.gunPrefab.transform.Find("GunView").GetComponent<MeshFilter>().sharedMesh;
        if (hasView) gunView.GetComponent<MeshFilter>().mesh = mesh;
    }
}
