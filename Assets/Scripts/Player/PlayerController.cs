using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    private CharacterController controller;

    // Velocity from gravity and other sources, NOT from player controls
    private Vector3 playerVelocity;

    private float cameraPitch = 0f;
    private bool touchingWall = false;
    private Vector3 wallNormal;

    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSensitivity = 0.35f;
    [SerializeField] private float gravityStrength = 17f;
    [SerializeField] private float terminalVelocity = 20f;
    [SerializeField] private float airDrag = 2f;
    [SerializeField] private float jumpHeight = 8f;
    [SerializeField] private float wallJumpHeight = 5f;
    [SerializeField] private float wallJumpSideVelocity = 5f;

    private void Awake()
    {
        input = new PlayerInput();
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Ignore floors and ceilings.
        if (Mathf.Abs(hit.normal.y) < 0.2f)
        {
            wallNormal = hit.normal;
        }
    }


    // Adds to the player's velocity.
    // Velocity is applied *after* movement from player input and is entirely separate.
    // The player will gradually lose/gain velocity according to deceleration/gravity.
    public void AddVelocity(Vector3 velocity)
    {
        playerVelocity += velocity;
    }

    // Sets the player's velocity.
    // If you don't want to set a specific direction (leave it as is), use null.
    // Velocity is applied *after* movement from player input and is entirely separate.
    // The player will gradually lose/gain velocity according to deceleration/gravity.
    public void SetVelocity(float? x, float? y, float? z)
    {
        Vector3 setVelo = new Vector3(
            x ?? playerVelocity.x,
            y ?? playerVelocity.y,
            z ?? playerVelocity.z
        );
        playerVelocity = setVelo;
    }

    private void Update()
    {
        // -- Get inputs --
        Vector2 moveInput = input.Player.Walk.ReadValue<Vector2>();
        Vector2 lookInput = input.Player.Look.ReadValue<Vector2>();
        bool jumpInput = input.Player.Jump.WasPressedThisFrame();

        // -- Do movement --
        // Player controlled movement
        Vector3 movement = (transform.right * moveInput.x) + (transform.forward * moveInput.y);
        movement *= moveSpeed;

        // Gravity / Jumping
        if (controller.isGrounded) {
            if (playerVelocity.y < 0f) {
                playerVelocity.y = -2f;
                playerVelocity.x = 0f;
                playerVelocity.z = 0f;
            }

            if (jumpInput) {
                playerVelocity.y = jumpHeight;
            }
        }
        else {
            if (touchingWall && jumpInput) {
                playerVelocity = wallNormal * wallJumpSideVelocity;
                playerVelocity.y = wallJumpHeight;
            }

            playerVelocity.y -= gravityStrength * Time.deltaTime;
            float dragFactor = Mathf.Exp(-airDrag * Time.deltaTime);
            playerVelocity.x *= dragFactor;
            playerVelocity.z *= dragFactor;
        }
        playerVelocity = Vector3.ClampMagnitude(playerVelocity, terminalVelocity);

        Vector3 finalMovement = movement + playerVelocity;
        CollisionFlags cflags = controller.Move(finalMovement * Time.deltaTime);

        touchingWall = (cflags & CollisionFlags.Sides)!= 0;

        // Camera rotation (do NOT use deltaTime!)
        transform.Rotate(
            0f,
            lookInput.x * lookSensitivity,
            0f
        );
        cameraPitch -= lookInput.y * lookSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -89f, 89f);
        cameraPivot.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }
}
