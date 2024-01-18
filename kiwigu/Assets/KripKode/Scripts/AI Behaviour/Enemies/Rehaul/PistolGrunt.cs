using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PistolGrunt : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter sfxEmitterAvailable;

    public enum EnemyState { Wandering, Seek, Cover, Shoot };
    [SerializeField] private EnemyState enemyState = EnemyState.Wandering;

    [Header("Drone Basic Settings")]
    [Range(0, 100)]
    [SerializeField] private float health;
    [Range(0, 100)]
    [SerializeField] private float shield;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject shieldObject;
    [SerializeField] private GameObject ragdoll;
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
    [SerializeField] private float shootDistance;
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
    private GameObject detectedPlayer;
    private float lastVisibleTime;
    private bool rememberPlayer;
    private bool loggedHidingObject;
    private GameObject loggedGameObject;
    private Vector3 hidingPos;
    private bool restocking;

    [Space(10)]
    [Header("Enemy Panic Settings")]
    [SerializeField] private float hideTime;
    [SerializeField] private GameObject hookTarget;
    [SerializeField] private Transform hookTargetPosition;
    private bool isHiding;
    public float hideTimer;
    private GameObject lastSelectedCover;

    [Space(10)]
    [Header("Enemy Attack Settings")]
    [SerializeField] Transform BulletExitPoint;
    [SerializeField] float shootCooldown;
    public bool isShooting;
    GunInfo info;
    float shootTimer;
    bool animDone;

    private bool gotHit;
    private float maxRotationTime = 0.035f; // smooothers
    private Quaternion startRotation;
    private float currentRotationTime;
    private bool isRotating;

    private void Start()
    {
        ht = GetComponentInChildren<HookTarget>();
        if (ht)
            isHoldingGun = true;

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
        Cover();
        Shoot();
        RememberPlayer();
    }

    private void StateManager()
    {
        if (!restocking && agent.velocity.magnitude <= 0.1f)
        {
            animator.SetBool("walk", false);
            animator.SetBool("run", false);
        }

        if (isShooting)
            RotateGunAndBodyTowardsPlayer();

        if (isHiding)
        {
            hideTimer += Time.deltaTime;

            if (hideTimer >= hideTime)
            {
                isShooting = false;
                gotHit = false;
                animator.SetBool("crouching", false);
                hideTimer = 0;
                isHiding = false;
            }
            return;
        }


        if (isShooting && !gotHit)
            return;

        if (gotHit && isHoldingGun && currentShield >= shield)
        {
            enemyState = EnemyState.Cover;
        }
        else if (gotHit && !isHoldingGun && currentShield >= shield) // change this later to puncherino
        {
            enemyState = EnemyState.Cover;
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
            agent.speed = seekSpeed;

            if (agent.velocity.magnitude >= 0.1f)
            {
                animator.SetBool("run", true);
            }

            Vector3 adjustedDestination = detectedPlayer.transform.position - (detectedPlayer.transform.position - transform.position).normalized * keepDistance;

            if (IsPlayerWithinRange() && !isShooting)
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

    private void Cover()
    {
        if (enemyState == EnemyState.Cover)
        {
            if (isHiding)
                return;

            agent.speed = panicSpeed;

            if (agent.velocity.magnitude >= 0.1f)
            {
                animator.SetBool("run", true);
            }

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
                animator.SetBool("crouching", true);
                isHiding = true;
                loggedGameObject = null;
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
            if (isShooting)
                return;

            StartCoroutine(ShootRoutine());
        }
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
            GameObject selectedCover = null;

            do
            {
                selectedCover = nearbyCovers[Random.Range(0, nearbyCovers.Count)];
            } while (selectedCover == lastSelectedCover);

            lastSelectedCover = selectedCover;

            return selectedCover;
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

        if (isShooting)
        {
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, Time.deltaTime * 4f);
        }
        else
        {
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, Time.deltaTime * 10);
        }
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

    private bool IsPlayerWithinRange()
    {
        float distanceTolerance = 0.5f;
        float distanceToDestination = Vector3.Distance(transform.position, detectedPlayer.transform.position);

        if (distanceToDestination < (shootDistance + distanceTolerance) && IsPlayerVisible())
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
        gotHit = true;
    }

    public virtual void TakeDamage(float bulletDamage)
    {
        if (isDead)
            return;

        if (currentShield < shield)
        {
            StartLerpShieldProgress();
            currentShield = Mathf.Min(currentShield + bulletDamage, shield);
        }
        else if (currentHealth < health)
        {
            agent.SetDestination(transform.position);
            animator.SetInteger("HitIndex", Random.Range(0, 3));
            animator.SetTrigger("Hit");
            gotHit = true;
            currentHealth = Mathf.Min(currentHealth + bulletDamage, health);
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
            ht.blockSteal = false;
            Destroy(shieldObject);
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
