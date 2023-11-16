using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SmallEnemyBehaviour : EnemyBase
{
    protected override void Update()
    {
        playerInSight = CheckPlayerVisibility();

        base.Update();

        EnemyMovement();
    }

    public void EnemyMovement()
    {
        HandleRegularEnemyMovement();
    }

    private void HandleRegularEnemyMovement()
    {
        if (Vector3.Distance(transform.position, playerPosition) <= enemyMovementVariables.AvoidPlayerDistance)
        {
            RotateGunAndBodyTowardsPlayer();
        }
        else
        {
            Vector3 offset = (transform.position - playerPosition).normalized * enemyMovementVariables.AvoidPlayerDistance;
            agent.SetDestination(playerPosition + offset);
        }
    }

    private void RotateGunAndBodyTowardsPlayer()
    {
        if (!canFacePlayer) return;

        RotateBodyMeshTowardsObj(playerPosition);
        RotateGunObjectExitPoint(playerPosition);
    }

    private void RotateBodyMeshTowardsObj(Vector3 objPos)
    {
        if (!canFacePlayer) return;

        Quaternion rRot = Quaternion.LookRotation(objPos - enemyMainVariables.BodyMesh.transform.position);
        enemyMainVariables.BodyMesh.transform.rotation = Quaternion.Slerp(enemyMainVariables.BodyMesh.transform.rotation, rRot, Time.deltaTime * 10);
    }

    private void RotateGunObjectExitPoint(Vector3 rotPos)
    {
        if (!canFacePlayer) return;

        gunObjectExitPoint = enemyMainVariables.GunObject.transform.GetChild(0).gameObject;

        Quaternion targetRotation = Quaternion.LookRotation(
            (rotPos + new Vector3(Random.Range(-enemyGunStats.GunInaccuracy, enemyGunStats.GunInaccuracy), 1.5f, Random.Range(-enemyGunStats.GunInaccuracy, enemyGunStats.GunInaccuracy))) - gunObjectExitPoint.transform.position
        );

        gunObjectExitPoint.transform.rotation = Quaternion.Slerp(
            gunObjectExitPoint.transform.rotation,
            targetRotation,
            Time.deltaTime * enemyGunStats.gunExitPointRotationSpeed
        );
    }


    ////////////////////////////////////////////
    
    private GameObject FindClosestStationWithTag(string tag)
    {
        GameObject[] stations = GameObject.FindGameObjectsWithTag(tag);
        GameObject closestStation = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject station in stations)
        {
            float distance = Vector3.Distance(transform.position, station.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestStation = station;
            }
        }

        return closestStation;
    }

    private bool IsAgentCloseToStation()
    {
        GameObject[] stations = GameObject.FindGameObjectsWithTag("EnemyRestockStation");

        foreach (GameObject station in stations)
        {
            float distance = Vector3.Distance(transform.position, station.transform.position);

            if (distance <= 2)
            {
                return true;
            }
        }

        return false;
    }

    public void EnemyBehaviour()
    {
        if (enemyMainVariables.GunObject)
        {
            HandleGunLogic();
        }
        else
        {
            HandleNoGunLogic();
        }
    }

    private void HandleGunLogic()
    {
        if (IsAgentCloseToStation())
        {
            isHoldingGun = true;
        }
        else
        {
            HandleGunSeeking();
        }
    }

    private void HandleGunSeeking()
    {
        if (enemyMainVariables.canSeekGun)
        {
            GameObject closestStation = FindClosestStationWithTag("EnemyRestockStation");

            if (closestStation != null)
            {
                agent.SetDestination(closestStation.transform.position);
            }
        }
        else
        {
            if (agent != null && (playerInSight || wasHit))
            {
                if (!startedFleeing)
                {
                    StartCoroutine(EnemyFlee());
                }
            }
            else if (!isWandering)
            {
                StartCoroutine(Wander());
            }
        }
    }

    private void HandleNoGunLogic()
    {
        if (agent != null && (playerInSight || wasHit))
        {
            if (!startedFleeing)
            {
                StartCoroutine(EnemyFlee());
            }
        }
        else if (!isWandering)
        {
            StartCoroutine(Wander());
        }
    }

    private IEnumerator EnemyFlee()
    {
        Vector3 fleeDestination = FindFleeDestination();

        agent.SetDestination(fleeDestination);

        yield return new WaitUntil(() => agent.remainingDistance < 0.5f);

        startedFleeing = false;
    }

    private Vector3 FindFleeDestination()
    {
        Vector3 awayFromPlayer = transform.position - playerPosition;
        awayFromPlayer.Normalize();

        Vector3 fleeDestination = transform.position + awayFromPlayer * enemyMovementVariables.FleeDistance;

        float randomOffsetX = Mathf.PerlinNoise(Time.time, 0) * 2 - 1;
        float randomOffsetZ = Mathf.PerlinNoise(0, Time.time) * 2 - 1;
        Vector3 randomOffset = new Vector3(randomOffsetX, 0f, randomOffsetZ) * enemyMovementVariables.FleeMovementVariation;
        fleeDestination += randomOffset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeDestination, out hit, enemyMovementVariables.FleeDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return transform.position;
        }
    }
}
