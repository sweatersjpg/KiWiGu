using UnityEngine;

public class EnemyHitboxRegister : MonoBehaviour
{
    public EnemyBase enemyBase;

    public class HitboxScript
    {
        [Header("Hitbox")]
        public bool CheckIfHitboxScript;
        public EnemyBase enemyBehaviour;
    }
}
