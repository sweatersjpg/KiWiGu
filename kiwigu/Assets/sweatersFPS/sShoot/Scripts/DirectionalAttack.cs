using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalAttack : MonoBehaviour
{
    public Transform target;

    [SerializeField] float force = 10;
    [SerializeField] float radius = 2;
    [SerializeField] float duration;
    [SerializeField] float damageDealt = 20;

    [SerializeField] GameObject hitEffect;

    List<Collider> alreadyHit;

    float startTime;

    // Start is called before the first frame update
    void Start()
    {
        alreadyHit = new List<Collider>();
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        CheckRadius(radius);

        transform.position = Vector3.Lerp(transform.parent.position, target.position, (Time.time - startTime) / duration);

        if (Time.time - startTime > duration) Destroy(gameObject);
    }

    void CheckRadius(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hits)
        {
            if (alreadyHit.Contains(hit)) return;

            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                EnemyHitBox enemy = hit.transform.gameObject.GetComponentInParent<EnemyHitBox>();
                if (enemy != null)
                {
                    var scriptType = System.Type.GetType(enemy.ReferenceScript);

                    Transform rootParent = GetRootParent(enemy.transform);

                    if (rootParent != null)
                    {
                        var enemyComponent = rootParent.GetComponent(scriptType) as MonoBehaviour;

                        if (enemyComponent != null)
                        {
                            var takeDamageMethod = scriptType.GetMethod("TakeDamage");

                            if (takeDamageMethod != null)
                            {
                                takeDamageMethod.Invoke(enemyComponent, new object[] { damageDealt });
                            }
                        }
                    }
                    alreadyHit.Add(hit);

                    Instantiate(hitEffect, hit.ClosestPoint(transform.position), Quaternion.identity);
                }
            }
            else if (hit.attachedRigidbody != null)
            {
                alreadyHit.Add(hit);

                //hit.attachedRigidbody.AddExplosionForce(force, transform.position, radius);
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
}
