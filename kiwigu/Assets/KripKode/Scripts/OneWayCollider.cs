using System.Collections;
using UnityEngine;

public class OneWayCollider : MonoBehaviour
{
    private Collider thisCollider;

    private void Start()
    {
        thisCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsMovingFromFront(other.transform))
        {
            Physics.IgnoreCollision(thisCollider, other, false);
        }
        else
        {
            Physics.IgnoreCollision(thisCollider, other, true);
        }
    }

    private bool IsMovingFromFront(Transform otherTransform)
    {
        Vector3 direction = otherTransform.position - transform.position;
        float dotProduct = Vector3.Dot(direction, transform.forward);

        return dotProduct > 0f;
    }
}
