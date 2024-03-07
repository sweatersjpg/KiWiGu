using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class MiniGunTurret : MonoBehaviour
{

    public GameObject gun;
    public GameObject shield;
    public GameObject barrel;
    public GameObject gunTip;
    public ParticleSystem flash;
    public BulletShooter shooter;
    public HookTarget hookTarget;

    public Renderer front;

    [Space]
    public GunInfo info;
    public float turnSpeed;
    public float range = 50;

    [Space]
    [SerializeField] float health = 100;
    float maxHealth;

    [SerializeField] GameObject explosionPrefab;
    [SerializeField] GameObject shieldBreakPrefab;

    // Start is called before the first frame update
    void Start()
    {
        maxHealth = health;
    }

    // Update is called once per frame
    void Update()
    {
        TrackPlayer();
        
        if(shield)
        {
            float dmg = Mathf.Lerp(front.material.GetFloat("_DamagePercent"), 1 - (health / maxHealth), Time.deltaTime * 5);
            front.material.SetFloat("_DamagePercent", dmg);
        }
        hookTarget.blockSteal = shield;

        if (Vector3.Distance(sweatersController.instance.transform.position, transform.position) < 30)
        {
            shooter.isShooting = Time.time % 9 > 5;
        }
        else shooter.isShooting = false;

        if (hookTarget.transform.parent != transform)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    public void TakeDamage(object[] args)
    {
        Vector3 point = (Vector3)args[0];
        Vector3 direction = (Vector3)args[1];
        float damage = (float)args[2];

        // front.material.SetColor("_Color", Color.Lerp(endColor, startColor, health / maxHealth));

        health -= damage;

        if (health <= 0)
        {
            Instantiate(shieldBreakPrefab, point, Quaternion.LookRotation(Vector3.up, -direction));
            Destroy(shield);
        }
    }

    public void TrackPlayer()
    {
        Quaternion oldR = gun.transform.rotation;
        
        gun.transform.LookAt(sweatersController.instance.playerCamera.transform.position + Vector3.down);

        gun.transform.rotation = Quaternion.Slerp(oldR, gun.transform.rotation, Time.deltaTime * turnSpeed);

    }
}
