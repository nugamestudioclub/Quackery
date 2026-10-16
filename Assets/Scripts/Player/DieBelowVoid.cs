using UnityEngine;

public class DieBelowVoid : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerItemController playerItemController;

    // How far below the void you can be before you die
    [SerializeField] private float VOID_GRACE_HEIGHT = 2;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        playerItemController = FindFirstObjectByType<PlayerItemController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.transform.position.y + VOID_GRACE_HEIGHT <= transform.position.y) {
            playerItemController.KillPlayer();
        }
    }
}
