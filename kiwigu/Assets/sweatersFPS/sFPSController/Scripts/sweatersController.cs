using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sweatersController : MonoBehaviour
{
    public static sweatersController instance;

    [HideInInspector]
    public CharacterController charController;

    public Camera playerCamera;
    //public static bool paused = false;

    [Header("Movement Metrics")]
    public float maxJumpDistance = 4;
    public float maxJumpHeight = 2;
    public float minJumpHeight = 0.5f;
    public float runningSpeed = 7.5f;
    public float airSpeed = 10;
    public float crouchSpeed = 3.75f;
    public float acceleration = 3.75f;
    [Space]
    public float slopeLimit = 45;
    public float deceleration = 4;
    public float airDeceleration = 1;
    public float maxSpeedDecay = 16;

    [Space]
    public float gravity = 20f;
    public float jumpSpeed = 8.0f;

    [Space]
    public float jumpBuffer = 0.2f;

    float maxJumpGravity;
    float minJumpGravity;

    float maxSpeed;

    [Space]
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    [Header("Player Metrics")]
    public float standingHeight = 2;
    public float crouchHeight = 1.5f;

    float targetHeight;

    [HideInInspector] public Vector2 mouseLook;
    public Vector3 velocity;
    public Vector3 input;

    float rotationX = 0;

    Vector3 hitPointNormal;
    Vector3 groundNormal;

    Vector3 spawnPoint;

    float deltaTime;

    bool jumpPressed = false;
    bool jumpJustReleased = false;

    public bool isSliding;
    public bool isGrounded;
    public bool isCrouching
    {
        get { return charController.height < standingHeight * 0.9f; }
    }

    private void Awake()
    {
        instance = this;

        velocity = new Vector3();



    }

    // Start is called before the first frame update
    void Start()
    {
        charController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        charController.height = standingHeight;
        targetHeight = charController.height;

        spawnPoint = transform.position;

        float a = (acceleration * 0.25f); // 1/2a

        float airTime = (-runningSpeed + Mathf.Sqrt((runningSpeed * runningSpeed) - 2 * a * (-maxJumpDistance))) / a;

        airTime /= 2;

        jumpSpeed = 2 * (maxJumpHeight / airTime);

        maxJumpGravity = jumpSpeed / airTime;

        gravity = maxJumpGravity;

        minJumpGravity = jumpSpeed * jumpSpeed / (2 * minJumpHeight);

        //playerCamera = Camera.main;
    }

    private void Update()
    {
        if (PauseSystem.paused)
        {
            deltaTime = 0;
            return;
        }
        else deltaTime = Time.deltaTime;

        DoMovement();

        UpdateHeight();

        if(Input.GetKeyDown(KeyCode.O))
        {
            charController.enabled = false;
            transform.position = spawnPoint;
            charController.enabled = true;
        }
    }

    // Update is called once per frame
    void DoMovement()
    {
        isSliding = GetIsSliding();
        isGrounded = GetIsGrounded();

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        input = new Vector3(Input.GetAxis("Vertical"), 0, Input.GetAxis("Horizontal"));
        input = Vector3.ClampMagnitude(input, 1); // limit input to 1

        // recalculate based on look facing
        input = (forward * input.x) + (right * input.z);
        if (!isGrounded || isSliding) input /= 4; // reduce acceleration in air

        Vector3 force = acceleration * input;

        float dec = isGrounded && !isSliding ? deceleration : airDeceleration;
        Vector3 vel = new(velocity.x, 0, velocity.z);

        float d = Mathf.Min(dec * deltaTime, Mathf.Abs(vel.x));
        if (Mathf.Abs(vel.x - input.x) >= Mathf.Abs(vel.x) + Mathf.Abs(input.x)) vel.x -= Mathf.Sign(vel.x) * d;
        d = Mathf.Min(dec * deltaTime, Mathf.Abs(vel.z)); ;
        if (Mathf.Abs(vel.z - input.z) >= Mathf.Abs(vel.z) + Mathf.Abs(input.z)) vel.z -= Mathf.Sign(vel.z) * d;

        velocity = new(vel.x, velocity.y, vel.z);

        if (!isGrounded || isSliding) force.y -= gravity;

        if(Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
            Invoke(nameof(ReleaseJump), jumpBuffer);
        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpJustReleased = true;
            jumpPressed = false;
        }

        if (jumpPressed && isGrounded)
        {
            Vector3 jumpVector;

            if (isSliding) jumpVector = groundNormal * jumpSpeed;
            else if (isCrouching) jumpVector = (transform.up + transform.forward).normalized * jumpSpeed;
            else jumpVector = transform.up * jumpSpeed;

            velocity.y = jumpVector.y;
            velocity.x += jumpVector.x;
            velocity.z += jumpVector.z;
        }

        if (jumpJustReleased && velocity.y > 0)
        {
            gravity = minJumpGravity;
            jumpJustReleased = false;
        }
        if (velocity.y < 0) gravity = maxJumpGravity;

        // get velocity w/o y
        Vector3 v = new(velocity.x, 0, velocity.z);

        // get targetSpeed
        float targetSpeed = Input.GetKey(KeyCode.LeftControl) ? crouchSpeed : runningSpeed;
        if (isGrounded && !isSliding)
        {
            // decay max speed while on ground
            maxSpeed -= maxSpeedDecay * deltaTime;
            if (maxSpeed < targetSpeed) maxSpeed = targetSpeed;

            // clamp to maxSpeed
            v = Vector3.ClampMagnitude(v, maxSpeed);
            velocity = new(v.x, velocity.y, v.z);

        } else if (v.magnitude > maxSpeed) maxSpeed = v.magnitude;
        // increase maxSpeed to match airSpeed (w/o y)

        // clamp to airSpeed
        v = Vector3.ClampMagnitude(v, airSpeed);
        velocity = new(v.x, velocity.y, v.z);

        velocity += 0.5f * deltaTime * force; // add half before moving
        charController.Move(velocity * deltaTime); // move player
        velocity += 0.5f * deltaTime * force; // add other half after moving

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        mouseLook = new Vector2(rotationX, Input.GetAxis("Mouse X") * lookSpeed);

        jumpJustReleased = false;
    }

    void ReleaseJump()
    {
        if (!jumpPressed) jumpJustReleased = true;
        jumpPressed = false;
    }

    void UpdateHeight()
    {
        targetHeight = Input.GetKey(KeyCode.LeftControl) ? crouchHeight : standingHeight;

        Debug.DrawRay(transform.position + new Vector3(0, charController.height), Vector3.up * (2f - charController.height));
        if (Physics.Raycast(transform.position + new Vector3(0, charController.height), Vector3.up, 2f - charController.height))
        {
            // stay croushed if theres something above
            targetHeight = crouchHeight;
        }

        charController.height -= (charController.height - targetHeight) / 8 * deltaTime * 50;

        charController.center = new Vector3(0, charController.height / 2, 0);
        playerCamera.transform.localPosition = new Vector3(0, charController.height - 0.5f, 0);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.rigidbody;

        if (body != null && !body.isKinematic)
        {
            if (hit.moveDirection.y > -0.3)
            {
                Vector3 pushDir = new(hit.moveDirection.x, 0, hit.moveDirection.z);

                body.velocity = pushDir * 10;
                return;
            }
        }

        Debug.DrawRay(hit.point, hit.normal, Color.red);

        hitPointNormal = hit.normal;

        Debug.DrawRay(transform.position, velocity, Color.cyan);

        Vector3 newVel = (velocity - Vector3.Project(velocity, hitPointNormal));
        velocity = newVel;
    }

    bool GetIsSliding()
    {
        Vector3 p1 = transform.position + (charController.height - charController.radius) * Vector3.up;
        Vector3 p2 = transform.position + charController.radius * Vector3.up;

        bool hasHit = Physics.CapsuleCast(p1, p2, charController.radius, Vector3.down, out RaycastHit hit, 2);

        Debug.DrawRay(transform.position, Vector3.down * 2f, Color.blue);
        Debug.DrawRay(hit.point, hit.normal, Color.blue);

        Debug.DrawLine(p1, p2, Color.black);

        return isGrounded && hasHit && Vector3.Angle(Vector3.up, hit.normal) > slopeLimit;
    }

    bool GetIsGrounded()
    {
        Vector3 p1 = transform.position + (charController.height - charController.radius) * Vector3.up;
        Vector3 p2 = transform.position + charController.radius * Vector3.up;

        bool hasHit = Physics.CapsuleCast(p1, p2, charController.radius, Vector3.down, out RaycastHit hit, 0.01f);

        groundNormal = hit.normal;

        return hasHit;
    }

}
