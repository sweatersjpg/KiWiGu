using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.GraphicsBuffer;

public class EnemyBehaviour : EnemyBase
{
    private Vector3 moveTarget;

    public bool moveDroneUpOrDown;
    public float timeDroneMove;

    public bool moveUp;

    public bool detectedPlayer;
    public bool stoppedDrone;

    bool doingShootingPattern;
    Vector3 newPosition;

    float t;
    float newYPosition;

    float shotTimer = 0;
    float lastShotTime = 0;

    [HideInInspector] public bool canShoot;

    private float initialPositionY;
    private float verticalOffset = 0.25f;
    private float duration;
    private float startTime;
    private bool isInView;

    protected override void Start()
    {
        if (DefenseDrone || OffenseDrone)
        {
            duration = Random.Range(2, 4);
            initialPositionY = BodyMesh.transform.position.y;
            startTime = Time.time;
        }

        base.Start();
        shotTimer = Time.time;
    }

    public void EnemyMovement()
    {
        if (DefenseDrone)
        {
           
        }
        else if (OffenseDrone)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            detectedPlayer = players.Any(player => Vector3.Distance(transform.position, player.transform.position) < EnemyAwareDistance);

            if (detectedPlayer)
            {
                Vector3 playerPosition = players
                               .OrderBy(player => Vector3.Distance(transform.position, player.transform.position))
                               .First()
                               .transform.position;

                if (!stoppedDrone)
                {
                    agent.ResetPath();
                    StopAllCoroutines();
                    isWandering = true;
                    stoppedDrone = true;
                }

                if (!doingShootingPattern)
                {
                    int randomPattern = Random.Range(0, 1);
                    if (randomPattern == 0)
                    {
                        StartCoroutine(OffenseDronePatternOne(playerPosition));
                    }
                    else
                    {
                        //StartCoroutine(OffenseDronePatternTwo(playerPosition));
                    }
                }

                Quaternion rRot = Quaternion.LookRotation(playerPosition - BodyMesh.transform.position);
                BodyMesh.transform.rotation = Quaternion.Slerp(BodyMesh.transform.rotation, rRot, Time.deltaTime * 10);
            }
            else if (!doingShootingPattern)
            {
                if (stoppedDrone && isWandering)
                {
                    StartCoroutine(Wander());
                    stoppedDrone = false;
                }
            }
        }
        else if (Small || Medium)
        {
            if (Vector3.Distance(transform.position, player.position) <= AvoidPlayerDistance)
            {
                if (GunObject)
                {
                    GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;

                    Quaternion targetRotation = Quaternion.LookRotation(
                        (player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy))) - GunObjectExitPoint.transform.position
                    );

                    float rotationSpeed = 90;
                    GunObjectExitPoint.transform.rotation = Quaternion.Slerp(
                        GunObjectExitPoint.transform.rotation,
                        targetRotation,
                        Time.deltaTime * rotationSpeed
                    );
                }

                Vector3 direction = player.position - transform.position;
                direction.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), RotationSpeed * Time.deltaTime);
            }
            else
            {
                if (GunObject)
                {
                    GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;

                    Quaternion targetRotation = Quaternion.LookRotation(
                        (player.position + new Vector3(Random.Range(-GunInaccuracy, GunInaccuracy), 1.5f, Random.Range(-GunInaccuracy, GunInaccuracy))) - GunObjectExitPoint.transform.position
                    );

                    float rotationSpeed = 90;
                    GunObjectExitPoint.transform.rotation = Quaternion.Slerp(
                        GunObjectExitPoint.transform.rotation,
                        targetRotation,
                        Time.deltaTime * rotationSpeed
                    );
                }

                Vector3 offset = (transform.position - player.position).normalized * AvoidPlayerDistance;
                agent.SetDestination(player.position + offset);
            }
        }
    }

    IEnumerator MoveBodyMesh(Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0;
        Vector3 initialPosition = BodyMesh.transform.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            BodyMesh.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            yield return null;
        }
    }

    IEnumerator OffenseDronePatternOne(Vector3 playerPosition)
    {
        doingShootingPattern = true;

        for (int i = 0; i < 4; i++)
        {
            moveUp = !moveUp;

            Vector3 currentPosition = transform.position;
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.Normalize();

            Vector3 targetPosition = currentPosition + randomDirection * 4;

            agent.SetDestination(targetPosition);

            timeDroneMove = agent.remainingDistance / agent.speed;

            if (moveUp)
            {
                newPosition = new Vector3(BodyMesh.transform.position.x, BodyMesh.transform.position.y + 1.5f, BodyMesh.transform.position.z);
            }
            else
            {
                newPosition = new Vector3(BodyMesh.transform.position.x, BodyMesh.transform.position.y - 1.5f, BodyMesh.transform.position.z);
            }

            newYPosition = newPosition.y;

            moveDroneUpOrDown = true;

            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.1f);

            moveDroneUpOrDown = false;
        }

        agent.ResetPath();
        yield return new WaitForSeconds(2);

        doingShootingPattern = false;
    }


    //IEnumerator OffenseDronePatternTwo(Vector3 playerPosition)
    //{
    //    // same behaviour for now
    //}

    protected override void Update()
    {
        playerInSight = CheckPlayerVisibility();

        base.Update();

        if (!isInView)
        {
            if (isHoldingGun && playerInSight)
            {
                if (!DefenseDrone && !OffenseDrone)
                {
                    if (Time.time - lastShotTime >= 1 / EnemyFireRate)
                    {
                        for (int i = 0; i < GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.burstSize; i++)
                            Invoke(nameof(EnemyShoot), i * 1 / GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.autoRate);

                        lastShotTime = Time.time;
                    }
                }
            }
        }

        if (isHoldingGun)
        {
            if (agent != null && (playerInSight || wasHit))
            {
                EnemyMovement();
            }
        }
        else if (!isHoldingGun && (DefenseDrone || OffenseDrone))
        {
            EnemyMovement();
        }
        else
        {
            EnemyBehaviour();
        }

        if (!doingShootingPattern)
        {
            if (DefenseDrone || OffenseDrone)
            {
                t = Mathf.PingPong((Time.time - startTime) / duration, 1);
                newYPosition = Mathf.Lerp(initialPositionY - verticalOffset, initialPositionY + verticalOffset, t);
                newPosition = new Vector3(BodyMesh.transform.position.x, newYPosition, BodyMesh.transform.position.z);
                BodyMesh.transform.position = newPosition;
            }
        }

        if (moveDroneUpOrDown)
        {
            Vector3 goTo = new Vector3(BodyMesh.transform.position.x, newYPosition, BodyMesh.transform.position.z);
            Vector3 lerpPosition = Vector3.Lerp(BodyMesh.transform.position, goTo, Time.deltaTime / timeDroneMove);
            BodyMesh.transform.position = lerpPosition;
        }
    }

    void EnemyShoot()
    {
        if (!isHoldingGun)
            return;

        isShooting = true;

        for (int i = 0; i < GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.projectiles; i++)
            SpawnBullet();
    }

    void SpawnBullet()
    {
        GameObject GunObjectExitPoint = GunObject.transform.GetChild(0).gameObject;

        GameObject bullet = Instantiate(GunObject.GetComponent<EnemyGunInfo>().BulletPrefab, GunObjectExitPoint.transform.position, GunObjectExitPoint.transform.rotation);
        bullet.transform.parent = gameObject.transform;

        Vector3 direction = GunObjectExitPoint.transform.forward;
        direction += SpreadDirection(GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.spread, 3);

        bullet.transform.position = GunObjectExitPoint.transform.position;
        bullet.transform.rotation = Quaternion.LookRotation(direction.normalized);

        EnemyBullet b = bullet.GetComponent<EnemyBullet>();
        b.BulletSpeed = GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.bulletSpeed;
        b.BulletGravity = GunObject.GetComponent<EnemyGunInfo>().GunAssetInfo.bulletGravity;
        isShooting = false;
    }

    Vector3 SpreadDirection(float spread, int rolls)
    {
        Vector3 offset = new();
        for (int i = 0; i < rolls; i++) offset += Random.onUnitSphere * spread;
        return offset / rolls;
    }
}