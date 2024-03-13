using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static UnityEngine.UI.Image;

public class LaserView : MonoBehaviour
{

    public BulletShooter shoot;
    public GameObject sparks;

    LineRenderer line;
    VisualEffect effect;

    public GameObject shield;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        effect = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {

        if (shoot.shotsToTake > 0 || shoot.shootForever)
        {
            if (line.enabled == false) line.enabled = true;
            if (effect.enabled == false) effect.enabled = true;
            sparks.SetActive(true);
            // shield.SetActive(true);
        }
        else
        {
            line.enabled = false;
            effect.enabled = false;
            sparks.SetActive(false);
            // shield.SetActive(false);
        }

        Vector3 target = GetTarget();

        line.SetPosition(1, transform.InverseTransformPoint(target));

        sparks.transform.position = target;
    }

    Vector3 GetTarget()
    {
        int mask = ~LayerMask.GetMask("GunHand", "HookTarget");
        bool hasHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 50, mask);

        if (hasHit)
        {
            return hit.point;
        } else
        {
            return transform.position + transform.forward * 50;
        }
    }
}
