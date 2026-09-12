using UnityEngine;

public class PlayerItemController : MonoBehaviour
{
    private PlayerInput input;
    private PlayerController playerController;

    private PrimaryItems primaryItem = PrimaryItems.None;
    private SecondaryItems secondaryItem = SecondaryItems.None;
    private JumpItems jumpItem = JumpItems.None;

    [SerializeField] private Transform cameraPivot;

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

    public void UseJumpItem()
    {
        switch (jumpItem) {
            case JumpItems.None:
                break;
            case JumpItems.PropellerHat:
                // TODO: Do something
                break;
        }
        jumpItem = JumpItems.None;
    }

    public void ObtainPrimaryItem(PrimaryItems item)
    {
        if (PrimaryItemUtility.ItemLocksPlayerCamera(item)) {
            playerController.SetVisualsLocked(true);
        }
        primaryItem = item;
    }

    public void ObtainSecondaryItem(SecondaryItems item)
    {
        if (SecondaryItemUtility.ItemLocksPlayerCamera(item)) {
            playerController.SetVisualsLocked(true);
        }
        secondaryItem = item;
    }

    public void ObtainJumpItem(JumpItems item)
    {
        if (JumpItemUtility.ItemLocksPlayerCamera(item)) {
            playerController.SetVisualsLocked(true);
        }
        jumpItem = item;
    }

    private void Update()
    {
        bool usePrimaryInput = input.Player.UsePrimary.WasPressedThisFrame();
        bool useSecondaryInput = input.Player.UseSecondary.WasPressedThisFrame();

        if (usePrimaryInput) {
            switch (primaryItem) {
                case PrimaryItems.None:
                    break;
                case PrimaryItems.RocketLauncher:
                    Vector3 launchDirection = -cameraPivot.forward * 19f;
                    playerController.AddVelocity(launchDirection);
                    break;
            }
            primaryItem = PrimaryItems.None;
            playerController.SetVisualsLocked(false);
        }

        if (useSecondaryInput) {
            switch (secondaryItem) {
                case SecondaryItems.None:
                    break;
            }
            secondaryItem = SecondaryItems.None;
        }
    }
}
