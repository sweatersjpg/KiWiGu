using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Mechemy : MonoBehaviour
{
    public enum EnemyState { Wandering, Shoot, Crush };
    [SerializeField] private EnemyState enemyState = EnemyState.Wandering;

    [Header("Mechemy Basic Settings")]
    [Range(0, 500)]
    [SerializeField] private float health;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform headPos;
    [SerializeField] private GameObject HeadshotIndicator;
    [SerializeField] private GameObject ExplosionX;
    [SerializeField] private GameObject Ragdoll;
    [SerializeField] private GameObject FireWarningSFX;
    [SerializeField] private Transform spineBone;
    private bool isDead;
    private float currentHealth;

    [Space(10)]
    [Header("Enemy Detection Settings")]
    [SerializeField] private Transform eyesPosition;
    [SerializeField] private float detectionRange;
    [SerializeField] private float rememberWaitTime;
    private float lastVisibleTime;
    private bool rememberPlayer;
    private GameObject detectedPlayer;

    [Space(10)]
    [Header("Enemy Movement Settings")]
    [SerializeField] private float wanderSpeed;
    [SerializeField] private float wanderWaitTime;
    [SerializeField] private float wanderRadius;
    private Vector3 initialPosition;
    private float wanderTimer;

    [Space(10)]
    [Header("Enemy Attack Settings")]
    [SerializeField] float shootCooldown;
    [SerializeField] float rememberPlayerTime;
    [SerializeField] private Transform leftGunExitPoint;
    [SerializeField] private Transform rightGunExitPoint;
    public HookTarget leftGun;
    public HookTarget rightGun;
    Transform BulletExitPoint;
    private bool isShooting;
    private bool animDone;
    private bool isRotating;
    private float maxRotationTime = 0.05f;
    private Quaternion startRotation;
    private float currentRotationTime;
    private bool angleCalculated;
    private float angle;

    private bool checkedLeftGun;
    private bool checkedRightGun;

    private bool holdingLeftGun;
    private bool holdingRightGun;

    private NavMeshAgent agent;
    GunInfo infoHT;

    // Crush
    private bool isCrushing;
    private float splatoodRadius = 5;
    public GameObject splatoodFX;

    private void Awake()
    {
        holdingLeftGun = leftGun != null;
        holdingRightGun = rightGun != null;
        BulletExitPoint = rightGunExitPoint;

        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (isDead)
            return;

        StateManager();
        RememberPlayer();
        Wander();
        ShootState();
        Crush();
    }

    private void LateUpdate()
    {
        if (!detectedPlayer)
            return;

        Vector3 directionToPlayer = detectedPlayer.transform.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer <= 90)
        {
            Vector3 offsetTargetPosition = detectedPlayer.transform.position + new Vector3(0, 1f, 0);
            spineBone.LookAt(offsetTargetPosition);
        }
        else if (detectedPlayer && Vector3.Distance(transform.position, detectedPlayer.transform.position) > detectionRange)
        {
            spineBone.rotation = Quaternion.Euler(Vector3.zero);
        }
    }

    private void StateManager()
    {
        IsPlayerVisible();

        if (leftGun == null && !checkedLeftGun)
        {
            holdingLeftGun = false;
            checkedLeftGun = true;
        }

        if (rightGun == null && !checkedRightGun)
        {
            holdingRightGun = false;
            checkedRightGun = true;
        }

        if (agent.velocity.magnitude <= 0.1f)
        {
            animator.SetBool("walk", false);
        }

        if (!IsPlayerWithinRange() && !rememberPlayer)
        {
            enemyState = EnemyState.Wandering;
        }
        else
        {
            enemyState = EnemyState.Shoot;

            if (holdingLeftGun || holdingRightGun)
            {
                enemyState = EnemyState.Shoot;
            }
            else
            {
                enemyState = EnemyState.Crush;
            }
        }
    }

    private void Wander()
    {
        if (enemyState == EnemyState.Wandering)
        {
            agent.speed = wanderSpeed;
            animator.speed = Mathf.Clamp(agent.speed / 2.5f, 0.5f, 1.15f);

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

    private void Crush()
    {
        if (enemyState == EnemyState.Crush)
        {
            if (isCrushing || isShooting)
                return;

            agent.speed = wanderSpeed * 2;
            animator.speed = Mathf.Clamp(agent.speed / 2.5f, 0.5f, 2.0f);

            if (agent.velocity.magnitude >= 0.1f)
            {
                animator.SetBool("walk", true);
            }

            agent.SetDestination(detectedPlayer.transform.position);

            float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.transform.position);

            if (distanceToPlayer <= wanderRadius && !isCrushing)
                StartCoroutine(LeapCoroutine());
        }
    }

    public virtual void TakeDamage(float bulletDamage, bool isHeadshot)
    {
        if (isDead)
            return;

        if (currentHealth < health)
        {
            if (isHeadshot)
            {
                Instantiate(HeadshotIndicator, headPos.transform.position, Quaternion.identity);
            }

            currentHealth = Mathf.Min(currentHealth + bulletDamage, health);
        }

        CheckStats();
    }

    public void CheckStats()
    {
        if (currentHealth >= health && !isDead)
        {
            isDead = true;

            Instantiate(ExplosionX, headPos.transform.position, transform.rotation);
            GameObject rag = Instantiate(Ragdoll, transform);
            rag.transform.parent = null;
            Destroy(rag, 4);

            agent.SetDestination(transform.position);

            Destroy(gameObject);
        }
    }

    IEnumerator LeapCoroutine()
    {
        isCrushing = true;

        animator.speed = 1.35f;

        animator.SetTrigger("crush");

        yield return null;

        while (animDone)
        {
            yield return null;
        }

        animator.ResetTrigger("crush");

        yield return new WaitForSeconds(2);

        isCrushing = false;
    }

    public void SplatoodEvent()
    {
        var splatoodPos = transform.position + transform.forward * 2;

        Collider[] hitColliders = Physics.OverlapSphere(splatoodPos, splatoodRadius, LayerMask.GetMask("Player"));

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                if (sweatersController.instance.isGrounded)
                {
                    Vector3 incomingDirection = (detectedPlayer.transform.position - transform.position).normalized;
                    Vector3 upwardDirection = Vector3.up;

                    Vector3 punchDirection = (incomingDirection + upwardDirection).normalized;

                    hitCollider.GetComponent<PlayerHealth>().DealDamage(35, -incomingDirection.normalized * 10);
                    sweatersController.instance.velocity += punchDirection * 15;

                    agent.SetDestination(transform.position);
                }
            }
        }

        GameObject dfx = Instantiate(splatoodFX, splatoodPos, Quaternion.identity);
        Destroy(dfx, 2);
    }

    private void ShootState()
    {
        if (enemyState == EnemyState.Shoot)
        {
            if (!IsPlayerWithinRange())
            {
                agent.SetDestination(detectedPlayer.transform.position);
                agent.speed = wanderSpeed;
                animator.speed = Mathf.Clamp(agent.speed / 2.5f, 0.5f, 1.15f);

                if (agent.velocity.magnitude >= 0.1f)
                {
                    animator.SetBool("walk", true);
                }

                return;
            }

            animator.speed = 1.0f;

            if (agent.velocity.magnitude >= 0.1f)
            {
                animator.SetBool("walk", false);
            }

            RotateNavMeshAgentTowardsObj(detectedPlayer.transform.position);

            if (isShooting)
                return;

            if (!detectedPlayer)
                return;

            if (holdingRightGun && holdingLeftGun)
            {
                if (Random.Range(0, 2) == 0)
                {
                    infoHT = rightGun.info;
                    BulletExitPoint = rightGunExitPoint;
                }
                else
                {
                    infoHT = leftGun.info;
                    BulletExitPoint = leftGunExitPoint;
                }
            }
            else if (holdingRightGun)
            {
                infoHT = rightGun.info;
                BulletExitPoint = rightGunExitPoint;
            }
            else if (holdingLeftGun)
            {
                infoHT = leftGun.info;
                BulletExitPoint = leftGunExitPoint;
            }

            if (!holdingRightGun && !holdingLeftGun)
            {
                return;
            }

            BulletShooter bs = BulletExitPoint.GetComponentInChildren<BulletShooter>();
            bs.isShooting = Time.time % 4 > 2;
        }
    }

    public void AnimTrue()
    {
        GameObject go = Instantiate(FireWarningSFX, headPos.transform.position, Quaternion.identity);
        Destroy(go, 2);

        animDone = true;
    }

    public void AnimFalse()
    {
        animDone = false;
    }

    private void RotateNavMeshAgentTowardsObj(Vector3 objPos)
    {
        agent.SetDestination(agent.transform.position);

        Quaternion targetRotation = Quaternion.LookRotation(objPos - agent.transform.position);

        targetRotation.x = 0;
        targetRotation.z = 0;

        agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, Time.deltaTime * 2.5f);

        RotateGunObjectExitPoint(detectedPlayer.transform.position);
    }

    private void RotateGunObjectExitPoint(Vector3 playerPosition)
    {
        if (!BulletExitPoint)
            return;

        Vector3 targetPosition = new Vector3(playerPosition.x, playerPosition.y + 1f, playerPosition.z);
        Vector3 direction = targetPosition - BulletExitPoint.transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        if (isRotating)
        {
            currentRotationTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentRotationTime / maxRotationTime);

            if (infoHT && infoHT.gunName == "EGL")
            {
                if (!angleCalculated)
                {
                    float distance = direction.magnitude;
                    float time = distance / infoHT.bulletSpeed;
                    float gravity = infoHT.bulletGravity;
                    angle = Mathf.Atan((time * time * gravity) / (2 * distance)) * Mathf.Rad2Deg;
                    angle *= Random.Range(0.35f, 0.75f);
                    angleCalculated = true;
                    angleCalculated = true;
                }

                BulletExitPoint.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                BulletExitPoint.transform.Rotate(Vector3.right, angle);
            }
            else
            {
                BulletExitPoint.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            }
            if (currentRotationTime >= maxRotationTime)
            {
                isRotating = false;
            }
        }
        else
        {
            startRotation = BulletExitPoint.transform.rotation;
            currentRotationTime = 0f;
            isRotating = true;
            angleCalculated = false;
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
        Collider[] hitColliders = Physics.OverlapSphere(eyesPosition.position, detectionRange, LayerMask.GetMask("Player"));
        int layerMask = LayerMask.GetMask("Enemy");
        int layerMask2 = LayerMask.GetMask("HookTarget");
        int layerMask3 = LayerMask.GetMask("Shield");
        int layerMask4 = LayerMask.GetMask("GunHand");
        int layerMask5 = LayerMask.GetMask("EnergyWall");
        int combinedLayerMask = layerMask | layerMask2 | layerMask3 | layerMask4 | layerMask5;


        foreach (Collider hitCollider in hitColliders)
        {
            RaycastHit hit;
            if (Physics.Raycast(eyesPosition.position, hitCollider.transform.position - eyesPosition.position - new Vector3(0, -1, 0), out hit, detectionRange, ~combinedLayerMask))
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
        if (detectedPlayer == null)
            return false;

        float distanceTolerance = 0.5f;
        float distanceToDestination = Vector3.Distance(transform.position, detectedPlayer.transform.position);

        if (distanceToDestination < (detectionRange + distanceTolerance) && IsPlayerVisible())
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

    private void OnDrawGizmos()
    {
        DrawColoredSphere(transform.position, detectionRange, Color.red);
        DrawColoredSphere(transform.position, wanderRadius, Color.yellow);
    }

    private void DrawColoredSphere(Vector3 center, float radius, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(center, radius);
    }
}
