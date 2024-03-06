using UnityEngine;

public class ArmorPiece : MonoBehaviour
{
    public GameObject breakSFX;
    public float armorHealth;
    private Vector3 pos;
    public bool hookBreakable;
    public Mechemy refScriptMech;
    public bool isLeft;
    public bool isRight;

    private void Awake()
    {
        pos = GetComponent<SkinnedMeshRenderer>().bounds.center;
    }

    public void Hit(float damage)
    {
        armorHealth -= damage;
        if (armorHealth <= 0)
        {
            if(hookBreakable)
            {
                if (isLeft)
                {
                    refScriptMech.leftGun.blockSteal = false;
                }
                if (isRight)
                {
                    refScriptMech.rightGun.blockSteal = false;
                }
            }

            Instantiate(breakSFX, pos, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
