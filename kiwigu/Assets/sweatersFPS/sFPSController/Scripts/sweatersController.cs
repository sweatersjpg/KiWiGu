using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class sweatersController : MonoBehaviour
{
    public static sweatersController instance;

    [HideInInspector]
    public CharacterController charController;

    public Camera playerCamera;
    public GameObject playerHead;
    //public static bool paused = false;
    public TextMeshProUGUI debugSpeedDisp;

    [Header("Movement Metrics")]
    public float maxJumpDistance = 4;
    public float maxJumpHeight = 2;
    public float minJumpHeight = 0.5f;

    [Header("Speed Limits")]
    public float runningSpeed = 7.5f;
    public float airSpeed = 10;
    public float crouchSpeed = 3.75f;
    public float maxSpeedIncrease = 8;
    public float maxSpeedDecay = 16;

    [Header("Acceleration")]
    public float acceleration = 3.75f;
    public float airAcceleration = 3;
    public float encomberedAcceleration = 3;
    public float grappleAcceleration = 0;

    [Header("Deceleration")]
    public float deceleration = 4;
    public float turnDeceleration = 64;
    public float airDeceleration = 1;
    public float grappleDeceleration = 0;

    [HideInInspector] public float gravity = 20f;
    [HideInInspector] public float jumpSpeed = 8.0f;

    [Space]
    public float slopeLimit = 45;
    public float jumpBuffer = 0.2f;
    public float kyoteTime = 0.2f;

    float maxJumpGravity;
    float minJumpGravity;

    [HideInInspector] public float maxSpeed;

    [Space]
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    [Header("Player Metrics")]
    public float standingHeight = 2;
    public float crouchHeight = 1.5f;

    float targetHeight;

    public Vector2 mouseLook;
    public Vector3 velocity;
    public Vector3 rawInput;
    public Vector3 input;

    float rotationX = 0;

    Vector3 hitPointNormal;
    Vector3 groundNormal;

    Vector3 spawnPoint;

    float deltaTime;

    bool jumpPressed = false;
    bool jumpJustReleased = false;

    public bool isEncombered;
    public bool isGrappling;

    public bool isSliding;
    public bool isGrounded;
    public bool wasGrounded;

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

        if(!CheckPointSystem.spawnPoint.Equals(new Vector3()))
        {
            charController.enabled = false;

            transform.position = CheckPointSystem.spawnPoint;

            charController.enabled = true;
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        charController.height = standingHeight;
        targetHeight = charController.height;

        spawnPoint = transform.position;

        // air time based on air acceleration and max speed
        float a = airAcceleration;
        float airTime = (-runningSpeed + Mathf.Sqrt((runningSpeed * runningSpeed) - 2 * a * (-maxJumpDistance))) / a;

        // air time based solely on max air speed
        //float airTime = maxJumpDistance / airSpeed;

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

        isGrappling = false;

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
        wasGrounded = isGrounded;
        GetIsGrounded();

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        input = new Vector3(Input.GetAxis("Vertical"), 0, Input.GetAxis("Horizontal"));
        input = Vector3.ClampMagnitude(input, 1); // limit input to 1

        rawInput = new(input.x, 0, input.z);
        // recalculate based on look facing
        input = (forward * input.x) + (right * input.z);

        // acceleration based on ground or air
        float acc = isGrounded && !isSliding ? acceleration : airAcceleration;
        if (isGrappling && !isGrounded) acc = grappleAcceleration;
        // if (isEncombered) acc = encomberedAcceleration;
        Vector3 force = acc * input;

        // deceleration based on ground air and movement
        float dec = input.magnitude > 0.1f ? turnDeceleration : deceleration;
        if(!isGrounded || isSliding)
        {
            dec = input.magnitude > 0.1f ? airDeceleration : 0;
            if (isGrappling) dec = grappleDeceleration;
        }

        Vector3 vel = new(velocity.x, 0, velocity.z);

        float d = Mathf.Min(dec * deltaTime, Mathf.Abs(vel.x));
        if (Mathf.Abs(vel.x - input.x) >= Mathf.Abs(vel.x) + Mathf.Abs(input.x)) vel.x -= Mathf.Sign(vel.x) * d;
        d = Mathf.Min(dec * deltaTime, Mathf.Abs(vel.z)); ;
        if (Mathf.Abs(vel.z - input.z) >= Mathf.Abs(vel.z) + Mathf.Abs(input.z)) vel.z -= Mathf.Sign(vel.z) * d;

        velocity = new(vel.x, velocity.y, vel.z);

        // apply gravity when not grounded or sliding
        if (!isGrounded || isSliding) force.y -= gravity;

        // jump buffer logic
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

        // do jump
        if (jumpPressed && isGrounded)
        {
            isGrounded = false;
            
            Vector3 jumpVector;

            if (isSliding) jumpVector = groundNormal * jumpSpeed;
            else if (isCrouching) jumpVector = (transform.up + transform.forward).normalized * jumpSpeed;
            else jumpVector = transform.up * jumpSpeed;

            velocity.y = jumpVector.y;
            velocity.x += jumpVector.x;
            velocity.z += jumpVector.z;
        }

        // increase gravity with jump release
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

            if(v.magnitude < maxSpeed) maxSpeed = v.magnitude;
            maxSpeed -= maxSpeedDecay * deltaTime;
            if (maxSpeed < targetSpeed) maxSpeed = targetSpeed;

            // clamp to maxSpeed
            v = Vector3.ClampMagnitude(v, maxSpeed);
            velocity = new(v.x, velocity.y, v.z);

        }
        else if (v.magnitude > maxSpeed)
        {
            //maxSpeed = v.magnitude;
            maxSpeed += Mathf.Min(maxSpeedIncrease * deltaTime, airSpeed-maxSpeed);
        }
        // increase maxSpeed to match airSpeed (w/o y)

        if (!wasGrounded && isGrounded) // when landing on the ground
        {
            maxSpeed -= maxSpeedDecay;
            if (maxSpeed < targetSpeed) maxSpeed = targetSpeed;
        }

        if (debugSpeedDisp) debugSpeedDisp.text = "speed:\n" + Mathf.Floor(v.magnitude * 100) / 100;

        // clamp to airSpeed
        if(!isGrappling) v = Vector3.ClampMagnitude(v, maxSpeed);
        velocity = new(v.x, velocity.y, v.z);

        velocity += 0.5f * deltaTime * force; // add half before moving
        charController.Move(velocity * deltaTime); // move player
        velocity += 0.5f * deltaTime * force; // add other half after moving

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerHead.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
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
        playerHead.transform.localPosition = new Vector3(0, charController.height - 0.5f, 0);
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

    void GetIsGrounded()
    {
        Vector3 p1 = transform.position + (charController.height - charController.radius) * Vector3.up;
        Vector3 p2 = transform.position + charController.radius * Vector3.up;

        bool hasHit = Physics.CapsuleCast(p1, p2, charController.radius, Vector3.down, out RaycastHit hit, 0.01f);

        groundNormal = hit.normal;

        if (hasHit) isGrounded = true;
        else
        {
            Invoke(nameof(SetGroundedToFalse), kyoteTime);
        }
        // isGrounded = hasHit;
    }

    void SetGroundedToFalse()
    {
        isGrounded = false;
    }

}
