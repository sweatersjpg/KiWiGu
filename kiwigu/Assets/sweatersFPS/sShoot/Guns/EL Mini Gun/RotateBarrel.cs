using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBarrel : MonoBehaviour
{
    // Start is called before the first frame update
    public ShootBullet sb;
    GunInfo info;

    [Space]
    public int segments = 1;
    public float decay = 1;

    float vel;
    // float angle = 0;

    void Start()
    {
        info = sb.anim.info;
    }

    // Update is called once per frame
    void Update()
    {
        // speed = 1 turn per shot
        //  => 360

        string[] shootButtons = { "LeftShoot", "RightShoot" };
        string shootButton = shootButtons[sb.anim.mouseButton];

        vel = Mathf.Lerp(vel, 0, Time.deltaTime * decay);

        if (Input.GetButton(shootButton))
        {
            float fireRate = info.fireRate * sb.charge;

            vel = (360 / segments) * fireRate;
        }
        // angle += vel * Time.deltaTime;

        //transform.localEulerAngles = new Vector3(angle, transform.localEulerAngles.y, transform.localEulerAngles.z);
        transform.Rotate(Vector3.forward, vel * Time.deltaTime);
    }
}
