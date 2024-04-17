using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PistolGrunt : MonoBehaviour
{
    public enum EnemyState { Wandering, Seek, Punch, Shoot };
    [SerializeField] private EnemyState enemyState = EnemyState.Wandering;

    [Header("Grunt Basic Settings")]
    [Range(0, 100)]
    [SerializeField] private float health;
    [Range(0, 100)]
    [SerializeField] private float shield;
    [Range(0, 100)]
    [SerializeField] private float backPackHealth;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject shieldObject;
    [SerializeField] private GameObject HeadshotIndicator;
    [SerializeField] private GameObject BrokenShieldIndicator;
    [SerializeField] private GameObject ragdoll;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private Transform headPos;
    [SerializeField] private Transform spineBone;
    private bool lerpingShield = false;
    private Material shieldMaterial;
    private float shieldLerpStartTime;
    private float shieldLerpDuration = 0.15f;
    private float startShieldValue;
    private float targetShieldValue;
    private bool isHoldingGun;
    private float currentHealth;
    private float currentShield;
    public float currentBackpackHealth;
    private bool isDead;

    [Space(10)]
    [Header("Enemy Movement Settings")]
    [SerializeField] private float wanderSpeed;
    [SerializeField] private float seekSpeed;
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

    [Space(10)]
    [Header("Enemy Attack Settings")]
    [SerializeField] private GunInfo gunInfo;
    [Range(0, 0.25f)]
    [SerializeField] private float gunSpread;
    [SerializeField] private bool isDefense;
    [SerializeField] Transform BulletExitPoint;
    [SerializeField] float shootCooldown;
    [SerializeField] private float punchSpeed;
    [SerializeField] private float punchDistance;
    private bool isShooting;
    bool animDone;

    private bool gotHit;
    private float maxRotationTime = 0.2f;
    private Quaternion startRotation;
    private float currentRotationTime;
    private bool isRotating;

    private float lastPunchTime;
    private float punchCooldown = 1.0f;
    public string enemyGender;

    private void Awake()
    {
        ChooseVoiceGender();

        ht = GetComponentInChildren<HookTarget>();

        if (ht)
        {
            isHoldingGun = true;
            ht.info = gunInfo;
        }
    }

    private void ChooseVoiceGender()
    {
        int random = Random.Range(0, 2);
        if (random == 0)
        {
            enemyGender = "Male";
        }
        else
        {
            enemyGender = "Female";
        }
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position;

        if (shield > 0 && ht)
        {
            ht.blockSteal = true;
        }



        ht = GetComponentInChildren<HookTarget>();

        if (ht)
        {
            isHoldingGun = true;

            BulletShooter bs = transform.GetComponentInChildren<BulletShooter>();
            if (bs) bs.info = ht.info;
        }
    }

    private void Update()
    {
        if (PauseSystem.paused)
            return;

        if (lerpingShield)
        {
            UpdateLerpShieldProgress();
        }

        if (isDead)
            return;

        StateManager();
        Wander();
        Seek();
        Punch();
        Shoot();
        RememberPlayer();
    }

    private void LateUpdate()
    {
        if (isDead || !detectedPlayer)
            return;

        Vector3 directionToPlayer = detectedPlayer.transform.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer <= 90)
        {
            spineBone.LookAt(detectedPlayer.transform.position);
        }
        else if (detectedPlayer && Vector3.Distance(transform.position, detectedPlayer.transform.position) > seekRange)
        {
            spineBone.rotation = Quaternion.Euler(Vector3.zero);
        }
    }

    public virtual void TakeGun()
    {
        GlobalAudioManager.instance.PlayEnemyBark(transform, "Take Gun", enemyGender);
        isHoldingGun = false;
        isShooting = false;
        gotHit = true;
    }

    private void StateManager()
    {
        if (agent.velocity.magnitude <= 0.1f)
        {
            animator.SetBool("walk", false);
            animator.SetBool("run", false);
        }

        if (isShooting)
        {
            RotateGunAndBodyTowardsPlayer();
            return;
        }

        if (gotHit && !isHoldingGun)
        {
            enemyState = EnemyState.Punch;
        }
        else if (!IsPlayerVisible() && !rememberPlayer)
        {
            enemyState = EnemyState.Wandering;
        }
        else if ((IsPlayerVisible() || rememberPlayer) && isHoldingGun)
        {
            enemyState = EnemyState.Seek;
        }
    }

    private void Wander()
    {
        if (enemyState == EnemyState.Wandering)
        {
            agent.speed = wanderSpeed;
            animator.speed = wanderSpeed * 0.4f;

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
            animator.speed = seekSpeed * 0.25f;

            if (agent.velocity.magnitude >= 0.1f)
            {
                animator.SetBool("run", true);
            }

            Vector3 playerPosition = detectedPlayer.transform.position;
            Vector3 directionToPlayer = playerPosition - transform.position;

            int layerMask = 1 << LayerMask.NameToLayer("Player");

            RaycastHit hit;
            //Debug.DrawLine(eyesPosition.position, playerPosition, Color.red);

            if (Physics.Raycast(eyesPosition.position, directionToPlayer, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.gameObject != detectedPlayer)
                {
                    Vector3 newDestination = FindNewDestinationAroundObstacle(playerPosition);
                    agent.SetDestination(newDestination);
                    return;
                }
            }

            if (IsPlayerWithinRange() && !isShooting)
            {
                agent.SetDestination(transform.position);
                enemyState = EnemyState.Shoot;
            }
            else
            {
                agent.SetDestination(playerPosition);
            }
        }
    }

    private Vector3 FindNewDestinationAroundObstacle(Vector3 targetPosition)
    {
        Vector3 randomDirection = Random.insideUnitSphere * 5f;
        randomDirection.y = 0f;
        Vector3 newDestination = targetPosition + randomDirection;
        return newDestination;
    }

    private void Punch()
    {
        if (enemyState == EnemyState.Punch)
        {
            agent.speed = punchSpeed;
            animator.speed = punchSpeed * 0.15f;

            if (agent.velocity.magnitude >= 0.1f)
            {
                animator.SetBool("run", true);
            }

            if (!detectedPlayer)
                return;

            Vector3 playerPosition = detectedPlayer.transform.position;
            Vector3 directionToPlayer = playerPosition - transform.position;

            int layerMask = 1 << LayerMask.NameToLayer("Player");

            RaycastHit hit;
            //Debug.DrawLine(eyesPosition.position, playerPosition, Color.red);

            if (Physics.Raycast(eyesPosition.position, directionToPlayer, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.gameObject != detectedPlayer)
                {
                    Vector3 newDestination = FindNewDestinationAroundObstacle(playerPosition);
                    agent.SetDestination(newDestination);
                    return;
                }
            }

            if (Time.time - lastPunchTime >= punchCooldown)
            {
                if (Vector3.Distance(transform.position, playerPosition) <= punchDistance)
                {
                    agent.SetDestination(transform.position);

                    if (isDefense)
                    {
                        Vector3 explosionPosition = detectedPlayer.transform.position + (detectedPlayer.transform.forward * 2) + (detectedPlayer.transform.up * 1);
                        Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);

                        TakeDamage(1000000, false);
                        return;
                    }

                    animator.SetTrigger("punch");
                    gotHit = false;
                    lastPunchTime = Time.time;

                    Quaternion targetRotation = Quaternion.LookRotation(playerPosition - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 150);
                }
                else
                {
                    agent.SetDestination(playerPosition);
                }
            }
        }
    }

    public void PunchEvent()
    {
        if (detectedPlayer)
        {
            Vector3 incomingDirection = (detectedPlayer.transform.position - transform.position).normalized;
            Vector3 upwardDirection = Vector3.up;

            Vector3 punchDirection = (incomingDirection + upwardDirection).normalized;

            var treshold = 1;

            if (Vector3.Distance(transform.position, detectedPlayer.transform.position) <= punchDistance + treshold)
            {
                detectedPlayer.GetComponent<PlayerHealth>().DealDamage(25, -incomingDirection);
                sweatersController.instance.velocity += punchDirection * 5;
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
            if (currentHealth <= 10)
                animator.speed = 4.0f;
            else if (currentHealth <= 25)
                animator.speed = 3.0f;
            else if (currentHealth <= 50)
                animator.speed = 2.0f;

            yield return null;
        }

        animator.speed = 1.0f;

        animator.ResetTrigger("shoot");

        yield return new WaitForSeconds(shootCooldown);

        isShooting = false;
    }

    private void RotateGunAndBodyTowardsPlayer()
    {
        if (agent.isOnNavMesh)
            RotateNavMeshAgentTowardsObj(detectedPlayer.transform.position);

        RotateGunObjectExitPoint(detectedPlayer.transform.position);
    }

    private void RotateNavMeshAgentTowardsObj(Vector3 objPos)
    {
        agent.SetDestination(agent.transform.position);

        Vector3 direction = objPos - agent.transform.position;
        if (direction.sqrMagnitude < Mathf.Epsilon)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        float rotationSpeed = isShooting ? 4f : 10f;

        agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * 1.5f);
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
        int layerMask3 = LayerMask.GetMask("Shield");
        int layerMask4 = LayerMask.GetMask("GunHand");
        int layerMask5 = LayerMask.GetMask("EnergyWall");
        int layerMask6 = LayerMask.GetMask("Backpack");

        int combinedLayerMask = layerMask | layerMask2 | layerMask3 | layerMask4 | layerMask5 | layerMask6;


        foreach (Collider hitCollider in hitColliders)
        {
            RaycastHit hit;
            //Debug.DrawLine(eyesPosition.position, hitCollider.transform.position, Color.red);

            if (Physics.Raycast(eyesPosition.position, hitCollider.transform.position - eyesPosition.position - new Vector3(0, -1, 0), out hit, seekRange, ~combinedLayerMask))
            {
                Debug.DrawRay(eyesPosition.position, hitCollider.transform.position - eyesPosition.position - new Vector3(0, -1, 0));

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
        if (PauseSystem.paused)
            return;

        EnemyShoot();
    }

    public void BackpackDamage(float bulletDamage)
    {
        if (isDead)
            return;

        if (currentBackpackHealth < backPackHealth)
        {
            // apply shader backpack hit maybe here idk lol
            currentBackpackHealth = Mathf.Min(currentBackpackHealth + bulletDamage, backPackHealth);
        }

        if (currentBackpackHealth >= backPackHealth)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            TakeDamage(1000000, false);
        }
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
        }
        else if (currentHealth < health)
        {
            if (isHeadshot)
                Instantiate(HeadshotIndicator, headPos.transform.position, Quaternion.identity);

            agent.SetDestination(transform.position);

            if(currentHealth >= health / 2f)
            {
                animator.SetInteger("HitIndex", Random.Range(0, 3));
                animator.SetTrigger("Hit");
            }

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

    private void UpdateLerpShieldProgress()
    {
        float elapsedTime = Time.time - shieldLerpStartTime;
        float progress = Mathf.Lerp(startShieldValue, targetShieldValue, elapsedTime / shieldLerpDuration);

        shieldMaterial.SetFloat("_Progress", progress);

        if (elapsedTime >= shieldLerpDuration)
        {
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
                Destroy(shieldObject);
            }
        }

        if (currentHealth >= health && !isDead)
        {
            isDead = true;

            GameObject ragdollInstance = Instantiate(ragdoll, transform.position, transform.rotation);

            if (isHoldingGun)
            {
                bool stillHasGun = true;
                HookTarget ht = GetComponentInChildren<HookTarget>();
                if (ht != null) stillHasGun = ht.BeforeDestroy();

                if (stillHasGun) EnableHookTargetsRecursively(ragdollInstance.transform);

                isHoldingGun = false;
                isShooting = false;
            }

            SendDeathSignal();
            agent.SetDestination(transform.position);
            Destroy(ragdollInstance, 15f);
            Destroy(gameObject);
        }
    }

    public void CommunicateDeath(Transform transForm, string gender)
    {
        Debug.Log("sent");
        GlobalAudioManager.instance.PlayEnemyBark(transForm, "Death Alert", gender);
    }

    private void SendDeathSignal()
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Enemy");

        List<float> distances = new List<float>();
        List<GameObject> nearbyObjects = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj == gameObject || obj.transform.IsChildOf(transform))
            {
                continue;
            }

            PistolGrunt pistolGruntScript = obj.GetComponent<PistolGrunt>();
            HellfireEnemy hellfireScript = obj.GetComponent<HellfireEnemy>();

            if (pistolGruntScript != null || hellfireScript != null)
            {
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                if (distance <= seekRange)
                {
                    distances.Add(distance);
                    nearbyObjects.Add(obj);
                }
            }
        }

        if (nearbyObjects.Count > 0)
        {
            float minDistance = Mathf.Min(distances.ToArray());
            int minIndex = distances.IndexOf(minDistance);
            GameObject closestObject = nearbyObjects[minIndex];

            if (closestObject.GetComponent<PistolGrunt>() != null)
            {
                closestObject.GetComponent<PistolGrunt>().CommunicateDeath(closestObject.transform, closestObject.GetComponent<PistolGrunt>().enemyGender);
            }
            else if (closestObject.GetComponent<HellfireEnemy>() != null)
            {
                closestObject.GetComponent<HellfireEnemy>().CommunicateDeath(closestObject.transform, closestObject.GetComponent<HellfireEnemy>().enemyGender);
            }
        }
    }

    private void EnableHookTargetsRecursively(Transform parent)
    {
        HookTarget hookTarget = parent.GetComponent<HookTarget>();

        if (hookTarget != null)
        {
            hookTarget.gameObject.SetActive(true);
        }

        int childCount = parent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = parent.GetChild(i);
            EnableHookTargetsRecursively(child);
        }
    }


    private float EnemyShoot()
    {
        if (!detectedPlayer)
            return 0;

        if (!isHoldingGun || !IsPlayerVisible())
            return 0;

        HookTarget gun = transform.GetComponentInChildren<HookTarget>();
        if (gun == null)
        {
            isHoldingGun = false;
            isShooting = false;

            BulletShooter b = transform.GetComponentInChildren<BulletShooter>();
            if (b) Destroy(b);

            return 0;
        }
        GunInfo info = gun.info;

        float burst = info.burstSize;
        if (info.fullAuto) burst = info.autoRate;

        BulletShooter bs = transform.GetComponentInChildren<BulletShooter>();
        if (bs)
        {
            if (info.fullAuto) bs.SetShootTime(0.3f);
            else bs.SetIsShooting();
        }

        //for (int j = 0; j < burst; j++)
        //{
        //    for (int i = 0; i < info.projectiles; i++) Invoke(nameof(SpawnBullet), j * 1 / info.autoRate);
        //}

        return burst * 1 / info.autoRate;
    }

    //private void SpawnBullet()
    //{
    //    if (!animDone)
    //        return;

    //    if (!isHoldingGun)
    //    {
    //        isShooting = false;
    //        return;
    //    }

    //    HookTarget gun = transform.GetComponentInChildren<HookTarget>();

    //    if (gun)
    //        gunInfo = gun.info;

    //    GameObject bullet = Instantiate(gunInfo.bulletPrefab, BulletExitPoint.transform.position, BulletExitPoint.transform.rotation);

    //    Vector3 direction = BulletExitPoint.transform.forward;
    //    direction += SpreadDirection(gunSpread, 3);

    //    bullet.transform.position = BulletExitPoint.transform.position;
    //    bullet.transform.rotation = Quaternion.LookRotation(direction.normalized);

    //    Bullet b = bullet.GetComponent<Bullet>();
    //    b.speed = gunInfo.bulletSpeed;
    //    b.gravity = gunInfo.bulletGravity;
    //    b.ignoreMask = ~LayerMask.GetMask("GunHand", "HookTarget", "Enemy");
    //    b.trackTarget = false;
    //    b.fromEnemy = true;
    //    b.bulletDamage = gunInfo.damage;
    //    b.charge = 0.5f;
    //}

    //private Vector3 SpreadDirection(float spread, int rolls)
    //{
    //    Vector3 offset = Vector3.zero;
    //    for (int i = 0; i < rolls; i++)
    //        offset += Random.onUnitSphere * spread;
    //    return offset / rolls;
    //}

    private void OnDrawGizmos()
    {
        // Draw each sphere with a different color
        DrawColoredSphere(transform.position, seekRange, Color.red);
        DrawColoredSphere(transform.position, shootDistance, Color.blue);
        DrawColoredSphere(transform.position, keepDistance, Color.green);
        DrawColoredSphere(transform.position, wanderRadius, Color.yellow);
    }

    private void DrawColoredSphere(Vector3 center, float radius, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(center, radius);
    }
}
