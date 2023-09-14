using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GunInfo", menuName = "GunInfo")]
public class GunInfo : ScriptableObject
{
    public enum FireType {Burst, Single, Auto}

    [Header("Aiming")]
    public bool canAim = true;
    public bool hasToAim = false;
    public float aimDelay = 0.3f;
    public float aimTime = 0.3f;

    [Header("Shooting")]
    public FireType fireType = FireType.Single;
    public float projectiles = 1;

    [Space]
    public float burstSize = 1;
    public float autoRate = 1;
    public float fireRate = 1;

    [Header("Metrics")]
    public float spread = 0; // works also as accuracy
    public float recoil = 45;

    public float bulletSpeed = 370;
    public float bulletGravity = -9.8f;
    public GameObject bulletPrefab;
}
