using UnityEngine;

// TODO: Make it so if the player hits the ground, their active grappling hook goes away, even if they didn't release LMB

public class PlayerItemController : MonoBehaviour
{
    private PlayerInput input;
    private PlayerController playerController;

    private PrimaryItems primaryItem = PrimaryItems.None;
    private SecondaryItems secondaryItem = SecondaryItems.None;
    private JumpItems jumpItem = JumpItems.None;

    // Item references to created visual instances
    // These exist so that they can be deleted later
    private GameObject primaryItemVisualReference = null;
    private GameObject secondaryItemVisualReference = null;
    private GameObject jumpItemVisualReference = null;

    private bool propellerHatInUse = false;
    private float propellerHatDestroyCooldown = 0f;

    private bool grapplingHookInUse = false;

    // -- CONFIGURATION --
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform bodyItemVisuals;
    [SerializeField] private Transform headItemVisuals;

    [SerializeField] private PrefabDatabase prefabs;

    private float propellerHatJumpBoost = 15f;

    private float grapplingHookRange = 25f;

    // -- Unity methods --
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        input = new PlayerInput();
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    // -- Local helpers --
    private void DestroyAllChildren(Transform parent)
    {
        // Iterate backwards, because destroying an item will shorten the array
        for (int i = parent.childCount - 1; i >= 0; i--) {
            Object.Destroy(parent.GetChild(i).gameObject);
        }
    }

    // -- Public api --
    public void UseJumpItem()
    {
        switch (jumpItem) {
            case JumpItems.None:
                break;
            case JumpItems.PropellerHat:
                if (propellerHatInUse) {
                    break;
                }
                playerController.WearPropellerHat(false);
                playerController.SetVelocity(null, propellerHatJumpBoost, null);
                propellerHatInUse = true;
                propellerHatDestroyCooldown = 1.0f;
                PropellerHat pHat = jumpItemVisualReference.GetComponent<PropellerHat>();
                pHat.setSpinSpeed(1600);
                break;
        }

        if (jumpItem != JumpItems.PropellerHat) {
            jumpItem = JumpItems.None;

            if (jumpItemVisualReference != null) {
                Object.Destroy(jumpItemVisualReference.gameObject);
                jumpItemVisualReference = null;
            }
        }
    }

    public bool HasPrimaryItem() {
        return primaryItem != PrimaryItems.None;
    }

    public bool HasSecondaryItem() {
        return secondaryItem != SecondaryItems.None;
    }

    public bool HasJumpItem() {
        return jumpItem != JumpItems.None;
    }

    public void ObtainPrimaryItem(PrimaryItems item)
    {
        if (primaryItemVisualReference != null) {
            Object.Destroy(primaryItemVisualReference.gameObject);
            primaryItemVisualReference = null;
        }

        if (PrimaryItemUtility.ItemLocksPlayerCamera(item)) {
            playerController.SetVisualsLocked(true);
        }
        primaryItem = item;

        // Update visuals
        switch (primaryItem) {
            case PrimaryItems.RocketLauncher:
                primaryItemVisualReference = Instantiate(
                    prefabs.rocketLauncher,
                    headItemVisuals.TransformPoint(new Vector3(0.4f, -0.1f, 0f)),
                    headItemVisuals.rotation,
                    headItemVisuals
                );
                break;
            case PrimaryItems.GrapplingHook:
                primaryItemVisualReference = Instantiate(
                    prefabs.grapplingHook,
                    bodyItemVisuals.TransformPoint(new Vector3(0.45f, 0.8f, 0f)),
                    bodyItemVisuals.rotation,
                    bodyItemVisuals
                );
                break;
            default:
                break;
        }
    }

    public void ObtainSecondaryItem(SecondaryItems item)
    {
        if (secondaryItemVisualReference != null) {
            Object.Destroy(secondaryItemVisualReference);
            secondaryItemVisualReference = null;
        }

        if (SecondaryItemUtility.ItemLocksPlayerCamera(item)) {
            playerController.SetVisualsLocked(true);
        }
        secondaryItem = item;
    }

    public void ObtainJumpItem(JumpItems item)
    {
        if (jumpItemVisualReference != null) {
            Object.Destroy(jumpItemVisualReference.gameObject);
            jumpItemVisualReference = null;
        }

        if (JumpItemUtility.ItemLocksPlayerCamera(item)) {
            playerController.SetVisualsLocked(true);
        }
        jumpItem = item;

        // Update visuals
        switch (jumpItem) {
            case JumpItems.PropellerHat:
                jumpItemVisualReference = Instantiate(
                    prefabs.propellerHat,
                    headItemVisuals.TransformPoint(new Vector3(0f, 0.2f, 0f)),
                    headItemVisuals.rotation,
                    headItemVisuals
                );
                playerController.WearPropellerHat(true);
                break;
            default:
                break;
        }
    }

    // -- Main update loop --
    private void Update()
    {
        bool usePrimaryInput = input.Player.UsePrimary.WasPressedThisFrame();
        bool releasePrimaryInput = input.Player.UsePrimary.WasReleasedThisFrame();
        bool useSecondaryInput = input.Player.UseSecondary.WasPressedThisFrame();

        if (usePrimaryInput) {
            switch (primaryItem) {
                case PrimaryItems.None:
                    break;
                case PrimaryItems.RocketLauncher:
                    Vector3 launchDirection = -cameraPivot.forward * 19f;
                    playerController.AddVelocity(launchDirection);
                    Instantiate(
                        prefabs.rocket,
                        headItemVisuals.TransformPoint(new Vector3(0.4f, -0.1f, 0f)),
                        headItemVisuals.rotation
                    );
                    break;
                case PrimaryItems.GrapplingHook:
                    if (grapplingHookInUse) {
                        break;
                    }
                    grapplingHookInUse = true;
                    playerController.StartGrappling(grapplingHookRange);

                    break;
            }
            if (primaryItem != PrimaryItems.GrapplingHook) {
                primaryItem = PrimaryItems.None;
                playerController.SetVisualsLocked(false);

                if (primaryItemVisualReference != null) {
                    Object.Destroy(primaryItemVisualReference);
                    primaryItemVisualReference = null;
                }
            }
        }

        if (releasePrimaryInput) {
            switch (primaryItem) {
                default:
                    break;

                case PrimaryItems.GrapplingHook:
                    if (grapplingHookInUse) {
                        grapplingHookInUse = false;
                        primaryItem = PrimaryItems.None;
                        playerController.SetVisualsLocked(false);
                        playerController.StopGrappling();
                        if (primaryItemVisualReference != null) {
                            Object.Destroy(primaryItemVisualReference);
                            primaryItemVisualReference = null;
                        }
                    }
                    break;
            }
        }

        if (useSecondaryInput) {
            switch (secondaryItem) {
                case SecondaryItems.None:
                    break;
            }
            secondaryItem = SecondaryItems.None;

            if (secondaryItemVisualReference != null) {
                Object.Destroy(secondaryItemVisualReference.gameObject);
                secondaryItemVisualReference = null;
            }
        }

        if (propellerHatDestroyCooldown > 0f) {
            propellerHatDestroyCooldown -= Time.deltaTime;
        }
        else if (propellerHatInUse && jumpItem == JumpItems.PropellerHat) {
            propellerHatInUse = false;
            jumpItem = JumpItems.None;

            if (jumpItemVisualReference != null) {
                Object.Destroy(jumpItemVisualReference.gameObject);
                jumpItemVisualReference = null;
            }
        }
    }
}
