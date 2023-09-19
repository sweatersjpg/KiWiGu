using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Main Variables")]
    public float MaxHealth;
    public float MaxShield;
    public GameObject GunObject;

    [Header("Enemy Movement")]
    public float MovementSpeed;
    public float StoppingDistance;
    public float RotationSpeed;
    [Range(1, 50)]
    public float EnemyViewDistance;

    [Header("Enemy Gun Stats")]
    [Range(1,10)]
    public float EnemyFireRate;
    [Range(0, 10)]
    public float GunInaccuracy;
    public GunInfo info;

    // Other Shared Variables
    [HideInInspector] public Transform player;
    [HideInInspector] public NavMeshAgent agent;

    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentShield;
    [HideInInspector] public bool isHoldingGun;
    [HideInInspector] public bool wasHit;

    protected virtual void Start()
    {
        if (GunObject.transform.parent != null)
        {
            isHoldingGun = true;
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        agent.speed = MovementSpeed;
        agent.stoppingDistance = StoppingDistance;
        agent.angularSpeed = RotationSpeed;
    }

    protected virtual void Update()
    {
        if (agent.enabled && (CheckPlayerVisibility() || wasHit))
        {
            EnemyMovement();
        }
        else
        {
            agent.ResetPath();
        }
    }

    public virtual void EnemyMovement()
    {

    }

    public virtual void TakeDamage(float bulletDamage)
    {
        wasHit = true;
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
            isHoldingGun = false;
            GunObject.GetComponent<Rigidbody>().isKinematic = false;
            GunObject.transform.parent = null;
            Destroy(GunObject, 60);
            Destroy(gameObject);
        }
    }

    public virtual bool CheckPlayerVisibility()
    {
        Vector3 direction = player.position - transform.position + new Vector3(0, 0.5f, 0);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction, out hit, EnemyViewDistance))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
}
