using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookTarget : MonoBehaviour
{
    public GunInfo info;

    public Transform gunView;
    public GameObject throwGunPrefab;

    public bool hasView = true;

    public float resistance = 2;

    private void Start()
    {        
        Mesh mesh = info.gunPrefab.transform.Find("GunView").GetComponent<MeshFilter>().sharedMesh;
        if (hasView)
        {
            gunView.GetComponent<MeshFilter>().mesh = mesh;
            gunView.GetComponent<MeshRenderer>().sharedMaterial = info.gunPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        }
    }

    public void BeforeDestroy()
    {
        ThrownGun gun = Instantiate(throwGunPrefab, transform).GetComponent<ThrownGun>();
        gun.transform.parent = null;

        gun.SetMesh(gunView.GetComponent<MeshFilter>().mesh, info.gunPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial);
        gun.info = info;
        gun.throwForce = 1;
        
        MoveHook hook = transform.GetComponentInChildren<MoveHook>();

        if (hook != null)
        {
            hook.transform.parent = null;

            hook.TakeThrownGun(gun.gameObject);
        } else
        {
            Destroy(gun.gameObject);
        }
    }
}
