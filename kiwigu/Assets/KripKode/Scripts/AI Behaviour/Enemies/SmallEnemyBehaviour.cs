using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SmallEnemyBehaviour : EnemyBase
{
    protected override void Update()
    {
        isPlayerVisible = CheckPlayerVisibility();

        base.Update();

        EnemyMovement();
    }

    public void EnemyMovement()
    {
        HandleRegularEnemyMovement();
    }

    private void HandleRegularEnemyMovement()
    {
        if (isPlayerVisible)
        {
            MoveAroundCover();
        }
    }

    private void MoveAroundCover()
    {
        GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");
        GameObject nearestCover = null;
        float minDistance = float.MaxValue;

        foreach (GameObject coverObject in coverObjects)
        {
            float distance = Vector3.Distance(transform.position, coverObject.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCover = coverObject;
            }
        }

        if (nearestCover != null)
        {
            Vector3 directionToPlayer = transform.position - playerPosition;
            Vector3 oppositePoint = nearestCover.transform.position + directionToPlayer.normalized * minDistance;

            agent.SetDestination(oppositePoint);
        }
    }


    // Maybe later
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
}
