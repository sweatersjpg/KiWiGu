using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PistolGrunt : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter sfxEmitterAvailable;

    public enum EnemyState { Wandering, Seek, Panic, Shoot };
    [SerializeField] private EnemyState enemyState = EnemyState.Wandering;

    [Header("Drone Basic Settings")]
    [Range(0, 100)]
    [SerializeField] private float health;
    [Range(0, 100)]
    [SerializeField] private float shield;
    private bool isHoldingGun;
    private float currentHealth;
    private float currentShield;
    private bool isDead;

    [Space(10)]
    [Header("Enemy Movement Settings")]
    [SerializeField] private float wanderSpeed;
    [SerializeField] private float seekSpeed;
    [SerializeField] private float panicSpeed;
    [SerializeField] private float keepDistance;
    [SerializeField] private float wanderWaitTime;
    [SerializeField] private float wanderRadius;
    [SerializeField] private float rememberWaitTime;
    private NavMeshAgent agent;
    private float wanderTimer;
    private Vector3 initialPosition;

    [Space(10)]
    [Header("Enemy Seeking Settings")]
    [SerializeField] private Transform eyesPosition;
    [SerializeField] private float seekRange;
    private GameObject detectedPlayer;
    private float lastVisibleTime;
    private bool rememberPlayer;
    private bool loggedHidingObject;
    private GameObject loggedGameObject;
    private Vector3 hidingPos;

    [Space(10)]
    [Header("Enemy Panic Settings")]
    [SerializeField] private float hideTime;
    private bool isHiding;
    private float hideTimer;

    [Space(10)]
    [Header("Enemy Attack Settings")]
    [SerializeField] Transform BulletExitPoint;
    public bool isShooting;
    GunInfo info;

    private bool gotHit;

    private void Start()
    {
        HookTarget ht = GetComponentInChildren<HookTarget>();
        if (ht)
            isHoldingGun = true;

        agent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position;
    }

    private void Update()
    {
        StateManager();
        Wander();
        Seek();
        Panic();
        Shoot();
        RememberPlayer();
    }

    private void StateManager()
    {
        if (isShooting && !gotHit)
            return;

        if (gotHit || !isHoldingGun)
        {
            enemyState = EnemyState.Panic;
        }
        else if (!IsPlayerVisible() && !rememberPlayer)
        {
            enemyState = EnemyState.Wandering;
        }
        else if (IsPlayerVisible() || rememberPlayer)
        {
            enemyState = EnemyState.Seek;
        }
    }

    private void Wander()
    {
        if (enemyState == EnemyState.Wandering)
        {
            agent.speed = wanderSpeed;

            wanderTimer += Time.deltaTime;

            if (wanderTimer >= wanderWaitTime)
            {
                Vector3 newPos = RandomNavSphere(initialPosition, wanderRadius, -1);
                agent.SetDestination(newPos);
                wanderTimer = 0f;
            }
        }
    }

    private void Seek()
    {
        if (enemyState == EnemyState.Seek)
        {
            agent.speed = seekSpeed;

            Vector3 adjustedDestination = detectedPlayer.transform.position - (detectedPlayer.transform.position - transform.position).normalized * keepDistance;

            if (IsPlayerInRange() && !isShooting)
            {
                agent.SetDestination(transform.position);
                enemyState = EnemyState.Shoot;
            }
            else
            {
                agent.SetDestination(adjustedDestination);
            }
        }
    }

    private void Panic()
    {
        if (enemyState == EnemyState.Panic)
        {
            agent.speed = panicSpeed;

            if (isHiding)
            {
                hideTimer += Time.deltaTime;
                if (hideTimer >= hideTime)
                {
                    if (IsPlayerVisible())
                    {
                        isHiding = false;
                    }
                }
                return;
            }

            hideTimer = 0f;

            if (!loggedHidingObject)
            {
                loggedGameObject = FindRandomCoverNearby();
                loggedHidingObject = true;
            }

            if (!loggedGameObject)
                return;

            float distance = Vector3.Distance(transform.position, loggedGameObject.transform.position);

            if (distance <= 3f)
            {
                loggedHidingObject = false;
                isHiding = true;
            }
            else
            {
                MoveToOppositePoint(loggedGameObject.transform.position);
            }
        }
    }

    private void Shoot()
    {
        if (enemyState == EnemyState.Shoot)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    IEnumerator ShootRoutine()
    {
        isShooting = true;
        yield return new WaitForSeconds(EnemyShoot());
        isShooting = false;
    }

    private GameObject FindRandomCoverNearby()
    {
        GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");

        List<GameObject> nearbyCovers = new List<GameObject>();
        foreach (GameObject cover in coverObjects)
        {
            float distance = Vector3.Distance(transform.position, cover.transform.position);
            if (distance <= 50f)
            {
                nearbyCovers.Add(cover);
            }
        }

        if (nearbyCovers.Count > 0)
        {
            return nearbyCovers[Random.Range(0, nearbyCovers.Count)];
        }
        else
        {
            return null;
        }
    }

    private void MoveToOppositePoint(Vector3 targetPosition)
    {
        if (!detectedPlayer)
            return;

        Vector3 directionToPlayer = transform.position - detectedPlayer.transform.position;
        Vector3 oppositePoint = targetPosition + directionToPlayer.normalized;

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(oppositePoint);
            hidingPos = oppositePoint;
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

    private bool IsPlayerInRange()
    {
        float distanceTolerance = 0.5f;
        float distanceToDestination = Vector3.Distance(transform.position, detectedPlayer.transform.position);

        if (Mathf.Abs(distanceToDestination - keepDistance) <= distanceTolerance)
            return true;
        else
            return false;
    }

    private Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, layermask);

        return navHit.position;
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
        if(!isHoldingGun)
        {
            isShooting = false;
            return;
        }

        HookTarget gun = transform.GetComponentInChildren<HookTarget>();

        if (gun)
            info = gun.info;

        GameObject bullet = Instantiate(info.bulletPrefab, BulletExitPoint.transform.position, BulletExitPoint.transform.rotation);

        Vector3 direction = BulletExitPoint.transform.forward;
        direction += SpreadDirection(info.spread, 3);

        bullet.transform.position = BulletExitPoint.transform.position;
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
