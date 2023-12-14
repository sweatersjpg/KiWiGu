using FMODUnity;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class DefenseDrone : MonoBehaviour
{
    public enum DroneState { Wandering, Defending };

    [SerializeField] private StudioEventEmitter sfxEmitterAvailable;
    [SerializeField] private float health = 100f;
    [SerializeField] private float shield = 100f;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private DroneState droneState = DroneState.Wandering;

    [Header("Drone Basic Settings")]
    [SerializeField] private bool isHoldingGun;

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
    [SerializeField] private Transform bulletExitPoint;
    [SerializeField] private float defendCooldown;
    [SerializeField] private string defendTag;

    [Space(10)]
    [Header("Drone Body Mesh")]
    [SerializeField] private GameObject droneBody;
    [SerializeField] private float floatValue;
    [SerializeField] private float floatSpeed;

    private NavMeshAgent agent;
    private Vector3 initialPosition;
    private float wanderTimer;
    private float initialDroneBodyPositionY;
    private Vector3 droneFloatPosition;
    private bool isMovingUp;

    private float currentHealth;
    private float currentShield;
    private bool isDead;
    public bool detectedEnemy;
    private Vector3 enemyPosition;
    private Vector3 behindPos;
    private GameObject detectedPlayer;
    private bool isDefending;
    private bool isShooting;
    private float lastVisibleTime;
    private bool rememberPlayer;
    private float timeSinceLastShot;

    private void Start()
    {
        HookTarget ht = GetComponentInChildren<HookTarget>();
        isHoldingGun = ht != null;

        agent = GetComponent<NavMeshAgent>();
        initialDroneBodyPositionY = agent.height;

        droneFloatPosition = new Vector3(droneBody.transform.position.x, initialDroneBodyPositionY, droneBody.transform.position.z);
        droneBody.transform.position = droneFloatPosition;

        initialPosition = transform.position;
    }

    private void Update()
    {
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
            if(timeSinceLastShot > defendCooldown)
            {
                EnemyShoot();
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
        Collider[] hitColliders = Physics.OverlapSphere(eyesPosition.position, seekRange);
        int layerMask = LayerMask.GetMask("Enemy");
        int layerMask2 = LayerMask.GetMask("HookTarget");
        int combinedLayerMask = layerMask | layerMask2;

        foreach (Collider hitCollider in hitColliders)
        {
            RaycastHit hit;
            if (Physics.Raycast(eyesPosition.position, hitCollider.transform.position - eyesPosition.position - new Vector3(0, -1, 0), out hit, seekRange, ~combinedLayerMask))
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
        float targetHeight = initialDroneBodyPositionY + direction;

        if (isMovingUp && droneBody.transform.position.y >= targetHeight)
        {
            isMovingUp = false;
        }
        else if (!isMovingUp && droneBody.transform.position.y <= targetHeight)
        {
            isMovingUp = true;
        }

        float newY = Mathf.MoveTowards(droneBody.transform.position.y, targetHeight, floatSpeed * Time.deltaTime);

        droneFloatPosition = new Vector3(droneBody.transform.position.x, newY, droneBody.transform.position.z);
        droneBody.transform.position = droneFloatPosition;
    }

    private bool IsPlayerWithinRange()
    {
        float distanceTolerance = 0.5f;
        float distanceToDestination = Vector3.Distance(transform.position, detectedPlayer.transform.position);

        if (distanceToDestination < (keepDistance + distanceTolerance))
            return true;
        else
            return false;
    }

    public virtual void TakeDamage(float bulletDamage)
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

    private float EnemyShoot()
    {
        if (!isHoldingGun || !IsPlayerVisible())
            return 0;

        HookTarget gun = transform.GetComponentInChildren<HookTarget>();
        if (gun == null)
        {
            isHoldingGun = false;
            return 0;
        }
        GunInfo info = gun.info;

        float burst = info.burstSize;
        if (info.fullAuto) burst = info.autoRate;

        for (int j = 0; j < burst; j++)
        {
            sfxEmitterAvailable.SetParameter("Charge", 0.5f);
            sfxEmitterAvailable.Play();

            for (int i = 0; i < info.projectiles; i++) Invoke(nameof(SpawnBullet), j * 1 / info.autoRate);
        }

        return burst * 1 / info.autoRate;
    }

    private void SpawnBullet()
    {
        HookTarget gun = transform.GetComponentInChildren<HookTarget>();
        GunInfo info = gun.info;

        GameObject bullet = Instantiate(info.bulletPrefab, bulletExitPoint.transform.position, bulletExitPoint.transform.rotation);

        Vector3 direction = bulletExitPoint.transform.forward;
        direction += SpreadDirection(info.spread, 3);

        bullet.transform.position = bulletExitPoint.transform.position;
        bullet.transform.rotation = Quaternion.LookRotation(direction.normalized);

        Bullet b = bullet.GetComponent<Bullet>();
        b.speed = info.bulletSpeed;
        b.gravity = info.bulletGravity;
        b.ignoreMask = ~LayerMask.GetMask("GunHand", "HookTarget", "Enemy");
        b.trackTarget = false;
        b.fromEnemy = true;
        b.bulletDamage = info.damage;
        b.charge = 0.5f;
    }

    private Vector3 SpreadDirection(float spread, int rolls)
    {
        Vector3 offset = Vector3.zero;
        for (int i = 0; i < rolls; i++)
            offset += Random.onUnitSphere * spread;
        return offset / rolls;
    }
}
