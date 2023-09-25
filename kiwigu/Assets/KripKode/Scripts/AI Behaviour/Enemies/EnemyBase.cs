using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Main Variables")]
    public bool spawnWithGun;
    [Range(10, 100)] public int MaxHealth = 100;
    [Range(10, 100)] public int MaxShield = 100;
    public GameObject GunObject;
    public GameObject EyesPosition;
    public GameObject HandPosition;

    [Header("Enemy Movement")]
    [Range(1, 15)] public int FleeDistance;
    [Range(1, 15)] public int FleeMovementVariation;
    [Range(1, 15)] public int MovementSpeed = 5;
    [Range(5, 10)] public int AvoidPlayerDistance = 7;
    [Range(100, 200)] public int RotationSpeed = 180;
    [Range(15, 25)] public int EnemyAwareDistance = 20;
    [Range(5, 20)] public int WanderRadius = 8;

    [Header("Enemy Gun Stats")]
    [Range(1, 10)] public float EnemyFireRate = 1.0f;
    [Range(0, 10)] public int GunInaccuracy = 5;

    [Header("Shared Variables")]
    [HideInInspector] public Transform player;
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentShield;
    [HideInInspector] public bool isHoldingGun;
    [HideInInspector] public bool wasHit;
    [HideInInspector] public bool isWandering;
    [HideInInspector] public bool isShooting;
    [HideInInspector] public bool playerInSight;

    private Vector3 wanderTarget;
    private Vector3 initialPosition;

    GameObject InitialGunObject;
    private bool startedFleeing;

    protected virtual void Start()
    {
        // set tag to "Enemy"
        gameObject.tag = "Enemy";

        if (spawnWithGun && GunObject)
        {
            GunObject = Instantiate(GunObject, HandPosition.transform);
            isHoldingGun = true;
        }

        if (GunObject)
            InitialGunObject = GunObject;

        initialPosition = transform.position;

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
            if (GunObject)
            {
                if (IsAgentCloseToStation())
                {
                    GunObject = Instantiate(InitialGunObject, HandPosition.transform);
                    isHoldingGun = true;
                }
                else
                {
                    GameObject closestStation = FindClosestStationWithTag("EnemyRestockStation");

                    if (closestStation != null)
                    {
                        agent.SetDestination(closestStation.transform.position);
                    }
                }
            }
            else
            {
                if (agent != null && (playerInSight || wasHit))
                {
                    if (!startedFleeing)
                    {
                        StartCoroutine(EnemyFlee());
                    }
                }
                else if (!isWandering)
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

    private GameObject FindClosestStationWithTag(string tag)
    {
        GameObject[] stations = GameObject.FindGameObjectsWithTag(tag);
        GameObject closestStation = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject station in stations)
        {
            float distance = Vector3.Distance(transform.position, station.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestStation = station;
            }
        }

        return closestStation;
    }

    private bool IsAgentCloseToStation()
    {
        GameObject[] stations = GameObject.FindGameObjectsWithTag("EnemyRestockStation");

        foreach (GameObject station in stations)
        {
            float distance = Vector3.Distance(transform.position, station.transform.position);

            if (distance <= 2)
            {
                return true;
            }
        }

        return false;
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
            if (GunObject)
            {
                GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;
                GunObjectExitPoint.transform.LookAt(player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy)));
            }

            Vector3 direction = player.position - transform.position;
            direction.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), RotationSpeed * Time.deltaTime);
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

                Debug.DrawRay(EyesPosition.transform.position, direction, Color.red, 0.1f);

                bool playerVisible = true;

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        return true;
                    }
                    else if (!hit.collider.CompareTag("Enemy"))
                    {
                        playerVisible = false;
                    }
                }

                return playerVisible;
            }
        }

        return false;
    }


    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}