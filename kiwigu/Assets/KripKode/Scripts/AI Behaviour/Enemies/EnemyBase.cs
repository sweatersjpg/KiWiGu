using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

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
    [Range(5, 10)]
    public int AvoidPlayerDistance;
    [Range(100, 200)]
    public int RotationSpeed;
    [Range(10, 20)]
    public int EnemyAwareDistance;
    [Range(4, 15)]
    public int WanderRadius;
    public bool TakeCover;

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

    private float wanderTimer = 5f;
    private Vector3 wanderTarget;
    private Vector3 initialPosition;
    private bool isWandering;

    protected virtual void Start()
    {
        initialPosition = transform.position;

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
            isWandering = false;
            EnemyMovement();
        }
        else
        {
            isWandering = true;
            Wander();
        }
    }

    private void Wander()
    {
        if (isWandering)
        {
            wanderTimer -= Time.deltaTime;

            if (wanderTimer <= 0)
            {
                wanderTarget = RandomWanderPoint();
                wanderTimer = 5f;
            }

            agent.SetDestination(wanderTarget);
        }
    }

    private Vector3 RandomWanderPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * WanderRadius;
        randomDirection += initialPosition;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, WanderRadius, NavMesh.AllAreas);

        return hit.position;
    }

    public virtual void EnemyMovement()
    {
        if (TakeCover)
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
                agent.SetDestination(FindClosestEdgeOnNavMesh());
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
    Vector3 FindClosestEdgeOnNavMesh()
    {
        NavMeshHit hit;
        Vector3 position = transform.position;

        if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
        {
            position = hit.position;
        }

        return position;
    }
}
