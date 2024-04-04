using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ArmorPiece : MonoBehaviour
{
    public GameObject breakSFX;
    public float armorHealth;
    public float armorHealthCurrent;

    public bool hookBreakable;
    public Mechemy refScriptMech;
    public bool isLeft;
    public bool isRight;

    private Material breakableMat;

    private void Awake()
    {
        breakableMat = GetComponent<SkinnedMeshRenderer>().material;
    }

    public void Hit(float damage)
    {
        armorHealthCurrent += damage;
        if (armorHealthCurrent >= armorHealth)
        {
            if (hookBreakable)
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

            Instantiate(breakSFX, GetComponent<SkinnedMeshRenderer>().bounds.center, Quaternion.identity);
            Destroy(gameObject);
        }

        breakableMat.SetFloat("_DamagePercent", armorHealthCurrent / armorHealth);
    }
}
