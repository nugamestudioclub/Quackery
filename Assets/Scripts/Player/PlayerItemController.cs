using UnityEngine;
using UnityEngine.SceneManagement;

// TODO: Make it so if the player hits the ground, their active grappling hook goes away, even if they didn't release LMB

public class PlayerItemController : MonoBehaviour
{
    private PlayerInput input;
    private PlayerController playerController;

    private PrimaryItems[] primaryItem = {PrimaryItems.None, PrimaryItems.None, PrimaryItems.None};
    private int selectedPrimaryItem = 1;
    private JumpItems jumpItem = JumpItems.None;

    // Item references to created visual instances
    // These exist so that they can be deleted later
    private GameObject[] primaryItemVisualReference = {null, null, null};
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
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void KillPlayer()
    {
        RestartLevel();
    }

    public int GetActiveHotbarSlot()
    {
        return selectedPrimaryItem;
    }

    public PrimaryItems[] GetPrimaryItems()
    {
        return primaryItem;
    }

    public JumpItems GetJumpItem()
    {
        return jumpItem;
    }

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

    public bool HasPrimaryItem(PrimaryItems item) {
        return item == primaryItem[0] || item == primaryItem[1] || item == primaryItem[2];
    }

    public bool PrimaryItemsFull() {
        return primaryItem[0] != PrimaryItems.None && primaryItem[1] != PrimaryItems.None && primaryItem[2] != PrimaryItems.None;
    }

    public bool HasJumpItem() {
        return jumpItem != JumpItems.None;
    }

    public void ObtainPrimaryItem(PrimaryItems item)
    {
        int slot = 0;
        if (primaryItem[0] != PrimaryItems.None) {
            slot = 1;
        }
        else if (primaryItem[1] != PrimaryItems.None) {
            slot = 2;
        }
        else if (primaryItem[2] != PrimaryItems.None) {
            return;
        }

        if (primaryItemVisualReference[slot] != null) {
            Object.Destroy(primaryItemVisualReference[slot].gameObject);
            primaryItemVisualReference[slot] = null;
        }

        if (PrimaryItemUtility.ItemLocksPlayerCamera(item) && selectedPrimaryItem == slot - 1) {
            playerController.SetVisualsLocked(true);
        }
        primaryItem[slot] = item;

        // Update visuals
        switch (primaryItem[slot]) {
            case PrimaryItems.RocketLauncher:
                primaryItemVisualReference[slot] = Instantiate(
                    prefabs.rocketLauncher,
                    headItemVisuals.TransformPoint(new Vector3(0.4f, -0.1f, 0f)),
                    headItemVisuals.rotation,
                    headItemVisuals
                );
                break;
            case PrimaryItems.GrapplingHook:
                primaryItemVisualReference[slot] = Instantiate(
                    prefabs.grapplingHook,
                    bodyItemVisuals.TransformPoint(new Vector3(0.45f, 0.8f, 0f)),
                    bodyItemVisuals.rotation,
                    bodyItemVisuals
                );
                break;
            default:
                break;
        }

        if (primaryItemVisualReference[slot] != null && selectedPrimaryItem - 1 != slot) {
            primaryItemVisualReference[slot].SetActive(false);
        }
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
        bool hotbar1Input = input.Player.Hotbar1.WasPressedThisFrame();
        bool hotbar2Input = input.Player.Hotbar2.WasPressedThisFrame();
        bool hotbar3Input = input.Player.Hotbar3.WasPressedThisFrame();
        bool restartLevelInput = input.Player.Restart.WasPressedThisFrame();

        if (restartLevelInput) {
            RestartLevel();
            return;
        }

        if (hotbar1Input) {
            selectedPrimaryItem = 1;
            if (primaryItemVisualReference[0] != null) {
                primaryItemVisualReference[0].SetActive(true);
            }
            if (primaryItemVisualReference[1] != null) {
                primaryItemVisualReference[1].SetActive(false);
            }
            if (primaryItemVisualReference[2] != null) {
                primaryItemVisualReference[2].SetActive(false);
            }
        } else if (hotbar2Input) {
            selectedPrimaryItem = 2;
            if (primaryItemVisualReference[0] != null) {
                primaryItemVisualReference[0].SetActive(false);
            }
            if (primaryItemVisualReference[1] != null) {
                primaryItemVisualReference[1].SetActive(true);
            }
            if (primaryItemVisualReference[2] != null) {
                primaryItemVisualReference[2].SetActive(false);
            }
        } else if (hotbar3Input) {
            selectedPrimaryItem = 3;
            if (primaryItemVisualReference[0] != null) {
                primaryItemVisualReference[0].SetActive(false);
            }
            if (primaryItemVisualReference[1] != null) {
                primaryItemVisualReference[1].SetActive(false);
            }
            if (primaryItemVisualReference[2] != null) {
                primaryItemVisualReference[2].SetActive(true);
            }
        }
        if (PrimaryItemUtility.ItemLocksPlayerCamera(primaryItem[selectedPrimaryItem - 1])) {
            playerController.SetVisualsLocked(true);
        } else {
            playerController.SetVisualsLocked(false);
        }

        int slot = selectedPrimaryItem - 1;

        if (usePrimaryInput) {
            switch (primaryItem[slot]) {
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
            if (primaryItem[slot] != PrimaryItems.GrapplingHook) {
                primaryItem[slot] = PrimaryItems.None;
                playerController.SetVisualsLocked(false);

                if (primaryItemVisualReference[slot] != null) {
                    Object.Destroy(primaryItemVisualReference[slot]);
                    primaryItemVisualReference[slot] = null;
                }
            }
        }

        if (releasePrimaryInput) {
            switch (primaryItem[slot]) {
                default:
                    break;

                case PrimaryItems.GrapplingHook:
                    if (grapplingHookInUse) {
                        grapplingHookInUse = false;
                        primaryItem[slot] = PrimaryItems.None;
                        playerController.SetVisualsLocked(false);
                        playerController.StopGrappling();
                        if (primaryItemVisualReference[slot] != null) {
                            Object.Destroy(primaryItemVisualReference[slot]);
                            primaryItemVisualReference[slot] = null;
                        }
                    }
                    break;
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
