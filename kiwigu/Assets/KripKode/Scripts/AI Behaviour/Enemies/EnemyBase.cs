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
    [HideInInspector] public bool canFacePlayer = true;
    [HideInInspector] public GameObject gunObjectExitPoint;
    [HideInInspector] public bool startedFleeing;

    private Vector3 wanderTarget;
    private Vector3 initialPosition;

    protected virtual void Start()
    {
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
    }

    protected virtual void Update()
    {
        if (enemyTypeVariables.OffenseDrone || enemyTypeVariables.Small || enemyTypeVariables.Medium)
            DetectPlayer();
        else if (enemyTypeVariables.DefenseDrone)
        {
            DetectPlayer();
            DetectEnemy();
        }

        if (!isWandering && !isShooting && !detectedPlayer)
        {
            StartCoroutine(Wander());
        }
    }

    private void DetectPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 0) return;

        detectedPlayer = players.Any(player => Vector3.Distance(transform.position, player.transform.position) < enemyMovementVariables.EnemyAwareDistance);

        playerPosition = players
            .OrderBy(player => Vector3.Distance(transform.position, player.transform.position))
            .First()
            .transform.position;
    }

    private void DetectEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("DroneEnemy");

        if (enemies.Length == 0)
            return;

        detectedEnemy = enemies.Any(enemy => Vector3.Distance(transform.position, enemy.transform.position) < enemyMovementVariables.EnemyAwareDistance);

        enemyPosition = enemies
            .OrderBy(enemy => Vector3.Distance(transform.position, enemy.transform.position))
            .First()
            .transform.position;
    }

    public IEnumerator Wander()
    {
        wanderTarget = RandomWanderPoint();
        agent.SetDestination(wanderTarget);
        isWandering = true;

        if (agent.isOnNavMesh) yield return new WaitUntil(() => agent.remainingDistance <= 0.5f);

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
        wasHit = true;
        if (currentShield < enemyMainVariables.MaxShield)
        {
            currentShield = Mathf.Min(currentShield + bulletDamage, enemyMainVariables.MaxShield);
        }
        else if (currentHealth < enemyMainVariables.MaxHealth)
        {
            currentHealth = Mathf.Min(currentHealth + bulletDamage, enemyMainVariables.MaxHealth);
        }

        CheckStats();
    }

    public void CheckStats()
    {
        if (currentHealth >= enemyMainVariables.MaxHealth)
        {
            if (isHoldingGun)
            {
                HookTarget ht = GetComponentInChildren<HookTarget>();
                if (ht != null) ht.BeforeDestroy();

                isHoldingGun = false;
            }

            Instantiate(enemyMainVariables.explosionPrefab, enemyMainVariables.BodyMesh.transform.position, Quaternion.identity);
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
                RaycastHit[] hits = Physics.RaycastAll(enemyMainVariables.EyesPosition.transform.position, direction, enemyMovementVariables.EnemyAwareDistance, ~LayerMask.GetMask("Enemy"));

                Debug.DrawRay(enemyMainVariables.EyesPosition.transform.position, direction, Color.red, 0.1f);

                bool playerVisible = true;

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        return true;
                    }
                }

                return playerVisible;
            }
        }

        return false;
    }

    private void OnDestroy()
    {
        if (Camera.main != null) Camera.main.GetComponent<Music>().Violence = 0;
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
        public GameObject explosionPrefab;
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
        [SerializeField] public float gunExitPointRotationSpeed = 180f;
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
}