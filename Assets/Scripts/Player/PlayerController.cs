using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    private CharacterController controller;
    private PlayerItemController playerItemController;

    // Velocity from gravity and other sources, NOT from player controls
    private Vector3 playerVelocity;

    private float cameraPitch = 0f;
    private bool touchingWall = false;
    private Vector3 wallNormal;

    private float bhopActiveCooldown = 0f;
    private Vector3 bhopStoredVelocity = new Vector3(0, 0, 0);

    // This controls where the visual player is looking. If it is false, the player will just look in the direction they are moving.
    // If it is true, the player will face the same direction of the camera, and their head will look direction where the camera is pointing.
    // It should be true if the player is using an item that requires aiming, like the rocket launcher. This gives the appearance that the
    // character themself is actually aiming.
    private bool playerVisualsLocked = false;
    private Vector3 lastLookDirection = new Vector3(1, 0, 0);

    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform playerBodyVisuals;
    [SerializeField] private Transform playerHeadVisuals;

    [SerializeField] private float visualsRotationSpeed = 10f; // How long it takes the player visuals to turn around (does NOT affect transform rotation)

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSensitivity = 0.35f;

    [SerializeField] private float gravityStrength = 17f;
    [SerializeField] private float terminalVelocity = 20f;   // The maximum speed the player can be going
    [SerializeField] private float airDrag = 2f;

    [SerializeField] private float jumpHeight = 8f;
    [SerializeField] private float wallJumpHeight = 5f;
    [SerializeField] private float wallJumpSideVelocity = 5f;  // The amount of sideways velocity you get from walljumps (jump *away* from the wall)
    [SerializeField] private float wallSlideVelocity = -1.5f;    // How fast you move down while *sliding* on a wall; MUST BE NEGATIVE!!

    [SerializeField] private float bhopVelocityCutoff = 6f; // You must be going at least this speed to bunny hop
    [SerializeField] private float bhopCooldown = 0.2f;     // Cooldown after hitting the ground when you can still bunny hop
    [SerializeField] private float bhopAddedVelocity = 10f; // Additional velocity you get from bunny hopping

    private void Awake()
    {
        input = new PlayerInput();
        controller = GetComponent<CharacterController>();
        playerItemController = GetComponent<PlayerItemController>();
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

    // True: player visuals are locked to the camera, meaning the player faces the direction of the camera as if they are looking in that direction
    // False: player rotates freely, and always faces the direction they are moving/were last moving in
    public void SetVisualsLocked(bool locked) {
        playerVisualsLocked = locked;
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
            // Store bunny hop information
            float horizontalSpeed = Mathf.Sqrt(
                (playerVelocity.x * playerVelocity.x) + (playerVelocity.z * playerVelocity.z)
            );
            if (horizontalSpeed > bhopVelocityCutoff) {
                bhopActiveCooldown = bhopCooldown;
                bhopStoredVelocity = new Vector3(playerVelocity.x, 0f, playerVelocity.z);
            }

            // Set velocity to 0
            if (playerVelocity.y < 0f) {
                playerVelocity.y = -2f;
                playerVelocity.x = 0f;
                playerVelocity.z = 0f;
            }

            // Jump. And if the player can bunny hop, do it
            if (jumpInput) {
                playerVelocity.y = jumpHeight;
                if (bhopActiveCooldown > 0f) {
                    playerVelocity += bhopStoredVelocity + (bhopStoredVelocity.normalized * bhopAddedVelocity);
                }
            }
        }
        else {
            if (touchingWall) {
                if (playerVelocity.y < wallSlideVelocity) {
                    playerVelocity.y = wallSlideVelocity;
                }

                if (jumpInput) {
                    playerVelocity = wallNormal * wallJumpSideVelocity;
                    playerVelocity.y = wallJumpHeight;
                }
            }
            else if (jumpInput) {
                playerItemController.UseJumpItem();
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

        // Player visuals rotation, depending on if the visuals are locked to the camera or not
        if (playerVisualsLocked) {
            playerBodyVisuals.rotation = transform.rotation;
            playerHeadVisuals.rotation = cameraPivot.rotation;
        }
        else {
            Vector3 lookDirection = new Vector3(finalMovement.x, 0f, finalMovement.z);
            Quaternion targetRotation;
            if (lookDirection.sqrMagnitude > 0.001f) {
                lastLookDirection = lookDirection;
                targetRotation = Quaternion.LookRotation(lookDirection);
            }
            else {
                targetRotation = Quaternion.LookRotation(lastLookDirection);
            }

            // Slerp smoothly rotates the player
            playerBodyVisuals.rotation = Quaternion.Slerp(
                playerBodyVisuals.rotation,
                targetRotation,
                visualsRotationSpeed * Time.deltaTime
            );
            playerHeadVisuals.rotation = Quaternion.Slerp(
                playerHeadVisuals.rotation,
                targetRotation,
                visualsRotationSpeed * Time.deltaTime
            );
        }

        // Reduce bhop timer
        if (bhopActiveCooldown >= 0f) {
            bhopActiveCooldown -= Time.deltaTime;
        }
    }
}
