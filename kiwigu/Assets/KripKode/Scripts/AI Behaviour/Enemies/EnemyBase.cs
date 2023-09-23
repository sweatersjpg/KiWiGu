using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Main Variables")]
    [Range(10, 100)] public int MaxHealth = 100;
    [Range(10, 100)] public int MaxShield = 100;
    public GameObject GunObject;
    public GameObject EyesPosition;

    [Header("Enemy Movement")]
    [Range(1, 15)] public int MovementSpeed = 5;
    [Range(5, 10)] public int AvoidPlayerDistance = 7;
    [Range(10, 15)] public int FleeDistance = 12;
    [Range(5, 10)] public int FleeMovementVariation = 7;
    [Range(100, 200)] public int RotationSpeed = 180;
    [Range(15, 25)] public int EnemyAwareDistance = 20;
    [Range(5, 20)] public int WanderRadius = 8;

    [Header("Enemy Gun Stats")]
    [Range(1, 10)] public float EnemyFireRate = 1.0f;
    [Range(0, 10)] public int GunInaccuracy = 5;
    public GunInfo GunAssetInfo;

    [Header("Shared Variables")]
    [HideInInspector] public Transform player;
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentShield;
    [HideInInspector] public bool isHoldingGun;
    [HideInInspector] public bool wasHit;
    [HideInInspector] public bool isWandering;
    [HideInInspector] public bool startedFleeing;
    [HideInInspector] public bool isShooting;

    private Vector3 wanderTarget;
    private Vector3 initialPosition;
    private bool playerInSight;

    protected virtual void Start()
    {
        initialPosition = transform.position;

        isHoldingGun = GunObject != null && GunObject.transform.parent != null;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = MovementSpeed;
            agent.angularSpeed = RotationSpeed;
        }
    }

    protected virtual void Update()
    {
        playerInSight = CheckPlayerVisibility();

        if (isHoldingGun)
        {
            if (agent != null && (playerInSight || wasHit))
            {
                EnemyMovement();
            }
            else if (!isWandering)
            {
                StartCoroutine(Wander());
            }
        }
        else
        {
            if (playerInSight || wasHit)
            {
                if (!startedFleeing)
                {
                    agent.ResetPath();
                    StartCoroutine(EnemyFlee());
                }
            }
            else
            {
                if (startedFleeing)
                {
                    return;
                }

                if (!isWandering)
                {
                    StartCoroutine(Wander());
                }
            }
        }
    }

    private IEnumerator EnemyFlee()
    {
        Vector3 fleeDestination = FindFleeDestination();

        agent.SetDestination(fleeDestination);

        yield return new WaitUntil(() => agent.remainingDistance < 0.5f);

        startedFleeing = false;
    }

    private Vector3 FindFleeDestination()
    {
        Vector3 awayFromPlayer = transform.position - player.position;
        awayFromPlayer.Normalize();

        Vector3 fleeDestination = transform.position + awayFromPlayer * FleeDistance;

        float randomOffsetX = Random.Range(-FleeMovementVariation, FleeMovementVariation);
        float randomOffsetZ = Random.Range(-FleeMovementVariation, FleeMovementVariation);
        Vector3 randomOffset = new Vector3(randomOffsetX, 0f, randomOffsetZ);
        fleeDestination += randomOffset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeDestination, out hit, FleeDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return transform.position;
        }
    }

    private IEnumerator Wander()
    {
        wanderTarget = RandomWanderPoint();
        agent.SetDestination(wanderTarget);
        isWandering = true;
        yield return new WaitUntil(() => agent.remainingDistance <= 0.5f);
        yield return new WaitForSeconds(Random.Range(4, 6));
        isWandering = false;
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
        if (Vector3.Distance(transform.position, player.position) <= AvoidPlayerDistance)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), RotationSpeed * Time.deltaTime);

            if (GunObject)
            {
                GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;
                GunObjectExitPoint.transform.LookAt(player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy)));
            }
        }
        else
        {
            if (GunObject)
            {
                GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;
                GunObjectExitPoint.transform.LookAt(player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy)));
            }

            Vector3 offset = (transform.position - player.position).normalized * AvoidPlayerDistance;
            agent.SetDestination(player.position + offset);
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
            if (isHoldingGun)
            {
                GunObject.GetComponent<Rigidbody>().isKinematic = false;
                GunObject.transform.parent = null;
                Destroy(GunObject, 60);
                isHoldingGun = false;
            }

            Destroy(gameObject);
        }
    }

    public virtual bool CheckPlayerVisibility()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, EnemyAwareDistance);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                Vector3 direction = player.position - EyesPosition.transform.position + new Vector3(0, 0.5f, 0);
                RaycastHit[] hits = Physics.RaycastAll(EyesPosition.transform.position, direction, EnemyAwareDistance);

                Debug.DrawLine(EyesPosition.transform.position, player.position, Color.red);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        return false;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}