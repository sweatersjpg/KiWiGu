using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ArmorPiece : MonoBehaviour
{
    public GameObject breakSFX;
    public float armorHealth;
    public float armorHealthCurrent;

    public bool hookBreakable;
    public Mechemy refScriptMech;
    public TheMini refScriptMini;
    public bool isLeft;
    public bool isRight;

    private Material breakableMat;
    public bool isTop;
    public CapsuleCollider[] PILOTCollider;
    public GameObject[] destroyOnBreak;

    public bool isMini;

    private void Awake()
    {
        if (GetComponent<SkinnedMeshRenderer>())
            breakableMat = GetComponent<SkinnedMeshRenderer>().material;
    }

    public void Hit(float damage)
    {
        armorHealthCurrent += damage;
        if (armorHealthCurrent >= armorHealth)
        {
            foreach (GameObject obj in destroyOnBreak)
            {
                Destroy(obj);
            }

            if (hookBreakable)
            {
                if (isLeft)
                {
                    if (isMini)
                    {
                        refScriptMini.leftGun.blockSteal = false;
                    }
                    else
                    {
                        refScriptMech.leftGun.blockSteal = false;
                    }
                }
                if (isRight)
                {
                    if (isMini)
                    {
                        refScriptMini.rightGun.blockSteal = false;
                    }
                    else
                    {
                        refScriptMech.rightGun.blockSteal = false;
                    }
                }
            }

            if (breakableMat != null)
                Instantiate(breakSFX, GetComponent<SkinnedMeshRenderer>().bounds.center, Quaternion.identity);
            else
                Instantiate(breakSFX, GetComponent<MeshRenderer>().bounds.center, Quaternion.identity);

            if (isTop)
            {
                foreach (CapsuleCollider col in PILOTCollider)
                {
                    col.enabled = true;
                }
            }

            Destroy(gameObject);
        }

        if (breakableMat != null)
            breakableMat.SetFloat("_DamagePercent", armorHealthCurrent / armorHealth);
    }
}
