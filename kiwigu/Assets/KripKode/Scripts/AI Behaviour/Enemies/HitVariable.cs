using System.Collections;
using UnityEngine;
using static EnemyBase;

public class HitVariable : MonoBehaviour
{
    public bool wasHit;
    public PistolGrunt PistolGrunt;

    public void ShootEvent()
    {
        PistolGrunt.EnemyShoot();
    }
}
