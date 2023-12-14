using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class OffenseDrone : MonoBehaviour
{
    public enum DroneState { Wandering, Seeking, Attacking };
    [SerializeField] private StudioEventEmitter sfxEmitterAvailable;

    [Header("Drone Basic Settings")]
    [Range(0, 100)]
    [SerializeField] private float health;
    [Range(0, 100)]
    [SerializeField] private float shield;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private DroneState droneState = DroneState.Wandering;

    private float currentHealth;
    private float currentShield;
    private bool isDead;
    private bool isHoldingGun;

    [Space(10)]
    [Header("Drone Seeking Settings")]
    [SerializeField] private Transform eyesPosition;
    [SerializeField] private float seekRange;
    private GameObject detectedPlayer;

    [Space(10)]
    [Header("Drone Movement Settings")]
    [SerializeField] private float wanderSpeed;
    [SerializeField] private float seekSpeed;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float wanderRadius;
    [SerializeField] private float keepDistance;
    [SerializeField] private float wanderWaitTime;
    [SerializeField] private float rememberWaitTime;
    private NavMeshAgent agent;
    private Vector3 initialPosition;
    private float wanderTimer;
    private float lastVisibleTime;
    private bool rememberPlayer;

    [Space(10)]
    [Header("Drone Attack Settings")]
    [SerializeField] Transform BulletExitPoint;
    [SerializeField] private float attackRange;
    [SerializeField] private float windUpTime;
    [SerializeField] private float patternTime;
    [SerializeField] private float attackCooldown;
    private bool isAttacking;
    private bool isShooting;

    [Space(10)]
    [Header("Drone Body Mesh")]
    [SerializeField] private GameObject DroneBody;
    [SerializeField] private float floatValue;
    [SerializeField] private float floatSpeed;
    private bool canFacePlayer = true;
    private float initialDroneBodyPositionY;
    private Vector3 droneFloatPosition;
    private bool isMovingUp;

    private void Start()
    {
        HookTarget ht = GetComponentInChildren<HookTarget>();
        if (ht)
            isHoldingGun = true;

        agent = GetComponent<NavMeshAgent>();
        initialDroneBodyPositionY = agent.height;

        droneFloatPosition = new Vector3(DroneBody.transform.position.x, initialDroneBodyPositionY, DroneBody.transform.position.z);
        DroneBody.transform.position = droneFloatPosition;

        DroneBody.transform.position = droneFloatPosition;
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
        RoamAround();
        Seeking();
        Attack();
        RememberPlayer();
        FacePlayer();
    }

    private void StateManager()
    {
        if (droneState == DroneState.Attacking)
            return;

        switch (IsPlayerVisible() || rememberPlayer)
        {
            case true:
                droneState = DroneState.Seeking;
                break;
            case false:
                droneState = DroneState.Wandering;
                break;
        }
    }

    private void RoamAround()
    {
        if (droneState == DroneState.Wandering)
        {
            agent.speed = wanderSpeed;

            if (isMovingUp)
            {
                MoveBody(floatValue);
            }
            else
            {
                MoveBody(-floatValue);
            }

            wanderTimer += Time.deltaTime;

            if (wanderTimer >= wanderWaitTime)
            {
                Vector3 newPos = RandomNavSphere(initialPosition, wanderRadius, -1);
                agent.SetDestination(newPos);
                wanderTimer = 0f;
            }
        }
    }

    private void Seeking()
    {
        if (droneState == DroneState.Seeking)
        {
            agent.speed = seekSpeed;

            if (isMovingUp)
            {
                MoveBody(floatValue / 2);
            }
            else
            {
                MoveBody(-floatValue / 2);
            }

            Vector3 adjustedDestination = detectedPlayer.transform.position - (detectedPlayer.transform.position - transform.position).normalized * keepDistance;

            if (IsPlayerWithinRange())
            {
                agent.SetDestination(transform.position);
                droneState = DroneState.Attacking;
            }
            else
            {
                agent.SetDestination(adjustedDestination);
            }
        }
    }

    private void Attack()
    {
        if (droneState == DroneState.Attacking)
        {
            if(isShooting)
            {
                if (isMovingUp)
                {
                    MoveBody(floatValue * 2);
                }
                else
                {
                    MoveBody(-floatValue * 2);
                }
            }

            agent.speed = attackSpeed;

            if (!isAttacking)
            {   
                int randomAttack = Random.Range(1,3);
                StartCoroutine(AttackRoutine(randomAttack));
            }
        }
    }

    IEnumerator AttackRoutine(int pattern)
    {
        isAttacking = true;
        yield return new WaitForSeconds(windUpTime);

        if (pattern == 1)
        {
            isShooting = true;
            for (int i = 0; i < 3; i++)
            {
                Vector3 randomPoint = RandomNavSphere(detectedPlayer.transform.position, attackRange, -1);
                agent.SetDestination(randomPoint);
                yield return new WaitUntil(() => !agent.pathPending && agent.isOnNavMesh && agent.remainingDistance < 0.1f);
                canFacePlayer = false;
                yield return new WaitForSeconds(EnemyShoot());
                canFacePlayer = true;
            }
            isShooting = false;
        }
        else if (pattern == 2)
        {
            isShooting = true;

            for (int i = 0; i < 3; i++)
            {
                Vector3 randomPoint = RandomNavSphere(detectedPlayer.transform.position, attackRange, -1);
                agent.SetDestination(randomPoint);
                yield return new WaitUntil(() => !agent.pathPending && agent.isOnNavMesh && agent.remainingDistance < 0.1f);
            }
            canFacePlayer = false;
            yield return new WaitForSeconds(EnemyShoot());
            canFacePlayer = true;
            isShooting = false;
        }

        yield return new WaitForSeconds(attackCooldown);

        droneState = DroneState.Seeking;
        isAttacking = false;
    }
    private void FacePlayer()
    {
        if (canFacePlayer && detectedPlayer != null && rememberPlayer)
        {
            Vector3 direction = detectedPlayer.transform.position - transform.position;
            Vector3 localDirection = transform.InverseTransformDirection(direction);
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(localDirection.x, localDirection.y, localDirection.z));

            DroneBody.transform.localRotation = Quaternion.Slerp(DroneBody.transform.localRotation, lookRotation, Time.deltaTime * 20f); ;
        }
        else if (!isAttacking)
        {
            DroneBody.transform.localRotation = Quaternion.Slerp(DroneBody.transform.localRotation, Quaternion.identity, Time.deltaTime * 20f);
        }
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

        if (isMovingUp && DroneBody.transform.position.y >= targetHeight)
        {
            isMovingUp = false;
        }
        else if (!isMovingUp && DroneBody.transform.position.y <= targetHeight)
        {
            isMovingUp = true;
        }

        float newY = Mathf.MoveTowards(DroneBody.transform.position.y, targetHeight, floatSpeed * Time.deltaTime);

        droneFloatPosition = new Vector3(DroneBody.transform.position.x, newY, DroneBody.transform.position.z);
        DroneBody.transform.position = droneFloatPosition;
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
                Instantiate(explosionPrefab, DroneBody.transform.position, Quaternion.identity);

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
