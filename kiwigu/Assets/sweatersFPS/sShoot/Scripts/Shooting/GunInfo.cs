using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GunInfo", menuName = "GunInfo")]
public class GunInfo : ScriptableObject
{
    [Header("Gun Type")]
    public string gunName = "";

    [Header("Aiming")]
    public bool canAim = true;
    public bool hasToAim = false;
    public float aimDelay = 0.3f;
    public float aimTime = 0.3f;
    public float scopeFOV = 30;

    [Header("Ammo")]
    public float capacity = 100;
    [Space]
    public bool recharge = false;
    public float rechargeRate = 5;

    [Header("Charge")]
    public bool canCharge = false;
    public float timeToMaxCharge = 1;
    public AnimationCurve chargeCurve;

    [Header("Shooting")]
    public bool fullAuto = false;
    public float projectiles = 1;

    [Space]
    public float burstSize = 1;
    public float autoRate = 1;
    public float fireRate = 1;

    [Header("Damage")]
    public float damage;

    [Header("Metrics")]
    public float spread = 0; // works also as accuracy
    public float recoil = 45;
    [Space]
    public AnimationCurve spreadVariation;
    public AnimationCurve cameraRecoil;
    public float recoilPerShot = 0.1f;
    public float recoilReturnTime = 1f;
    [Space]
    public float bulletSpeed = 370;
    public float bulletGravity = -9.8f;

    [Header("Prefabs")]
    public GameObject bulletPrefab;
    public GameObject gunPrefab;

    public GameObject guUI;

}
