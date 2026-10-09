using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Objects
    private PlayerInput input;
    private Hotbar hotbarScript;

    // State
    private bool gamePaused = false;


    // --- Public API ---
    public bool isPaused() {
        return gamePaused;
    }

    // --- Unity functions ---
    private void Awake()
    {
        // Input
        input = new PlayerInput();
    }

    private void Start()
    {
        // Lock & hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        hotbarScript = FindFirstObjectByType<Hotbar>();
        if (hotbarScript == null) {
            throw new System.Exception("GameManager couldn't find hotbar. Add UI Canvas to your scene (Prefabs/UI/UI Canvas)");
        }
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Update()
    {
        bool pauseInput = input.Player.Pause.WasPressedThisFrame();

        if (pauseInput) {
            gamePaused = !gamePaused;

            if (gamePaused) {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
            else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
