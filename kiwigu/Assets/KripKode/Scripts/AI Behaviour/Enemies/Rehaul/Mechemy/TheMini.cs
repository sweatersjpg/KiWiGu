//using System.Collections;
//using UnityEngine;
//using UnityEngine.AI;
//using static MiniMenuSystem;

//public class TheMini : MonoBehaviour
//{
//    [Header("Mechemy Basic Settings")]
//    [Range(0, 500)]
//    [SerializeField] private float health;
//    [SerializeField] private Transform spineBone;

//    [Space(10)]
//    [Header("Enemy Detection Settings")]
//    [SerializeField] private Transform eyesPosition;
//    [SerializeField] private float detectionRange;
//    private GameObject detectedPlayer;

//    private void Update()
//    {
//        if (IsPlayerVisible())
//        {
//            ShootState();
//        }
//    }

//    private void LateUpdate()
//    {
//        if (!detectedPlayer)
//            return;

//        Vector3 directionToPlayer = detectedPlayer.transform.position - transform.position;
//        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

//        if (angleToPlayer <= 90)
//        {
//            spineBone.LookAt(detectedPlayer.transform.position);
//        }
//        else if (detectedPlayer && Vector3.Distance(transform.position, detectedPlayer.transform.position) > detectionRange)
//        {
//            spineBone.rotation = Quaternion.Euler(Vector3.zero);
//        }
//    }

//    private void ShootState()
//    {
//        RotateNavMeshAgentTowardsObj(detectedPlayer.transform.position);

//        if (isShooting)
//            return;
//        StartCoroutine(ShootRoutine());
//    }

//    private void RotateNavMeshAgentTowardsObj(Vector3 objPos)
//    {
//        Quaternion targetRotation = Quaternion.LookRotation(objPos - transform.position);

//       transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 1.75f);

//        RotateGunObjectExitPoint(detectedPlayer.transform.position);
//    }

//    private void RotateGunObjectExitPoint(Vector3 playerPosition)
//    {
//        if (!BulletExitPoint)
//            return;

//        Vector3 targetPosition = new Vector3(playerPosition.x, playerPosition.y + 1f, playerPosition.z);
//        Vector3 direction = targetPosition - BulletExitPoint.transform.position;

//        Quaternion targetRotation = Quaternion.LookRotation(direction);

//        if (isRotating)
//        {
//            currentRotationTime += Time.deltaTime;
//            float t = Mathf.Clamp01(currentRotationTime / maxRotationTime);

//            if (infoHT && infoHT.gunName == "EGL")
//            {
//                if (!angleCalculated)
//                {
//                    float distance = direction.magnitude;
//                    float time = distance / infoHT.bulletSpeed;
//                    float gravity = infoHT.bulletGravity;
//                    angle = Mathf.Atan((time * time * gravity) / (2 * distance)) * Mathf.Rad2Deg;
//                    angle *= Random.Range(0.35f, 0.75f);
//                    angleCalculated = true;
//                    angleCalculated = true;
//                }

//                BulletExitPoint.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
//                BulletExitPoint.transform.Rotate(Vector3.right, angle);
//            }
//            else
//            {
//                BulletExitPoint.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
//            }
//            if (currentRotationTime >= maxRotationTime)
//            {
//                isRotating = false;
//            }
//        }
//        else
//        {
//            startRotation = BulletExitPoint.transform.rotation;
//            currentRotationTime = 0f;
//            isRotating = true;
//            angleCalculated = false;
//        }
//    }

//    private float EnemyShoot()
//    {
//        if (!detectedPlayer)
//            return 0;

//        HookTarget gun = transform.GetComponentInChildren<HookTarget>();
//        if (gun == null)
//        {
//            isShooting = false;
//            return 0;
//        }

//        infoHT = gun.info;

//        if (gun)
//        {
//            if (holdingRightGun && holdingLeftGun)
//            {
//                if (Random.Range(0, 2) == 0)
//                {
//                    infoHT = rightGun.info;
//                    BulletExitPoint = rightGunExitPoint;
//                }
//                else
//                {
//                    infoHT = leftGun.info;
//                    BulletExitPoint = leftGunExitPoint;
//                }
//            }
//            else if (holdingRightGun)
//            {
//                infoHT = rightGun.info;
//                BulletExitPoint = rightGunExitPoint;
//            }
//            else if (holdingLeftGun)
//            {
//                infoHT = leftGun.info;
//                BulletExitPoint = leftGunExitPoint;
//            }
//        }

//        float burst = infoHT.burstSize;
//        if (infoHT.fullAuto) burst = infoHT.autoRate;

//        BulletShooter bs = BulletExitPoint.GetComponentInChildren<BulletShooter>();

//        if (bs) bs.info = infoHT;

//        if (bs)
//        {
//            bs.SetShootTime(1.15f);
//        }

//        return burst * 1 / infoHT.autoRate;
//    }

//    private bool IsPlayerVisible()
//    {
//        Collider[] hitColliders = Physics.OverlapSphere(eyesPosition.position, detectionRange, LayerMask.GetMask("Player"));
//        int layerMask = LayerMask.GetMask("Enemy");
//        int layerMask2 = LayerMask.GetMask("HookTarget");
//        int layerMask3 = LayerMask.GetMask("Shield");
//        int layerMask4 = LayerMask.GetMask("GunHand");
//        int layerMask5 = LayerMask.GetMask("EnergyWall");
//        int combinedLayerMask = layerMask | layerMask2 | layerMask3 | layerMask4 | layerMask5;


//        foreach (Collider hitCollider in hitColliders)
//        {
//            RaycastHit hit;
//            if (Physics.Raycast(eyesPosition.position, hitCollider.transform.position - eyesPosition.position - new Vector3(0, -1, 0), out hit, detectionRange, ~combinedLayerMask))
//            {
//                if (hit.collider.CompareTag("Player"))
//                {
//                    detectedPlayer = hit.collider.gameObject;
//                    return true;
//                }
//            }
//        }
//        return false;
//    }
//}