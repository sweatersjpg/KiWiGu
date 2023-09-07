using System.Collections;using System.Collections.Generic;using UnityEngine;[RequireComponent(typeof(CharacterController))]public class FPSController : MonoBehaviour{    public static FPSController instance;    public float walkingSpeed = 7.5f;    public float runningSpeed = 11.5f;    public float crouchSpeed = 5;    public float crouchHeight = 1;    public float jumpSpeed = 8.0f;    public float gravity = 20.0f;    public Camera playerCamera;    public float lookSpeed = 2.0f;    public float lookXLimit = 45.0f;    public float friction = 0.3f;    [HideInInspector] public CharacterController characterController;    Vector3 moveDirection = Vector3.zero;    float rotationX = 0;    Vector3 slopeVector;    public Vector2 MouseLook;    [HideInInspector]    public bool canMove = true;    public bool paused = false;    private void Awake()    {        if (instance == null)        {            instance = this;            DontDestroyOnLoad(gameObject);        }        else        {            Destroy(gameObject);        }    }    void Start()    {        characterController = GetComponent<CharacterController>();        // Lock cursor        Cursor.lockState = CursorLockMode.Locked;        Cursor.visible = false;    }    void Update()    {        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.P)) TogglePaused();        if (paused) return;        CharacterMovement();    }    public void TogglePaused()    {        Cursor.lockState = !paused ? CursorLockMode.None : CursorLockMode.Locked;        Cursor.visible = !paused;        Time.timeScale = !paused ? 0 : 1;        paused = !paused;    }    private void CharacterMovement()    {        // We are grounded, so recalculate move direction based on axes        Vector3 forward = transform.TransformDirection(Vector3.forward);        Vector3 right = transform.TransformDirection(Vector3.right);        // Press Left Shift to run        bool isRunning = Input.GetKey(KeyCode.LeftShift);        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        //float movementDirectionY = moveDirection.y;
        Vector3 movementDirection = moveDirection;        moveDirection = (forward * curSpeedX) + (right * curSpeedY);                if (Input.GetButton("Jump") && canMove && characterController.isGrounded)        {            moveDirection.y = jumpSpeed;        }        else        {            moveDirection.y = movementDirection.y;        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        Vector3 grav = new Vector3(0, -gravity * Time.deltaTime, 0);

        if (!characterController.isGrounded)        {            moveDirection.y -= gravity * Time.deltaTime;        } else {

            // use hit slopeVector to calc sideways velocity
            moveDirection = movementDirection;

            Vector3 proj = Vector3.Project(grav, slopeVector);

            moveDirection += proj;

            Debug.DrawRay(transform.position, slopeVector, Color.red);

        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);        MouseLook = new Vector2(rotationX, Input.GetAxis("Mouse X") * lookSpeed);        //playerCamera.transform.localPosition += 4 * 50 * Time.deltaTime * (cameraTargetPosition - playerCamera.transform.localPosition);    }    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //hitNormal = hit.normal;

        Vector3 rightVector = Vector3.Cross(hit.normal, Vector3.up);
        slopeVector = -Vector3.Cross(rightVector, hit.normal);

        Debug.Log(slopeVector);
    }}