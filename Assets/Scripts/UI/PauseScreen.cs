using UnityEngine;

public class PauseScreen : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private GameObject components;
    [SerializeField] private GameObject settingsScreen;

    private CanvasRenderer canvasRenderer;

    private bool wasPausedLastFrame = true;
    private bool settingsScreenShown = false;

    public void SettingsScreenToggle()
    {
        if (settingsScreenShown) {
            settingsScreen.SetActive(false);
        }
        else {
            settingsScreen.SetActive(true);
        }

        settingsScreenShown = !settingsScreenShown;
    }

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) {
            throw new System.Exception("PauseScreen couldn't find GameManager. Add one to your scene (Prefabs/Core/GameManager)");
        }

        if (components == null) {
            throw new System.Exception("PauseScreen components was null");
        }

        if (components == null) {
            throw new System.Exception("PauseScreen: no settingsScreen set");
        }

        canvasRenderer = GetComponent<CanvasRenderer>();
        if (canvasRenderer == null) {
            throw new System.Exception("PauseScreen has no CanvasRenderer");
        }

        components.SetActive(true);
        canvasRenderer.SetAlpha(1f);
    }

    private void Update()
    {
        if (gameManager.isPaused() != wasPausedLastFrame)
        {
            wasPausedLastFrame = gameManager.isPaused();
            if (gameManager.isPaused()) {
                components.SetActive(true);
                canvasRenderer.SetAlpha(1f);

                settingsScreenShown = false;
                settingsScreen.SetActive(false);
            }
            else {
                components.SetActive(false);
                canvasRenderer.SetAlpha(0f);
            }
        }
    }
}
