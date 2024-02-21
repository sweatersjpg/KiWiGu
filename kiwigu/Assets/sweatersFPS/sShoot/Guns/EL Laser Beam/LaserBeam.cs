using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class LaserBeam : MonoBehaviour
{
    // Start is called before the first frame update

    public GunHand anim;
    public GameObject sparks;

    LineRenderer line;
    VisualEffect effect;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        effect = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {

        string[] shootButtons = { "LeftShoot", "RightShoot" };
        string shootButton = shootButtons[anim.mouseButton];

        if(Input.GetButton(shootButton) && !anim.outOfAmmo)
        {
            if(line.enabled == false) line.enabled = true;
            if(effect.enabled == false) effect.enabled = true;
            sparks.SetActive(true);
        } else
        {
            line.enabled = false;
            effect.enabled = false;
            sparks.SetActive(false);
        }

        Vector3 target = AcquireTarget.instance.target;

        line.SetPosition(1, transform.InverseTransformPoint(target));

        sparks.transform.position = target;
    }

}
