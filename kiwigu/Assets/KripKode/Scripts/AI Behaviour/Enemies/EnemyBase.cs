using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    public EnemyMainVariables enemyMainVariables;
    public EnemyMovementVariables enemyMovementVariables;
    public EnemyGunStats enemyGunStats;
    public EnemyTypeVariables enemyTypeVariables;
    public HitboxScript hitBoxScript;

    [Header("Shared Variables")]
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentShield;
    [HideInInspector] public bool isHoldingGun;
    [HideInInspector] public bool wasHit;
    [HideInInspector] public bool isWandering;
    [HideInInspector] public bool isShooting;
    [HideInInspector] public bool playerInSight;
    [HideInInspector] public bool detectedPlayer;
    [HideInInspector] public bool detectedEnemy;
    [HideInInspector] public Vector3 playerPosition;
    [HideInInspector] public Vector3 enemyPosition;

    private Vector3 wanderTarget;
    private Vector3 initialPosition;

    private GameObject initialGunObject;
    private bool startedFleeing;

    protected virtual void Start()
    {
        if (hitBoxScript.CheckIfHitboxScript)
            return;

        SetTagBasedOnEnemyType();

        if (enemyMainVariables.spawnWithGun && enemyMainVariables.GunObject)
        {
            SetupInitialGun();
        }

        initialPosition = transform.position;

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = enemyMovementVariables.MovementSpeed;
            agent.angularSpeed = enemyMovementVariables.RotationSpeed;
        }
    }

    private void SetTagBasedOnEnemyType()
    {
        if (enemyTypeVariables.Small || enemyTypeVariables.Medium)
            gameObject.tag = "Enemy";
        else if (enemyTypeVariables.DefenseDrone)
            gameObject.tag = "DroneDefense";
        else if (enemyTypeVariables.OffenseDrone)
            gameObject.tag = "DroneEnemy";
    }

    private void SetupInitialGun()
    {
        enemyMainVariables.GunObject = Instantiate(enemyMainVariables.GunObject, enemyMainVariables.HandPosition.transform);
        isHoldingGun = true;
        initialGunObject = enemyMainVariables.GunObject;
    }

    public void EnemyBehaviour()
    {
        if (enemyTypeVariables.DefenseDrone || enemyTypeVariables.OffenseDrone)
            return;

        if (enemyMainVariables.GunObject)
        {
            HandleGunLogic();
        }
        else
        {
            HandleNoGunLogic();
        }
    }

    private void HandleGunLogic()
    {
        if (IsAgentCloseToStation())
        {
            enemyMainVariables.GunObject = Instantiate(initialGunObject, enemyMainVariables.HandPosition.transform);
            isHoldingGun = true;
        }
        else
        {
            HandleGunSeeking();
        }
    }

    private void HandleGunSeeking()
    {
        if (enemyMainVariables.canSeekGun)
        {
            GameObject closestStation = FindClosestStationWithTag("EnemyRestockStation");

            if (closestStation != null)
            {
                agent.SetDestination(closestStation.transform.position);
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

    private void HandleNoGunLogic()
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

    protected virtual void Update()
    {
        if (hitBoxScript.CheckIfHitboxScript)
            return;

        if (enemyTypeVariables.OffenseDrone || enemyTypeVariables.Small || enemyTypeVariables.Medium)
            DetectPlayer();
        else if (enemyTypeVariables.DefenseDrone)
        {
            DetectPlayer();
            DetectEnemy();
        }

        if (isHoldingGun && !isWandering)
        {
            StartCoroutine(Wander());
        }
    }

    private void DetectPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        detectedPlayer = players.Any(player => Vector3.Distance(transform.position, player.transform.position) < enemyMovementVariables.EnemyAwareDistance);

        playerPosition = players
            .OrderBy(player => Vector3.Distance(transform.position, player.transform.position))
            .First()
            .transform.position;
    }

    private void DetectEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        detectedEnemy = enemies.Any(enemy => Vector3.Distance(transform.position, enemy.transform.position) < enemyMovementVariables.EnemyAwareDistance);

        enemyPosition = enemies
            .OrderBy(enemy => Vector3.Distance(transform.position, enemy.transform.position))
            .First()
            .transform.position;
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
        Vector3 awayFromPlayer = transform.position - playerPosition;
        awayFromPlayer.Normalize();

        Vector3 fleeDestination = transform.position + awayFromPlayer * enemyMovementVariables.FleeDistance;

        float randomOffsetX = Mathf.PerlinNoise(Time.time, 0) * 2 - 1;
        float randomOffsetZ = Mathf.PerlinNoise(0, Time.time) * 2 - 1;
        Vector3 randomOffset = new Vector3(randomOffsetX, 0f, randomOffsetZ) * enemyMovementVariables.FleeMovementVariation;
        fleeDestination += randomOffset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeDestination, out hit, enemyMovementVariables.FleeDistance, NavMesh.AllAreas))
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

    public IEnumerator Wander()
    {
        wanderTarget = RandomWanderPoint();
        agent.SetDestination(wanderTarget);
        isWandering = true;

        if (agent.isOnNavMesh)
            yield return new WaitUntil(() => agent.remainingDistance <= 0.5f);

        yield return new WaitForSeconds(Random.Range(enemyMovementVariables.WanderIdleVariation - 1, enemyMovementVariables.WanderIdleVariation + 2));
        isWandering = false;
    }

    public Vector3 RandomWanderPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * enemyMovementVariables.WanderRadius;
        randomDirection += initialPosition;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, enemyMovementVariables.WanderRadius, NavMesh.AllAreas);

        return hit.position;
    }

    public virtual void TakeDamage(float bulletDamage)
    {
        if (hitBoxScript.CheckIfHitboxScript && hitBoxScript.enemyBehaviour)
        {
            hitBoxScript.enemyBehaviour.wasHit = true;
            if (hitBoxScript.enemyBehaviour.currentShield < hitBoxScript.enemyBehaviour.enemyMainVariables.MaxShield)
            {
                hitBoxScript.enemyBehaviour.currentShield = Mathf.Min(hitBoxScript.enemyBehaviour.currentShield + bulletDamage, hitBoxScript.enemyBehaviour.enemyMainVariables.MaxShield);
            }
            else if (hitBoxScript.enemyBehaviour.currentHealth < hitBoxScript.enemyBehaviour.enemyMainVariables.MaxHealth)
            {
                hitBoxScript.enemyBehaviour.currentHealth = Mathf.Min(hitBoxScript.enemyBehaviour.currentHealth + bulletDamage, hitBoxScript.enemyBehaviour.enemyMainVariables.MaxHealth);
            }

            hitBoxScript.enemyBehaviour.CheckStats();
        }
    }

    public void CheckStats()
    {
        if (currentHealth >= enemyMainVariables.MaxHealth)
        {
            if (isHoldingGun)
            {
                HookTarget ht = GetComponentInChildren<HookTarget>();
                if(ht != null) ht.BeforeDestroy();

                isHoldingGun = false;
            }

            Destroy(gameObject);
        }
    }

    public virtual bool CheckPlayerVisibility()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, enemyMovementVariables.EnemyAwareDistance);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                Vector3 direction = playerPosition - enemyMainVariables.EyesPosition.transform.position + new Vector3(0, 0.5f, 0);
                RaycastHit[] hits = Physics.RaycastAll(enemyMainVariables.EyesPosition.transform.position, direction, enemyMovementVariables.EnemyAwareDistance);

                Debug.DrawRay(enemyMainVariables.EyesPosition.transform.position, direction, Color.red, 0.1f);

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


    [System.Serializable]
    public class EnemyMainVariables
    {
        [Header("Enemy Main Variables")]
        [Tooltip("Whether the enemy can seek a gun.")]
        public bool canSeekGun;
        [Tooltip("Whether the enemy spawns with a gun.")]
        public bool spawnWithGun;
        [Range(10, 100)]
        [Tooltip("The maximum health of the enemy.")]
        public int MaxHealth = 100;
        [Range(0, 100)]
        [Tooltip("The maximum shield of the enemy.")]
        public int MaxShield = 100;
        [Tooltip("The GameObject representing the enemy's gun.")]
        public GameObject GunObject;
        [Tooltip("The GameObject representing the position of the enemy's eyes.")]
        public GameObject EyesPosition;
        [Tooltip("The GameObject representing the enemy's body mesh.")]
        public GameObject BodyMesh;
        [Tooltip("Make sure Hand Transform is attached as a child of the Body Object!")]
        public GameObject HandPosition;
    }

    [System.Serializable]
    public class EnemyMovementVariables
    {
        [Header("Enemy Movement")]
        [Range(1, 15)]
        [Tooltip("The distance the enemy flees when in danger.")]
        public int FleeDistance = 5;
        [Range(1, 15)]
        [Tooltip("Variation in the movement during fleeing.")]
        public int FleeMovementVariation = 4;
        [Range(1, 15)]
        [Tooltip("The movement speed of the enemy.")]
        public int MovementSpeed = 5;
        [Range(5, 10)]
        [Tooltip("The distance at which the enemy avoids the player.")]
        public int AvoidPlayerDistance = 7;
        [Range(100, 200)]
        [Tooltip("The rotation speed of the enemy.")]
        public int RotationSpeed = 180;
        [Range(10, 25)]
        [Tooltip("The distance at which the enemy becomes aware of the player.")]
        public int EnemyAwareDistance = 20;
        [Range(5, 20)]
        [Tooltip("The radius for wandering.")]
        public int WanderRadius = 8;
        [Range(2, 8)]
        [Tooltip("Variation in idle time during wandering.")]
        public float WanderIdleVariation;
        [Range(1, 10)]
        [Tooltip("Idle time for a drone.")]
        public int DroneIdleTime = 2;
    }

    [System.Serializable]
    public class EnemyGunStats
    {
        [Header("Enemy Gun Stats")]
        [Range(1, 10)]
        [Tooltip("The fire rate of the enemy.")]
        public float EnemyFireRate = 1.0f;
        [Range(0, 10)]
        [Tooltip("Inaccuracy of the enemy's gun.")]
        public int GunInaccuracy = 5;
    }

    [System.Serializable]
    public class EnemyTypeVariables
    {
        [Header("Enemy Type")]
        public bool Small;
        public bool Medium;
        public bool DefenseDrone;
        public bool OffenseDrone;
    }

    [System.Serializable]
    public class HitboxScript
    {
        [Header("Hitbox")]
        public bool CheckIfHitboxScript;
        public EnemyBehaviour enemyBehaviour;
    }
}