using UnityEngine;

public class ArmorPiece : MonoBehaviour
{
    public GameObject breakSFX;
    public float armorHealth;
    private Vector3 pos;

    private void Awake()
    {
        pos = GetComponent<SkinnedMeshRenderer>().bounds.center;
    }

    public void Hit(float damage)
    {
        armorHealth -= damage;
        if (armorHealth <= 0)
        {
            Instantiate(breakSFX, pos, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
