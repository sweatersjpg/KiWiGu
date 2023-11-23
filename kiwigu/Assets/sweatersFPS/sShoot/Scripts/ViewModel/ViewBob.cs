using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewBob : MonoBehaviour
{
    float time = 0;

    public float timeScale;
    public Vector3 delta;
    public Vector3 rotationDelta;

    public Vector3 velocity;

    Vector3 targetPosition;

    Vector3 targetRotation;
    Vector3 rotation;

    int hand = -1;

    sweatersController player;

    public WeaponCameraFX cameraFX;

    // Start is called before the first frame update
    void Start()
    {
        if (transform.parent.localPosition.x > 0) hand = 1;
        else hand = -1;

        player = sweatersController.instance;
    }

    // Update is called once per frame
    void Update()
    {

        velocity = player.velocity;

        if (!PauseSystem.paused) time += Time.deltaTime * new Vector2(velocity.x, velocity.z).magnitude;

        float Y = Mathf.Sin(time * timeScale) * delta.y;
        float X = Mathf.Sin(time * timeScale) * delta.x;
        float Z = Mathf.Sin(time * timeScale) * delta.z;

        if (velocity.magnitude > 0.5f) targetPosition = new(-X, Y * hand, Z * hand);
        else targetPosition = new();

        if (!player.isGrounded) targetPosition = new(0, velocity.y * -0.01f, 0);

        targetRotation = new(0, 0, Vector3.Dot(velocity, player.transform.right) * rotationDelta.z);

        if (Mathf.Abs(transform.parent.localPosition.x) < 0.01)
        {
            targetPosition = 0.1f * targetPosition.magnitude * targetPosition.normalized;
            targetRotation = 0.2f * targetRotation.magnitude * targetRotation.normalized;
        }

        transform.localPosition += (targetPosition - transform.localPosition) / 4 * Time.deltaTime * 50;

        rotation += (targetRotation - rotation) / 4 * Time.deltaTime * 50;

        transform.localEulerAngles = rotation;
        
    }
}
