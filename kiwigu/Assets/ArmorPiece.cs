using UnityEngine;

public class ArmorPiece : MonoBehaviour
{
    public GameObject breakSFX;
    public float armorHealth;

    public void Hit(float damage)
    {
        armorHealth -= damage;
        if (armorHealth <= 0)
        {
            Instantiate(breakSFX, transform.localPosition, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
