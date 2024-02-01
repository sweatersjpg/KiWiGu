using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] float force = 10;
    [SerializeField] float duration;
    [SerializeField] float finalRadius;
    [SerializeField] AnimationCurve explosionSize;

    [SerializeField] float damageDealt = 20;

    [SerializeField] Transform explosionFX;

    List<Collider> alreadyHit;

    float startTime;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;

        if(explosionFX)
        {
            explosionFX.parent = null;
            explosionFX.localScale = new(finalRadius / 2, finalRadius / 2, finalRadius / 2);
        }

        alreadyHit = new List<Collider>();
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

        foreach (Collider hit in hits)
        {
            if (alreadyHit.Contains(hit)) return;

            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Vector3 direction = (transform.position - hit.transform.position);
                hit.transform.GetComponent<PlayerHealth>().DealDamage(damageDealt, direction.normalized);

                sweatersController.instance.velocity -= direction.normalized * 20;

                alreadyHit.Add(hit);
            }
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
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
                }
            }
            else if (hit.attachedRigidbody != null)
            {
                alreadyHit.Add(hit);

                //hit.attachedRigidbody.AddExplosionForce(force, transform.position, radius);
                hit.attachedRigidbody.AddForce(force * (hit.transform.position - transform.position).normalized, ForceMode.Impulse);
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
