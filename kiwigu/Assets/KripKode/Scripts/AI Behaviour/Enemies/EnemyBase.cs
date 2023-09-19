using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Main Variables")]
    [Range(10, 100)]
    public int MaxHealth;
    [Range(10, 100)]
    public int MaxShield;
    public GameObject GunObject;

    [Header("Enemy Movement")]
    [Range(1, 15)]
    public int MovementSpeed;
    [Range(5, 20)]
    public int AvoidPlayerDistance;
    [Range(100, 200)]
    public int RotationSpeed;
    [Range(10, 50)]
    public int EnemyAwareDistance;
    public bool CanTakeCover;

    [Header("Enemy Gun Stats")]
    [Range(1, 10)]
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
        agent.stoppingDistance = AvoidPlayerDistance;
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
        if (CanTakeCover)
        {
            if (Vector3.Distance(transform.position, player.position) <= AvoidPlayerDistance)
            {
                Vector3 direction = player.position - transform.position;
                direction.y = 0;

                Vector3 coverPosition = FindClosestCover();

                // KEEP working on this uwu
                if (transform.position != coverPosition)
                {
                    agent.SetDestination(coverPosition);
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), RotationSpeed * Time.deltaTime);
                    GunObject.transform.LookAt(player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy)));
                }
            }
            else
            {
                GunObject.transform.LookAt(player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy)));
                agent.SetDestination(player.position);
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, player.position) <= AvoidPlayerDistance)
            {
                Vector3 direction = player.position - transform.position;
                direction.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), RotationSpeed * Time.deltaTime);

                GunObject.transform.LookAt(player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy)));
            }
            else
            {
                GunObject.transform.LookAt(player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy)));
                agent.SetDestination(player.position);
            }
        }
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

        if (Physics.Raycast(transform.position, direction, out hit, EnemyAwareDistance))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    public virtual Vector3 FindClosestCover()
    {
        NavMeshHit hit;
        if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return Vector3.zero;
    }
}
