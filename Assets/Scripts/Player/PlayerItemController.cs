using UnityEngine;

public class PlayerItemController : MonoBehaviour
{
    private PlayerInput input;
    private PlayerController playerController;

    private PrimaryItems primaryItem = PrimaryItems.None;

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

    void Update()
    {
        bool usePrimaryInput = input.Player.UsePrimary.WasPressedThisFrame();
        bool useSecondaryInput = input.Player.UseSecondary.WasPressedThisFrame();

        if (usePrimaryInput) {
            switch (primaryItem) {
                case PrimaryItems.None:
                    break;
                case PrimaryItems.RocketLauncher:
                    Vector3 launchDirection = -cameraPivot.forward * 16f;
                    playerController.AddVelocity(launchDirection);
                    break;
            }
            primaryItem = PrimaryItems.None;
        }
    }
}
