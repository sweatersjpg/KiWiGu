using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] GameObject daddy;
    [SerializeField] GameObject directionalPrefab;
    [SerializeField] HealthBar healthBar;
    
    float health;

    [Space]
    [SerializeField] float totalHealth = 300;
    [SerializeField] float regenRate = 5;

    [Space]
    [SerializeField] float regenBufferTime = 1;

    float regenBuffer = 0;

    float deltaTime;

    // Start is called before the first frame update
    void Start()
    {
        health = totalHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PauseSystem.paused) deltaTime = Time.deltaTime;
        else deltaTime = 0;

        if (regenBuffer <= 0)
        {
            health += deltaTime * regenRate;
            if (health > totalHealth) health = totalHealth;
        }
        else regenBuffer -= deltaTime;

        print(health / totalHealth + " " + health);

        CheckStats();
    }

    public void FixedUpdate()
    {
        if (!PauseSystem.paused) healthBar.TargetPercent = health / totalHealth;
    }

    public void DealDamage(float damage, Vector3 source)
    {
        RectTransform indicator = Instantiate(directionalPrefab, daddy.transform).GetComponent<RectTransform>();

        Quaternion sourceRot = Quaternion.LookRotation(source);

        sourceRot.z = -sourceRot.y;
        sourceRot.x = sourceRot.y = 0;

        Vector3 northDirection = new Vector3(0, 0, transform.eulerAngles.y);

        indicator.localRotation = sourceRot * Quaternion.Euler(northDirection);

        regenBuffer = regenBufferTime;
        health -= damage;
    }

    void CheckStats()
    {
        if (health >= 0) return;

        PauseSystem.pauseSystem.ReloadScene();
    }
}
