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
    public float runningSpeed = 7.5f;
    public float crouchSpeed = 3.75f;
    public float acceleration = 3.75f;
    [Space]
    public float slopeLimit = 45;
    public float deceleration = 4;
    public float slopeSpeed = 175;
    public float maxSpeedDecay = 16;

    [Space]
    public float gravity = 20f;
    public float jumpSpeed = 8.0f;

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

        gravity = jumpSpeed / airTime;

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

        if (isGrounded && !isSliding)
        {
            Vector3 v = new(velocity.x, 0, velocity.z);
            //float mag = v.magnitude - deceleration * Time.deltaTime * (1-input.magnitude);
            //if (mag < 0) mag = 0;

            //v = v.normalized * mag;

            //velocity.x = v.x;
            //velocity.z = v.z;

            // x component
            float d = Mathf.Min(deceleration * deltaTime, Mathf.Abs(v.x));
            if (Mathf.Abs(v.x - input.x) >= Mathf.Abs(v.x) + Mathf.Abs(input.x)) v.x -= Mathf.Sign(v.x) * d;

            // z
            d = Mathf.Min(deceleration * deltaTime, Mathf.Abs(v.z)); ;
            if (Mathf.Abs(v.z - input.z) >= Mathf.Abs(v.z) + Mathf.Abs(input.z)) v.z -= Mathf.Sign(v.z) * d;

            velocity = new(v.x, velocity.y, v.z);
        }

        if(!isGrounded || isSliding) force.y -= gravity;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Vector3 jumpVector;

            if (isSliding) jumpVector = groundNormal * jumpSpeed;
            else if (isCrouching) jumpVector = (transform.up + transform.forward).normalized * jumpSpeed;
            else jumpVector = transform.up * jumpSpeed;

            velocity.y = jumpVector.y;
            velocity.x += jumpVector.x;
            velocity.z += jumpVector.z;
        }

        //if (isSliding) force += slopeSpeed * new Vector3(groundNormal.x, -groundNormal.y, groundNormal.z);

        velocity += 0.5f * deltaTime * force; // add half before moving

        float targetSpeed = Input.GetKey(KeyCode.LeftControl) ? crouchSpeed : runningSpeed;
        if (isGrounded && !isSliding)
        {
            maxSpeed -= maxSpeedDecay * deltaTime;
            if (maxSpeed < targetSpeed) maxSpeed = targetSpeed;

            Vector3 v = new(velocity.x, 0, velocity.z);
            v = Vector3.ClampMagnitude(v, maxSpeed);

            velocity = new(v.x, velocity.y, v.z);
        } else if (velocity.magnitude > maxSpeed) maxSpeed = velocity.magnitude;

        charController.Move(velocity * deltaTime); // move player
        velocity += 0.5f * deltaTime * force; // add other half after moving

        //Debug.DrawRay(transform.position, velocity, Color.cyan);

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        mouseLook = new Vector2(rotationX, Input.GetAxis("Mouse X") * lookSpeed);

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

                body.velocity = pushDir * 5;
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

        //Debug.Log(Vector3.Angle(Vector3.up, hit.normal));

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
