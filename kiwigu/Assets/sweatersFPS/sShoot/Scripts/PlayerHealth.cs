using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] HealthBar healthBar;
    
    float health;

    [Space]
    [SerializeField] float totalHealth = 300;
    [SerializeField] float regenRate = 5;

    [Space]
    [SerializeField] float regenBufferTime = 1;

    float regenBuffer;

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
            health += Mathf.Min(deltaTime * regenRate, totalHealth - health);
        }
        else regenBuffer -= deltaTime;

        healthBar.TargetPercent = health / totalHealth;

        CheckStats();
    }

    public void DealDamage(float damage, Vector3 direction)
    {
        regenBuffer = regenBufferTime; // resetBuffer;

        health -= damage;
    }

    void CheckStats()
    {
        if (health >= 0) return;

        PauseSystem.pauseSystem.ReloadScene();
    }
}
