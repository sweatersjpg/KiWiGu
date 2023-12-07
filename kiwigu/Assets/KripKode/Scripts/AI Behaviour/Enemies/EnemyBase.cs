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
    public bool hasAnimationDeath;

    [Header("Shared Variables")]
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentShield;
    [HideInInspector] public bool isHoldingGun;
    [HideInInspector] public bool isShooting;
    [HideInInspector] public bool isPlayerVisible;
    [HideInInspector] public bool isPlayerVisibleKnees;
    [HideInInspector] public bool detectedEnemy;
    [HideInInspector] public Vector3 playerPosition;
    [HideInInspector] public Vector3 playerEyesPosition;
    [HideInInspector] public Vector3 enemyPosition;
    [HideInInspector] public bool canFacePlayer = true;
    [HideInInspector] public GameObject gunObjectExitPoint;
    [HideInInspector] public Vector3 initialPosition;
    [HideInInspector] public bool gotHit;
    [HideInInspector] public bool isDead;


    protected virtual void Start()
    {
        SetTagBasedOnEnemyType();
        initialPosition = transform.position;

        if (enemyMainVariables.GunObject)
        {
            SetupInitialGun();
        }

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
        if (enemyTypeVariables.OffenseDrone || enemyTypeVariables.Small)
        {
            DetectPlayer();

        }
        else if (enemyTypeVariables.DefenseDrone)
        {
            DetectPlayer();
            DetectEnemy();
        }

        isPlayerVisible = CheckEyesVisibility();

        if (enemyMainVariables.hasKnees)
            isPlayerVisibleKnees = CheckKneesVisibility();
    }

    public virtual void TakeDamage(float bulletDamage)
    {
        if (isDead)
            return;

        if (enemyMainVariables.canBeHitAnim) HitBase();

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

    private void DetectPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 0) return;

        playerPosition = players
            .OrderBy(player => Vector3.Distance(transform.position, player.transform.position))
            .First()
            .transform.position;

        playerEyesPosition = players
            .OrderBy(player => Vector3.Distance(transform.position, player.transform.position))
            .First().GetComponentInChildren<Camera>().transform.position;
    }

    private void DetectEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
            return;

        detectedEnemy = enemies.Any(enemy => Vector3.Distance(transform.position, enemy.transform.position) < enemyMovementVariables.EnemyAwareDistance);

        enemyPosition = enemies
            .OrderBy(enemy => Vector3.Distance(transform.position, enemy.transform.position))
            .First()
            .transform.position;
    }

    protected virtual void HitBase()
    {
        gotHit = true;
        agent.enabled = false;
        StartCoroutine(PlayHitAnimation(enemyMainVariables.animator));
    }

    private IEnumerator PlayHitAnimation(Animator animator)
    {
        animator.SetInteger("HitIndex", Random.Range(0, 3));
        animator.SetTrigger("Hit");

        yield return new WaitForSeconds(1.5f);

        agent.enabled = true;
    }
    public void CheckStats()
    {
        if (currentHealth >= enemyMainVariables.MaxHealth && !isDead)
        {
            isDead = true;

            if (isHoldingGun)
            {
                HookTarget ht = GetComponentInChildren<HookTarget>();
                if (ht != null) ht.BeforeDestroy();

                isHoldingGun = false;
            }

            if (enemyMainVariables.explosionPrefab != null)
                Instantiate(enemyMainVariables.explosionPrefab, enemyMainVariables.BodyMesh.transform.position, Quaternion.identity);

            if (hasAnimationDeath)
            {
                enemyMainVariables.animator.SetTrigger("Dead");
                Destroy(gameObject, 5f);
            }
            else if (!hasAnimationDeath)
            {
                Destroy(gameObject);
            }
        }
    }

    public virtual bool CheckEyesVisibility()
    {
        Vector3 direction = playerEyesPosition - enemyMainVariables.EyesPosition.transform.position;

        int layersToIgnore = LayerMask.GetMask("Enemy", "CoverBehaviour");
        int finalLayerMask = ~layersToIgnore;


        if (Physics.Raycast(enemyMainVariables.EyesPosition.transform.position, direction, out RaycastHit hit, enemyMovementVariables.EnemyAwareDistance, finalLayerMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.DrawRay(enemyMainVariables.EyesPosition.transform.position, direction.normalized * hit.distance, Color.green, 0.1f);
                return true;
            }
            else
            {
                Debug.DrawRay(enemyMainVariables.EyesPosition.transform.position, direction.normalized * hit.distance, Color.red, 0.1f);
                return false;
            }
        }

        return false;
    }

    public virtual bool CheckKneesVisibility()
    {
        Vector3 direction = playerPosition - enemyMainVariables.KneesPosition.transform.position + new Vector3(0, 0.5f, 0);

        int layersToIgnore = LayerMask.GetMask("Enemy", "CoverBehaviour");
        int finalLayerMask = ~layersToIgnore;

        if (Physics.Raycast(enemyMainVariables.KneesPosition.transform.position, direction, out RaycastHit hit, enemyMovementVariables.EnemyAwareDistance, finalLayerMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.DrawRay(enemyMainVariables.KneesPosition.transform.position, direction.normalized * hit.distance, Color.green, 0.1f);
                return true;
            }
            else
            {
                Debug.DrawRay(enemyMainVariables.KneesPosition.transform.position, direction.normalized * hit.distance, Color.red, 0.1f);
                return false;
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
        public bool canBeHitAnim;
        public Animator animator;
        [Header("Enemy Main Variables")]
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
        public bool hasKnees;
        public GameObject KneesPosition;
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
        [Tooltip("The movement speed of the enemy.")]
        public int MovementSpeed = 5;
        [Range(3, 10)]
        public float WanderRadius = 7;
        [Range(5, 10)]
        [Tooltip("The distance at which the enemy avoids the player.")]
        public int AvoidPlayerDistance = 7;
        [Range(100, 1000)]
        [Tooltip("The rotation speed of the enemy.")]
        public int RotationSpeed = 180;
        [Range(10, 100)]
        [Tooltip("The distance at which the enemy becomes aware of the player.")]
        public int EnemyAwareDistance = 20;
        [Range(1, 10)]
        public int IdleTime = 2;
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