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
    [HideInInspector] public float maxResistance;
    public bool tether = false;
    public bool swing = false;
    public bool blockSteal;

    private void Start()
    {
        // Mesh mesh = info.gunPrefab.GetComponentInChildren<MeshFilter>().sharedMesh;
        if (hasView)
        {
            Transform gunView = info.gunPrefab.transform.Find("GunView");

            if (gunView)
            {
                GameObject gun = Instantiate(gunView.gameObject, transform);
                gun.transform.localPosition = new();
                gun.layer = gameObject.layer;

                foreach (Transform child in gun.transform)
                {
                    child.gameObject.layer = gameObject.layer;
                }
            }

            //gunView.GetComponent<MeshFilter>().mesh = info.gunPrefab.GetComponentInChildren<MeshFilter>().sharedMesh;
            //gunView.GetComponent<MeshRenderer>().sharedMaterial = info.gunPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        }

        maxResistance = resistance;
    }

    public void BeforeDestroy()
    {
        ThrownGun gun = Instantiate(throwGunPrefab, transform).GetComponent<ThrownGun>();
        gun.transform.parent = null;

        //gun.SetMesh(gunView.GetComponent<MeshFilter>().mesh, info.gunPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial);
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
