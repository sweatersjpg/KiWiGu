using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class DefenseDrone : MonoBehaviour
{
    public enum DroneState { Wandering, Defending };

    [Header("Drone Basic Settings")]
    [Range(0, 100)]
    [SerializeField] private float health;
    [Range(0, 100)]
    [SerializeField] private float shield;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private DroneState droneState = DroneState.Wandering;
    bool isHoldingGun;

    [Space(10)]
    [Header("Drone Seeking Settings")]
    [SerializeField] private Transform eyesPosition;
    [SerializeField] private float seekRange;

    [Space(10)]
    [Header("Drone Movement Settings")]
    [SerializeField] private float wanderSpeed;
    [SerializeField] private float seekSpeed;
    [SerializeField] private float wanderRadius;
    [SerializeField] private float keepDistance;
    [SerializeField] private float awareDistance;
    [SerializeField] private float wanderWaitTime;
    [SerializeField] private float rememberWaitTime;

    [Space(10)]
    [Header("Drone Defense Settings")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform bulletExitPoint;
    [SerializeField] private float defendCooldown;
    [SerializeField] private string defendTag;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private int numGrenades = 12;

    [Space(10)]
    [Header("Drone Body Mesh")]
    [SerializeField] private GameObject droneBody;
    [SerializeField] private float floatValue;
    [SerializeField] private float floatSpeed;

    private NavMeshAgent agent;
    private Vector3 initialPosition;
    private float wanderTimer;
    private float initialDroneBodyPositionY;
    private Vector3 initialLocalPosition;
    private Vector3 droneFloatPosition;
    private bool isMovingUp;

    private float currentHealth;
    private float currentShield;
    private bool isDead;
    public bool detectedEnemy;
    private Vector3 enemyPosition;
    private GameObject detectedPlayer;
    private bool isDefending;
    private float lastVisibleTime;
    private bool rememberPlayer;
    private float timeSinceLastShot;

    private void Start()
    {
        HookTarget ht = GetComponentInChildren<HookTarget>();
        isHoldingGun = ht != null;

        agent = GetComponent<NavMeshAgent>();
        initialDroneBodyPositionY = agent.height;

        droneFloatPosition = new Vector3(droneBody.transform.localPosition.x, initialDroneBodyPositionY, droneBody.transform.localPosition.z);
        droneBody.transform.localPosition = droneFloatPosition;

        initialLocalPosition = droneBody.transform.localPosition;

        initialPosition = transform.position;
    }

    private void Update()
    {
        // add to update functions to pause them        
        if (PauseSystem.paused) return;

        if (isDead)
        {
            StopAllCoroutines();
            return;
        }

        StateManager();
        DetectEnemy();
        RememberPlayer();
        FacePlayer();
        RoamAround();
        Defending();
        IsPlayerVisible();
    }

    private void StateManager()
    {
        if (detectedEnemy)
        {
            droneState = DroneState.Defending;
        }
        else
        {
            droneState = DroneState.Wandering;
        }
    }

    private void RoamAround()
    {
        if (droneState != DroneState.Wandering)
            return;

        if (isMovingUp)
        {
            MoveBody(floatValue);
        }
        else
        {
            MoveBody(-floatValue);
        }

        agent.speed = wanderSpeed;
        MoveBody(floatValue);
        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderWaitTime)
        {
            Vector3 newPos = RandomNavSphere(initialPosition, wanderRadius, -1);
            agent.SetDestination(newPos);
            wanderTimer = 0f;
        }
    }

    private void Defending()
    {
        if (droneState != DroneState.Defending)
            return;

        agent.speed = seekSpeed;

        if (isMovingUp)
        {
            MoveBody(floatValue / 2);
        }
        else
        {
            MoveBody(-floatValue / 2);
        }

        Vector3 adjustedDestination = enemyPosition - (enemyPosition - transform.position).normalized * keepDistance;
        agent.SetDestination(adjustedDestination);

        if (IsPlayerWithinRange())
        {
            // Debug.Log("within range");
            if(timeSinceLastShot > defendCooldown)
            {
                StartCoroutine(SpawnGrenades());
                timeSinceLastShot = 0;
                isDefending = false;
                detectedEnemy = false;
            }
            else
            {
                isDefending = true;
                timeSinceLastShot += Time.deltaTime;
            }
        }
    }

    private void FacePlayer()
    {
        if (detectedEnemy && detectedPlayer != null && rememberPlayer)
        {
            Vector3 direction = detectedPlayer.transform.position - transform.position;
            Vector3 localDirection = transform.InverseTransformDirection(direction);
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(localDirection.x, localDirection.y, localDirection.z));
            droneBody.transform.localRotation = Quaternion.Slerp(droneBody.transform.localRotation, lookRotation, Time.deltaTime * 20f);
        }
        else if (!isDefending)
        {
            droneBody.transform.localRotation = Quaternion.Slerp(droneBody.transform.localRotation, Quaternion.identity, Time.deltaTime * 20f);
        }
    }

    private void RememberPlayer()
    {
        if (!detectedPlayer)
            return;

        if (IsPlayerVisible())
        {
            lastVisibleTime = Time.time;
            rememberPlayer = true;
        }
        else
        {
            if (Time.time - lastVisibleTime >= rememberWaitTime)
            {
                rememberPlayer = false;
            }
            else
            {
                rememberPlayer = true;
            }
        }
    }

    private bool IsPlayerVisible()
    {
        Collider[] hitColliders = Physics.OverlapSphere(eyesPosition.position, seekRange, LayerMask.GetMask("Player"));
        int layerMask = LayerMask.GetMask("Enemy");
        int layerMask2 = LayerMask.GetMask("HookTarget");
        int layerMask3 = LayerMask.GetMask("Shield");
        int layerMask4 = LayerMask.GetMask("GunHand");
        int layerMask5 = LayerMask.GetMask("EnergyWall");

        int combinedLayerMask = layerMask | layerMask2 | layerMask3 | layerMask4 | layerMask5;


        foreach (Collider hitCollider in hitColliders)
        {
            Vector3 rayDirection = hitCollider.transform.position - eyesPosition.position - new Vector3(0, -1, 0);

            // Draw debug ray
            Debug.DrawRay(eyesPosition.position, rayDirection, Color.green);

            RaycastHit hit;
            if (Physics.Raycast(eyesPosition.position, rayDirection, out hit, seekRange, ~combinedLayerMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    detectedPlayer = hit.collider.gameObject;
                    return true;
                }
            }
        }

        return false;
    }


    private void DetectEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(defendTag);

        if (enemies.Length == 0)
            return;

        detectedEnemy = enemies.Any(enemy => Vector3.Distance(transform.position, enemy.transform.position) < seekRange);

        enemyPosition = enemies
            .OrderBy(enemy => Vector3.Distance(transform.position, enemy.transform.position))
            .First()
            .transform.position;
    }


    private Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, layermask);

        return navHit.position;
    }

    private void MoveBody(float direction)
    {
        float targetHeight = initialLocalPosition.y + direction;

        if (isMovingUp && droneBody.transform.localPosition.y >= targetHeight)
        {
            isMovingUp = false;
        }
        else if (!isMovingUp && droneBody.transform.localPosition.y <= targetHeight)
        {
            isMovingUp = true;
        }

        float newY = Mathf.MoveTowards(droneBody.transform.localPosition.y, targetHeight, floatSpeed * Time.deltaTime);

        droneFloatPosition = new Vector3(droneBody.transform.localPosition.x, newY, droneBody.transform.localPosition.z);
        droneBody.transform.localPosition = droneFloatPosition;
    }

    private bool IsPlayerWithinRange()
    {
        if(detectedPlayer)
        {
            float distanceTolerance = 0.5f;
            float distanceToDestination = Vector3.Distance(transform.position, detectedPlayer.transform.position);

            if (distanceToDestination < (awareDistance + distanceTolerance))
                return true;
            else
                return false;
        }
        else
        {
            return false;
        }
    }

    public virtual void TakeDamage(float bulletDamage, bool isHeadShot)
    {
        if (isDead)
            return;

        if (currentShield < shield)
        {
            currentShield = Mathf.Min(currentShield + bulletDamage, shield);
        }
        else if (currentHealth < health)
        {
            currentHealth = Mathf.Min(currentHealth + bulletDamage, health);
        }

        CheckStats();
    }

    public void CheckStats()
    {
        if (currentHealth >= health && !isDead)
        {
            isDead = true;

            if (isHoldingGun)
            {
                HookTarget ht = GetComponentInChildren<HookTarget>();
                if (ht != null) ht.BeforeDestroy();

                isHoldingGun = false;
            }

            if (explosionPrefab != null)
                Instantiate(explosionPrefab, droneBody.transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }

    private IEnumerator SpawnGrenades()
    {
        float angleIncrement = 360f / numGrenades;

        // Get the initial spawn direction based on spawner's rotation
        Vector3 initialSpawnDirection = (detectedPlayer.transform.position - transform.position).normalized;

        for (int i = 0; i < numGrenades; i++)
        {
            SpawnGrenadeWithForce(initialSpawnDirection);
            initialSpawnDirection = Quaternion.Euler(0, angleIncrement, 0) * initialSpawnDirection;

            yield return new WaitForSeconds(1f / rotationSpeed);
        }
    }

    private void SpawnGrenadeWithForce(Vector3 spawnDirection)
    {
        Vector3 spawnPosition = bulletExitPoint.position + spawnDirection * spawnRadius;

        GameObject grenade = Instantiate(grenadePrefab, spawnPosition, Quaternion.identity);

        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
        grenadeRb.AddForce(spawnDirection * 2, ForceMode.Impulse);
    }

}