using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HellfireEnemy : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter sfxEmitterAvailable;

    public enum EnemyState { Wandering, Seek, Shoot, Leap };
    [SerializeField] private EnemyState enemyState = EnemyState.Wandering;

    [Header("Drone Basic Settings")]
    [Range(0, 100)]
    [SerializeField] private float health;
    [Range(0, 100)]
    [SerializeField] private float shield;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject shieldObject;
    [SerializeField] private GameObject HeadshotIndicator;
    [SerializeField] private GameObject BreakingShieldIndicator;
    [SerializeField] private GameObject BrokenShieldIndicator;
    [SerializeField] private Transform headPos;
    [SerializeField] private GameObject ragdoll;
    [SerializeField] GameObject splatoodFX;
    private bool lerpingShield = false;
    private Material shieldMaterial;
    private float shieldLerpStartTime;
    private float shieldLerpDuration = 0.15f;
    private float startShieldValue;
    private float targetShieldValue;
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
    [SerializeField] private float marchDistance;
    [SerializeField] private float wanderWaitTime;
    [SerializeField] private float wanderRadius;
    [SerializeField] private float rememberWaitTime;
    HookTarget ht;
    private NavMeshAgent agent;
    private float wanderTimer;
    private Vector3 initialPosition;

    [Space(10)]
    [Header("Enemy Seeking Settings")]
    [SerializeField] private Transform eyesPosition;
    [SerializeField] private float seekRange;
    [SerializeField] private GameObject hookTarget;
    [SerializeField] private Transform hookTargetPosition;
    private GameObject detectedPlayer;
    private float lastVisibleTime;
    private bool rememberPlayer;
    private bool holdingShield = true;

    [Space(10)]
    [Header("Enemy Attack Settings")]
    [SerializeField] private GunInfo gunInfo;
    [SerializeField] Transform BulletExitPoint;
    [SerializeField] float shootCooldown;
    [Range(0, 0.25f)]
    [SerializeField] private float gunSpread;
    public bool isShooting;
    private GunInfo info;
    private float shootTimer;
    private bool animDone;
    private float maxRotationTime = 0.05f;
    private Quaternion startRotation;
    private float currentRotationTime;
    private bool isRotating;
    private bool isLeaping;
    private bool canLeap = true;

    private void Awake()
    {
        ht = GetComponentInChildren<HookTarget>();

        if (ht)
        {
            isHoldingGun = true;
            ht.info = gunInfo;
        }
    }

    private void Start()
    {
        ht = GetComponentInChildren<HookTarget>();

        if (ht)
        {
            isHoldingGun = true;
        }

        agent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position;

        if (shield > 0 && ht)
        {
            ht.blockSteal = true;
        }
    }

    private void Update()
    {
        if (lerpingShield)
        {
            LerpShieldProgressUpdate();
        }

        if (isDead)
            return;

        StateManager();
        Wander();
        Seek();
        Shoot();
        Leap();
        RememberPlayer();
    }

    private void StateManager()
    {
        if (agent.velocity.magnitude <= 0.1f)
        {
            animator.SetBool("walk", false);
            animator.SetBool("run", false);
        }

        if (isShooting)
            RotateGunAndBodyTowardsPlayer();

        if (isShooting)
            return;

        if (!IsPlayerVisible() && !rememberPlayer)
        {
            enemyState = EnemyState.Wandering;
        }
        else
        {
            enemyState = EnemyState.Seek;
        }
    }

    private void Wander()
    {
        if (enemyState == EnemyState.Wandering)
        {
            agent.speed = wanderSpeed;
            animator.speed = 1.0f;

            if (agent.velocity.magnitude >= 0.1f)
            {
                animator.SetBool("walk", true);
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

    private void Seek()
    {
        if (enemyState == EnemyState.Seek)
        {
            if (holdingShield)
                agent.speed = seekSpeed;
            else
                agent.speed = seekSpeed * 1.35f;

            Vector3 adjustedDestination = detectedPlayer.transform.position - (detectedPlayer.transform.position - transform.position).normalized * keepDistance;

            if (IsPlayerWithinRange() && !isShooting)
            {
                if (holdingShield)
                {
                    if (agent.velocity.magnitude >= 0.1f)
                    {
                        animator.speed = 1.0f;

                        animator.SetBool("walk", true);
                        animator.SetBool("run", false);
                    }
                }
                else
                {
                    if (agent.velocity.magnitude >= 0.1f)
                    {
                        animator.speed = 1.55f;
                        animator.SetBool("run", true);
                        animator.SetBool("walk", false);
                    }
                }

                if (isHoldingGun)
                    agent.SetDestination(adjustedDestination);
                else
                    agent.SetDestination(detectedPlayer.transform.position);

                float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.transform.position);

                if (distanceToPlayer <= keepDistance + 2 && isHoldingGun)
                {
                    enemyState = EnemyState.Shoot;
                    agent.SetDestination(transform.position);
                }
                else if (distanceToPlayer <= wanderRadius && !isHoldingGun)
                {
                    enemyState = EnemyState.Leap;
                }
            }
            else
            {
                if (agent.velocity.magnitude >= 0.1f)
                {
                    animator.speed = 1.2f;
                    animator.SetBool("run", true);
                    animator.SetBool("walk", false);
                }

                agent.SetDestination(adjustedDestination);
            }
        }
    }

    private void Shoot()
    {
        if (enemyState == EnemyState.Shoot)
        {
            if (isShooting)
                return;

            StartCoroutine(ShootRoutine());
        }
    }

    private void Leap()
    {
        if (enemyState == EnemyState.Leap)
        {
            if (isLeaping)
                return;

            StartCoroutine(LeapCoroutine());
        }
    }

    public void SplatoodEvent()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5, LayerMask.GetMask("Player"));

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                Vector3 incomingDirection = (detectedPlayer.transform.position - transform.position).normalized;
                Vector3 upwardDirection = Vector3.up;

                Vector3 punchDirection = (incomingDirection + upwardDirection).normalized;

                hitCollider.GetComponent<PlayerHealth>().DealDamage(35, -incomingDirection.normalized * 10);
                sweatersController.instance.velocity += punchDirection * 25;
            }
        }

        GameObject dfx = Instantiate(splatoodFX, transform.position, Quaternion.identity);
        Destroy(dfx, 2);
    }

    IEnumerator ShootRoutine()
    {
        isShooting = true;

        animator.SetTrigger("shoot");

        yield return null; // wait a framerino

        while (animDone)
        {
            yield return null;
        }

        animator.ResetTrigger("shoot");

        yield return new WaitForSeconds(shootCooldown);

        isShooting = false;
    }

    IEnumerator LeapCoroutine()
    {
        isLeaping = true;

        animator.SetTrigger("leap");

        yield return null; // wait a framerino

        while (animDone)
        {
            yield return null;
        }

        animator.ResetTrigger("leap");

        yield return new WaitForSeconds(3);

        isLeaping = false;
    }

    private void RotateGunAndBodyTowardsPlayer()
    {
        if (!isShooting) return;

        if (agent.isOnNavMesh)
            RotateNavMeshAgentTowardsObj(detectedPlayer.transform.position);

        RotateGunObjectExitPoint(detectedPlayer.transform.position);
    }

    private void RotateNavMeshAgentTowardsObj(Vector3 objPos)
    {
        if (!isShooting) return;

        agent.SetDestination(agent.transform.position);

        Quaternion targetRotation = Quaternion.LookRotation(objPos - agent.transform.position);

        agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, Time.deltaTime * 10);
    }

    private void RotateGunObjectExitPoint(Vector3 playerPosition)
    {
        Vector3 targetPosition = new Vector3(playerPosition.x, playerPosition.y + 1f, playerPosition.z);
        Vector3 direction = targetPosition - BulletExitPoint.transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        if (isRotating)
        {
            currentRotationTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentRotationTime / maxRotationTime);

            BulletExitPoint.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);

            if (currentRotationTime >= maxRotationTime)
            {
                isRotating = false;
            }
        }

        startRotation = BulletExitPoint.transform.rotation;
        currentRotationTime = 0f;
        isRotating = true;
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

    private bool IsPlayerWithinRange()
    {
        float distanceTolerance = 0.5f;
        float distanceToDestination = Vector3.Distance(transform.position, detectedPlayer.transform.position);

        if (distanceToDestination < (marchDistance + distanceTolerance) && IsPlayerVisible())
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

    // can't do animDone = !animDone because it breaks due to hide timerino :(
    public void AnimTrue()
    {
        animDone = true;
    }

    public void AnimFalse()
    {
        animDone = false;
    }

    public void ShootEvent()
    {
        EnemyShoot();
    }

    IEnumerator ShootFloat()
    {
        yield return new WaitForSeconds(EnemyShoot());
    }

    public virtual void TakeGun()
    {
        isHoldingGun = false;
        isShooting = false;
        enemyState = EnemyState.Wandering;
    }

    public virtual void TakeDamage(float bulletDamage, bool isHeadshot)
    {
        if (isDead)
            return;

        if (currentShield < shield)
        {
            StartLerpShieldProgress();

            float healthPercent = 1.0f - Mathf.Clamp01(currentShield / shield);
            shieldMaterial.SetFloat("_HealthPercent", healthPercent);

            currentShield = Mathf.Min(currentShield + bulletDamage, shield);

            Instantiate(BreakingShieldIndicator, shieldObject.transform.position, Quaternion.identity);
        }
        else if (currentHealth < health)
        {
            if (isHeadshot)
                Instantiate(HeadshotIndicator, headPos.transform.position, Quaternion.identity);

            agent.SetDestination(transform.position);
            animator.SetInteger("HitIndex", Random.Range(0, 3));
            animator.SetTrigger("Hit");
            currentHealth = Mathf.Min(currentHealth + bulletDamage, health);

            AnimFalse();
        }

        CheckStats();
    }

    private void StartLerpShieldProgress()
    {
        shieldMaterial = shieldObject.GetComponent<Renderer>().material;
        shieldLerpStartTime = Time.time;
        startShieldValue = shieldMaterial.GetFloat("_Progress");
        targetShieldValue = 4f;
        lerpingShield = true;
    }

    private void LerpShieldProgressUpdate()
    {
        float elapsedTime = Time.time - shieldLerpStartTime;

        if (elapsedTime < shieldLerpDuration)
        {
            float progress = Mathf.Lerp(startShieldValue, targetShieldValue, elapsedTime / shieldLerpDuration);
            shieldMaterial.SetFloat("_Progress", progress);
        }
        else
        {
            shieldMaterial.SetFloat("_Progress", targetShieldValue);
            lerpingShield = false;

            StartReverseLerp();
        }
    }

    private void StartReverseLerp()
    {
        shieldLerpStartTime = Time.time;
        startShieldValue = targetShieldValue;
        targetShieldValue = 0f;
        lerpingShield = true;
    }

    public void CheckStats()
    {
        if (currentShield >= shield)
        {
            if (shieldObject)
            {
                Instantiate(BrokenShieldIndicator, shieldObject.transform.position, Quaternion.identity);
                ht.blockSteal = false;
                holdingShield = false;
                Destroy(shieldObject);
            }
        }

        if (currentHealth >= health && !isDead)
        {
            isDead = true;

            GameObject Ragdollerino = Instantiate(ragdoll, transform.position, transform.rotation);

            if (isHoldingGun)
            {
                EnableHookTargetsRecursively(Ragdollerino.transform);

                GetComponentInChildren<HookTarget>();
                if (ht != null) ht.BeforeDestroy();

                isHoldingGun = false;
                isShooting = false;
            }
            agent.SetDestination(transform.position);

            //animator.SetInteger("DeadIndex", Random.Range(0, 3));
            //animator.SetTrigger("Dead");

            Destroy(Ragdollerino, 15f);
            Destroy(gameObject);
        }
    }

    private void EnableHookTargetsRecursively(Transform parent)
    {
        HookTarget hookTarget = parent.GetComponent<HookTarget>();

        if (hookTarget != null)
        {
            hookTarget.gameObject.SetActive(true);
        }

        foreach (Transform child in parent)
        {
            EnableHookTargetsRecursively(child);
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
            isShooting = false;
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
        if (!animDone)
            return;

        if (!isHoldingGun)
        {
            isShooting = false;
            return;
        }

        HookTarget gun = transform.GetComponentInChildren<HookTarget>();

        if (gun)
            info = gun.info;

        GameObject bullet = Instantiate(info.bulletPrefab, BulletExitPoint.transform.position, BulletExitPoint.transform.rotation);

        Vector3 direction = BulletExitPoint.transform.forward;
        direction += SpreadDirection(gunSpread, 3);

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

    private void OnDrawGizmos()
    {
        // Draw each sphere with a different color
        DrawColoredSphere(transform.position, seekRange, Color.red);
        DrawColoredSphere(transform.position, marchDistance, Color.blue);
        DrawColoredSphere(transform.position, keepDistance, Color.green);
        DrawColoredSphere(transform.position, wanderRadius, Color.yellow);
    }

    private void DrawColoredSphere(Vector3 center, float radius, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(center, radius);
    }
}
