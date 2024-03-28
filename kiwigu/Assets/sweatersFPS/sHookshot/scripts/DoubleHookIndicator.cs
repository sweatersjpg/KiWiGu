using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleHookIndicator : MonoBehaviour
{
    [SerializeField] GameObject toEnable;
    public float maxDistance = 12f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (CheckForHookTarget(out HookTarget ht) && HasBothHooks())
        {
            if (!ht.tether && ht.resistance != 69)
            {
                toEnable.SetActive(true);
                return;
            }
        }

        toEnable.SetActive(false);
    }

    bool HasBothHooks()
    {
        return sweatersController.instance.gameObject.GetComponentsInChildren<ThrowHook>().Length == 2;
    }

    private bool CheckForHookTarget(out HookTarget ht)
    {
        //return Physics.Raycast(transform.position, transform.forward, out hit, maxDistance) &&
        //       hit.collider.CompareTag("HookTarget");
        Vector3 hit = AcquireTarget.instance.GetJustHookTarget(out ht);

        return Vector3.Distance(transform.position, hit) < maxDistance;
    }
}
