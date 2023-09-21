using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunHand : MonoBehaviour
{

    [HideInInspector] public bool obstructed = false; // gun against wall

    [HideInInspector] public bool downSights = false;

    Vector3 startPosition; // local
    Vector3 targetPosition; // local

    public int mouseButton;
    public float aimDelay = 0.4f;

    public GunInfo info;
    float gunAngle = 0;
    float targetAngle = 0;

    float aimTimer = 0;

    public bool canShoot;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.localPosition;
        targetPosition = startPosition;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //if (Input.GetMouseButtonDown(1)) ToggleDownSights();
        //if (Input.GetMouseButtonDown(0)) AnimateShoot();

        ObstacleAvoidance();

        if (Input.GetMouseButton(mouseButton) && aimTimer <= aimDelay) aimTimer += Time.deltaTime;
        if (!Input.GetMouseButton(mouseButton) && aimTimer >= 0) aimTimer -= Time.deltaTime;

        if (aimTimer <= 0 && downSights) ToggleDownSights();
        if (aimTimer >= aimDelay && !downSights && info.canAim) ToggleDownSights();

        //if (Input.GetMouseButtonUp(mouseButton))
        //{
        //    //if(downSights) Invoke(nameof(ToggleDownSights), 0.1f);
        //    //aimTimer = 0;
        //    AnimateShoot();
        //}

        transform.localPosition += 50 * ((targetPosition - transform.localPosition) / 4) * Time.deltaTime;

        if (!canShoot) targetAngle = 5;
        else targetAngle = 0;

        gunAngle += 50 * ((targetAngle - gunAngle) / 8) * Time.deltaTime;
        transform.localEulerAngles = new(gunAngle, 0, 0);
    }

    public void ObstacleAvoidance()
    {
        Vector3 origin = transform.parent.position + new Vector3(0, transform.localPosition.y, 0);
        Vector3 direction = (transform.position - origin).normalized;

        Debug.DrawRay(origin, direction);

        RaycastHit hit;

        bool hasHit = Physics.Raycast(origin, direction, out hit, direction.magnitude, ~LayerMask.GetMask("GunHand", "Player"));

        Vector3 offset = new(0,0,0);

        if(hasHit)
        {
            offset = hit.point - (origin + direction);

            targetPosition = transform.InverseTransformPoint(transform.TransformPoint(startPosition) + (hit.normal + new Vector3(0,-1,0)) * offset.magnitude);
            targetAngle = offset.magnitude * -100;

            return;
        }

        targetPosition = startPosition;
        if(downSights) targetPosition = new(0, -0.17f, startPosition.z);
        targetAngle = 0;

    }

    public void ToggleDownSights()
    {
        if(downSights) targetPosition = startPosition;
        else targetPosition = new(0, -0.19f, startPosition.z);

        downSights = !downSights;
    }

    public void AnimateShoot()
    {
        gunAngle -= info.recoil;

        transform.localPosition += new Vector3(0, 0, -0.1f);
    }
}
