using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    [Header("Base")]
    public float MaxHealth;
    public float MaxShield;
    public GameObject GunObject;

    [Header("Movement")]
    public float MovementSpeed;
    public float StoppingDistance;

    [Header("Stats")]
    public float AttackSpeed;
    public float AttackDamage;
    public float AttackCooldown;

    // Other Variables
    private Transform player;
    private NavMeshAgent agent;

    private float currentHealth;
    private float currentShield;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        agent.speed = MovementSpeed;
        agent.stoppingDistance = StoppingDistance;
    }

    private void Update()
    {
        if (agent.enabled)
        {
            EnemyMovement();
        }
    }

    public virtual void EnemyMovement()
    {
        agent.SetDestination(player.position);
    }

    public virtual void TakeDamage(float bulletDamage)
    {
        //Debug.Log("Hit");

        if (currentShield < MaxShield)
        {
            currentShield = Mathf.Min(currentShield + bulletDamage, MaxShield);
        }
        else if (currentHealth < MaxHealth)
        {
            currentHealth = Mathf.Min(currentHealth + bulletDamage, MaxHealth);
        }

        CheckStats();
    }

    public void CheckStats()
    {
        if (currentHealth >= MaxHealth)
        {
            GunObject.GetComponent<Rigidbody>().isKinematic = false;
            GunObject.transform.parent = null;
            Destroy(gameObject);
        }
    }
}
