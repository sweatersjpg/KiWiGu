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

    public void DealDamage(float damage, Vector3 source)
    {
        // Please fix this :(

        // Instantiate a UI element from the 'directionalPrefab' and attach it to the 'daddy' object.
        RectTransform indicator = Instantiate(directionalPrefab, daddy.transform).GetComponent<RectTransform>();

        // Calculate the direction from this object's position to the 'source' position.
        Vector3 _direction = transform.position - source;

        // Calculate a rotation that points from this object to the 'source'.
        Quaternion sourceRot = Quaternion.LookRotation(_direction);

        // Adjust the 'sourceRot' to control the rotation of the UI element.
        sourceRot.z = -sourceRot.y;
        sourceRot.x = sourceRot.y = 0; // reset the 'x' and 'y' components of 'sourceRot'.

        // Create a 'northDirection' vector using the 'y' component of the current object's rotation.
        Vector3 northDirection = new Vector3(0, 0, transform.eulerAngles.y);

        // Combine 'sourceRot' and 'northDirection' to set the local rotation of the UI element.
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
