using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OneWayCollider : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Vector3 entryDirection = Vector3.up;
    [SerializeField] private bool localDirection = false;

    private BoxCollider mainCollider = null;
    private BoxCollider collisionCheckTrigger = null;

    public Vector3 PassthroughDirection
    {
        get
        {
            return localDirection
                ? transform.TransformDirection(entryDirection.normalized)
                : entryDirection.normalized;
        }
    }

    private void Awake()
    {
        mainCollider = GetComponent<BoxCollider>();
        CreateCollisionCheckTrigger();
    }

    private void OnValidate()
    {
        mainCollider = GetComponent<BoxCollider>();
        mainCollider.isTrigger = false;
    }

    private void CreateCollisionCheckTrigger()
    {
        collisionCheckTrigger = gameObject.AddComponent<BoxCollider>();
        collisionCheckTrigger.size = new Vector3(
            mainCollider.size.x + 4,
            mainCollider.size.y,
            mainCollider.size.z + 2
        );
        collisionCheckTrigger.center = mainCollider.center;
        collisionCheckTrigger.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        TryIgnoreCollision(other);
    }

    public void TryIgnoreCollision(Collider other)
    {
        if (Physics.ComputePenetration(
            collisionCheckTrigger, collisionCheckTrigger.bounds.center, transform.rotation,
            other, other.bounds.center, other.transform.rotation,
            out Vector3 collisionDirection, out float penetrationDepth))
        {
            float dot = Vector3.Dot(PassthroughDirection, collisionDirection);

            if (dot < 0)
            {
                if (penetrationDepth < 0.2f)
                {
                    Physics.IgnoreCollision(mainCollider, other, false);
                }
            }
            else
            {
                Physics.IgnoreCollision(mainCollider, other, true);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.TransformPoint(mainCollider.center), PassthroughDirection * 2);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.TransformPoint(mainCollider.center), -PassthroughDirection * 2);
    }
}
