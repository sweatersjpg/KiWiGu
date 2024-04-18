using UnityEngine;

public class RippleFollower : MonoBehaviour
{
    public GameObject objectReference;
    public float rippleSpeed = 1f;
    public float rippleDistance = 1f;

    private Vector3 previousPosition;

    private void Start()
    {
        previousPosition = objectReference.transform.position;
    }
     
    private void Update()
    {
        if (objectReference == null)
            return;

        if (objectReference.transform.position.y > transform.position.y + 1)
            return;

        Vector3 velocity = (objectReference.transform.position - previousPosition) / Time.deltaTime;

        Vector3 targetPosition = objectReference.transform.position + velocity.normalized * rippleDistance;

        transform.position = Vector3.Lerp(transform.position, targetPosition, rippleSpeed * Time.deltaTime);

        previousPosition = objectReference.transform.position;
    }
}
