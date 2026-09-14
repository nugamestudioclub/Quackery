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

    // If you press jump and release quickly, you stop your jump early. It works by setting this variable when you jump, and then subtracting it
    // from your velocity when you stop holding the jump button. It gets reduced by gravity/deltatime each frame.
    private float jumpAbortVelocity = 0f;

    private float bhopActiveCooldown = 0f;
    private Vector3 bhopStoredVelocity = new Vector3(0, 0, 0);

    // Coyote time / jump buffer
    private float incomingJBufferActiveCooldown = 0f;


    // This controls where the visual player is looking. If it is false, the player will just look in the direction they are moving.
    // If it is true, the player will face the same direction of the camera, and their head will look direction where the camera is pointing.
    // It should be true if the player is using an item that requires aiming, like the rocket launcher. This gives the appearance that the
    // character themself is actually aiming.
    private bool playerVisualsLocked = false;
    private Vector3 lastLookDirection = new Vector3(1, 0, 0);

    private bool wearingPropellerHat = false;
    private float propellerHatMinVelo = -2f;

    // -- PLAYER CONFIGURATION --
    // These variables are essentially "magic number" constants which define player physics/input/etc. defaults.
    // Despite being SerializeFields, they probably should not be adjusted per level; they are set to SerializeField primarily
    // for convenience. If you change a variable in the Unity editor and want to keep the change, please change it here too, AND remember to
    // update the prefab
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform playerBodyVisuals;
    [SerializeField] private Transform playerHeadVisuals;

    [SerializeField] private float visualsRotationSpeed = 10f; // How long it takes the player visuals to turn around (does NOT affect transform rotation)

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSensitivity = 0.35f;

    [SerializeField] private float gravityStrength = 17f;
    [SerializeField] private float terminalVelocity = 30f;   // The maximum speed the player can be going
    [SerializeField] private float airDrag = 2f;

    // How long (in seconds) you have BEFORE hitting the ground in which you can press jump and have it register when you hit the ground
    [SerializeField] private float incomingJumpBufferCooldown = 0.18f;

    [SerializeField] private float jumpHeight = 8f;
    [SerializeField] private float wallJumpHeight = 8f;
    [SerializeField] private float wallJumpSideVelocity = 8f;  // The amount of sideways velocity you get from walljumps (jump *away* from the wall)
    [SerializeField] private float wallSlideVelocity = -1.5f;  // How fast you move down while *sliding* on a wall; MUST BE NEGATIVE!!

    [SerializeField] private float bhopVelocityCutoff = 6f; // You must be going at least this speed to bunny hop
    [SerializeField] private float bhopCooldown = 0.2f;    // Cooldown after hitting the ground when you can still bunny hop
    [SerializeField] private float bhopAddedVelocity = 10f; // Additional velocity you get from bunny hopping

    [SerializeField] private float superjumpVelocityCutoff = 10f; // You must be going at least this speed to super walljump
    [SerializeField] private float superjumpAddedVelocity = 15f; // Additional vertical velocity you get from a super walljump

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

    // True: wear the propeller hat (caps minimum y velocity), false: take hat off
    public void WearPropellerHat(bool wearing) {
        wearingPropellerHat = wearing;
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
        // NOTE: These variables exist purely to determine which buttons were pressed
        // Their values should never be changed later in the script for any reason
        Vector2 moveInput = input.Player.Walk.ReadValue<Vector2>();
        Vector2 lookInput = input.Player.Look.ReadValue<Vector2>();
        bool jumpInput = input.Player.Jump.WasPressedThisFrame();
        bool jumpReleasedInput = input.Player.Jump.WasReleasedThisFrame();
        // ----

        // -- Do movement --
        // Player controlled movement
        Vector3 movement = (transform.right * moveInput.x) + (transform.forward * moveInput.y);
        movement *= moveSpeed;

        bool incomingJBuffer = incomingJBufferActiveCooldown > 0f;

        // - Gravity / Jumping -
        // Grounded
        if (controller.isGrounded) {
            jumpAbortVelocity = 0f;

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
            if (jumpInput || incomingJBuffer) {
                playerVelocity.y = jumpHeight;
                if (bhopActiveCooldown > 0f) {
                    playerVelocity += bhopStoredVelocity + (bhopStoredVelocity.normalized * bhopAddedVelocity);
                }
                else {
                    jumpAbortVelocity = (float)(jumpHeight * 0.85);
                }
            }
        }
        // Not grounded
        else {
            // On wall
            if (touchingWall) {
                jumpAbortVelocity = 0f;

                if (playerVelocity.y < wallSlideVelocity) {
                    playerVelocity.y = wallSlideVelocity;
                }

                if (jumpInput || incomingJBuffer) {
                    float originalYVelocity = playerVelocity.y;
                    playerVelocity = wallNormal * wallJumpSideVelocity;

                    if (originalYVelocity > superjumpVelocityCutoff) {
                        playerVelocity.y = originalYVelocity + superjumpAddedVelocity;
                    }
                    else {
                        playerVelocity.y = wallJumpHeight;
                    }
                }
            }
            // Not touching wall
            else if (jumpInput) {
                playerItemController.UseJumpItem();

                // NOTE: since we set jump buffer here, it is possible to use a jump item AND jump on the same input.
                // This might not be an issue, but it's something to be aware of.
                incomingJBufferActiveCooldown = incomingJumpBufferCooldown;
            }

            playerVelocity.y -= gravityStrength * Time.deltaTime;
            if (jumpAbortVelocity >= 0f) {
                jumpAbortVelocity -= gravityStrength * Time.deltaTime;
            }

            float dragAmount = airDrag * Time.deltaTime;
            playerVelocity.x = Mathf.MoveTowards(playerVelocity.x, 0f, dragAmount);
            playerVelocity.z = Mathf.MoveTowards(playerVelocity.z, 0f, dragAmount);
        }

        if (wearingPropellerHat && playerVelocity.y < propellerHatMinVelo) {
            playerVelocity.y = propellerHatMinVelo;
        }

        playerVelocity = Vector3.ClampMagnitude(playerVelocity, terminalVelocity);

        // Abort jump
        if (jumpReleasedInput && jumpAbortVelocity > 0f) {
            playerVelocity.y -= jumpAbortVelocity;
            jumpAbortVelocity = 0f;
        }

        // -- Apply final movement --
        Vector3 finalMovement = movement + playerVelocity;
        CollisionFlags cflags = controller.Move(finalMovement * Time.deltaTime);

        touchingWall = (cflags & CollisionFlags.Sides)!= 0;

        // -- Rotations --
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

        // -- Reduce timers --
        if (bhopActiveCooldown >= 0f) {
            bhopActiveCooldown -= Time.deltaTime;
        }

        if (incomingJBufferActiveCooldown >= 0f) {
            incomingJBufferActiveCooldown -= Time.deltaTime;
        }
    }
}
