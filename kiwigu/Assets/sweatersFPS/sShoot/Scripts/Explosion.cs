using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] float force = 10;
    [SerializeField] float duration;
    [SerializeField] float finalRadius;
    [SerializeField] AnimationCurve explosionSize;

    [SerializeField] float damageDealt = 20;

    [SerializeField] Transform explosionFX;

    List<GameObject> alreadyHit;

    [SerializeField] bool scaleExplosion = true;

    float startTime;
    public string explosionType;

    // Start is called before the first frame update
    void Start()
    {
        GlobalAudioManager.instance.PlayExplosion(transform, explosionType);
        startTime = Time.time;

        if(explosionFX)
        {
            explosionFX.parent = null;
            if(scaleExplosion) explosionFX.localScale = new(finalRadius / 2, finalRadius / 2, finalRadius / 2);
        }

        alreadyHit = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.time - startTime;

        float scale = explosionSize.Evaluate(time / duration) * finalRadius;
        transform.localScale = new(scale, scale, scale);

        CheckRadius(scale / 2);

        if (time > duration) Destroy(gameObject);
    }

    void CheckRadius(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        // prioritize shields
        foreach (Collider hit in hits)
        {
            if (alreadyHit.Contains(hit.gameObject)) return;

            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Shield") && hit.transform.gameObject.CompareTag("Shield"))
            {
                EnemyHitBox enemy = hit.transform.gameObject.GetComponent<EnemyHitBox>();

                if (enemy != null)
                {
                    if (enemy.isShield)
                        ShieldDamage(enemy);
                }
            }
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Shield") && hit.transform.gameObject.CompareTag("Armor"))
            {
                alreadyHit.Add(hit.gameObject);

                ArmorPiece armor = hit.transform.gameObject.GetComponent<ArmorPiece>();

                GlobalAudioManager.instance.PlayBulletHit(hit.transform, "Armor");

                armor.Hit(damageDealt);
            }
        }

        foreach (Collider hit in hits)
        {
            if (alreadyHit.Contains(hit.gameObject)) return;

            if (hit.transform.CompareTag("TakeDamage"))
            {
                Vector3 direction = (transform.position - hit.transform.position);
                hit.transform.gameObject.SendMessageUpwards("TakeDamage",
                    new object[] { hit.ClosestPoint(transform.position), direction, damageDealt });

                alreadyHit.Add(hit.gameObject);
            }
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("EnergyWall"))
            {
                // if behind shield, pass through otherwise deal damage
                Vector3 direction = (transform.position - hit.transform.position);
                hit.transform.gameObject.SendMessageUpwards("TakeDamage",
                        new object[] { hit.ClosestPoint(transform.position), direction, damageDealt });

                alreadyHit.Add(hit.gameObject);
            }
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                // rocket jumping uwu
                Vector3 direction = (transform.position - hit.transform.position);
                hit.transform.GetComponent<PlayerHealth>().DealDamage(damageDealt / 2, direction.normalized); // half damage

                direction.Normalize();
                // Vector3 launchDirection = sweatersController.instance.playerCamera.transform.forward.normalized;
                Vector3 launch = new Vector3(direction.x * 10, -Mathf.Abs(direction.y * 20), direction.z * 10);

                if (sweatersController.instance.velocity.y < 0) sweatersController.instance.velocity.y *= -0.5f;
                sweatersController.instance.velocity -= launch;
                sweatersController.instance.maxSpeed = Mathf.Max(sweatersController.instance.airSpeed / 2, sweatersController.instance.maxSpeed);

                alreadyHit.Add(hit.gameObject);
            }
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy") && !hit.transform.gameObject.CompareTag("Backpack"))
            {
                EnemyHitBox enemy = hit.transform.gameObject.GetComponent<EnemyHitBox>();

                if (enemy != null)
                {
                    if (enemy.doubleDamage)
                        ApplyDamage(enemy, 2f, true);
                    else if (enemy.chestDamage)
                        ApplyDamage(enemy, 1.5f, false);
                    else if (enemy.leastDamage)
                        ApplyDamage(enemy, 0.75f, false);
                    else
                        ApplyDamage(enemy, 1f, false); ;
                }
            }
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Backpack") && hit.transform.gameObject.CompareTag("Backpack"))
            {
                EnemyHitBox enemy = hit.transform.gameObject.GetComponent<EnemyHitBox>();

                if (enemy != null)
                {
                    if (enemy.isBackpack)
                        BackpackDamage(enemy);
                }
            }
            else if (hit.attachedRigidbody != null)
            {
                alreadyHit.Add(hit.gameObject);

                Vector3 direction = (transform.position - hit.transform.position);
                //hit.attachedRigidbody.AddExplosionForce(force, transform.position, radius);
                hit.attachedRigidbody.AddForce(-force * direction.normalized, ForceMode.Impulse);
                // print(hit.name);
            }

        }
    }

    private Transform GetRootParent(Transform child)
    {
        Transform parent = child.parent;

        while (parent != null)
        {
            child = parent;
            parent = child.parent;
        }

        return child;
    }

    private void ApplyDamage(EnemyHitBox enemy, float damageMultiplier, bool isHeadshot)
    {
        if (enemy == null)
            return;

        var scriptType = System.Type.GetType(enemy.ReferenceScript);

        Transform rootParent = GetRootParent(enemy.transform);

        if (rootParent != null && scriptType != null)
        {
            if (alreadyHit.Contains(rootParent.gameObject)) return;
            alreadyHit.Add(rootParent.gameObject);

            var enemyComponent = rootParent.GetComponent(scriptType) as MonoBehaviour;

            if (enemyComponent != null)
            {
                var takeDamageMethod = scriptType.GetMethod("TakeDamage");

                if (takeDamageMethod != null)
                {
                    if (isHeadshot)
                        takeDamageMethod.Invoke(enemyComponent, new object[] { damageDealt * damageMultiplier, true });
                    else
                        takeDamageMethod.Invoke(enemyComponent, new object[] { damageDealt * damageMultiplier, false });
                }
            }
        }
    }

    private void BackpackDamage(EnemyHitBox enemy)
    {
        if (enemy == null)
            return;

        var scriptType = System.Type.GetType(enemy.ReferenceScript);

        Transform rootParent = GetRootParent(enemy.transform);

        if (rootParent != null && scriptType != null)
        {
            if (alreadyHit.Contains(rootParent.gameObject)) return;
            alreadyHit.Add(rootParent.gameObject);

            var enemyComponent = rootParent.GetComponent(scriptType) as MonoBehaviour;

            if (enemyComponent != null)
            {
                var takeDamageMethod = scriptType.GetMethod("BackpackDamage");

                if (takeDamageMethod != null)
                {
                    takeDamageMethod.Invoke(enemyComponent, new object[] { damageDealt });
                }
            }
        }
    }

    private void ShieldDamage(EnemyHitBox enemy)
    {
        if (enemy == null)
            return;

        var scriptType = System.Type.GetType(enemy.ReferenceScript);

        Transform rootParent = GetRootParent(enemy.transform);

        if (rootParent != null && scriptType != null)
        {
            if (alreadyHit.Contains(rootParent.gameObject)) return;
            alreadyHit.Add(rootParent.gameObject);

            var enemyComponent = rootParent.GetComponent(scriptType) as MonoBehaviour;

            if (enemyComponent != null)
            {
                var takeDamageMethod = scriptType.GetMethod("ShieldDamage");

                if (takeDamageMethod != null)
                {
                    takeDamageMethod.Invoke(enemyComponent, new object[] { damageDealt });
                }
            }
        }
    }

}
