using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public class DirectionalAttack : MonoBehaviour
{
    public Transform target;

    [SerializeField] float force = 10;
    [SerializeField] float radius = 2;
    [SerializeField] float duration;
    [SerializeField] float damageDealt = 20;

    [SerializeField] GameObject hitEffect;

    public List<GameObject> ignoreList;

    List<GameObject> alreadyHit;

    float startTime;

    Vector3 startPos;

    Collider currentHit;

    // Start is called before the first frame update
    void Start()
    {
        alreadyHit = new List<GameObject>();
        startTime = Time.time;

        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        CheckRadius(radius);

        Vector3 pos = startPos;
        if (transform.parent) pos = transform.parent.position;

        transform.position = Vector3.Lerp(pos, target.position, (Time.time - startTime) / duration);

        if (Time.time - startTime > duration)
        {
            if (!transform.parent) Destroy(target.gameObject);
            Destroy(gameObject);
        }

    }

    void CheckRadius(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, 
            LayerMask.GetMask("Enemy", "PhysicsObject", "Shield", "EnergyWall", "Default"));

        // prioritize shields
        foreach (Collider hit in hits)
        {
            if (ignoreList.Contains(hit.gameObject)) continue;
            currentHit = hit;

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
                Transform root = GetRootParent(hit.transform);

                if (alreadyHit.Contains(root.gameObject)) return;
                alreadyHit.Add(root.gameObject);

                ArmorPiece armor = hit.transform.gameObject.GetComponent<ArmorPiece>();

                GlobalAudioManager.instance.PlayBulletHit(hit.transform, "Armor");

                armor.Hit(damageDealt);
            }
        }

        foreach (Collider hit in hits)
        {
            if (ignoreList.Contains(hit.gameObject)) continue;
            currentHit = hit;

            if (hit.transform.CompareTag("TakeDamage"))
            {
                Vector3 direction = transform.forward;
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
                if (alreadyHit.Contains(hit.gameObject)) return;
                alreadyHit.Add(hit.gameObject);

                //hit.attachedRigidbody.AddExplosionForce(force, transform.position, radius);
                hit.attachedRigidbody.AddForce(sweatersController.instance.velocity, ForceMode.VelocityChange);
                hit.attachedRigidbody.AddForce(force * transform.forward, ForceMode.Impulse);

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

            GlobalAudioManager.instance.PlayBulletHit(transform, "Flesh");

            Instantiate(hitEffect, currentHit.ClosestPoint(transform.position), Quaternion.identity);

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

            GlobalAudioManager.instance.PlayBulletHit(transform, "Armor");

            Instantiate(hitEffect, currentHit.ClosestPoint(transform.position), Quaternion.identity);

            var enemyComponent = rootParent.GetComponent(scriptType) as MonoBehaviour;

            if (enemyComponent != null)
            {
                var takeDamageMethod = scriptType.GetMethod("BackpackDamage");

                takeDamageMethod?.Invoke(enemyComponent, new object[] { damageDealt });
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

            GlobalAudioManager.instance.PlayBulletHit(transform, "Armor");

            Instantiate(hitEffect, currentHit.ClosestPoint(transform.position), Quaternion.identity);

            var enemyComponent = rootParent.GetComponent(scriptType) as MonoBehaviour;

            if (enemyComponent != null)
            {
                var takeDamageMethod = scriptType.GetMethod("ShieldDamage");

                takeDamageMethod?.Invoke(enemyComponent, new object[] { damageDealt * 0.5f });
            }
        }
    }
}
