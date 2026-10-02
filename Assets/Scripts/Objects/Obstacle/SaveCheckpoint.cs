using UnityEngine;
using System.Linq;
using System.Collections;

public class SaveCheckpoint : MonoBehaviour
{
    // Will store all single use item drops in the scene so that they can be restored when the player dies
    private GameObject[] items;
    private PlayerController playerController;

    // Specifies where in the order of checkpoints this checkpoint is. Lower numbers come first (0 is the starting point).
    [SerializeField] public int checkpointIndex = 0;
    
    void Start()
    {
        // Gets a list of all single use items in the scene
        items = GameObject.FindObjectsByType<ItemDrop>(FindObjectsSortMode.None).Where(o => o.IsSingleUse()).Select(o => o.gameObject).ToArray();
        // Gets playerController script
        playerController = FindAnyObjectByType<PlayerController>();
    }

    public void SetCheckpoint()
    {
        // Updates the current checkpoint stored in the player
        playerController.currentCheckpoint = checkpointIndex; // Updates the player's current checkpoint
    }

    private void OnTriggerEnter(Collider other)
    {
        // Checks if it's the player that has collided
        if(other.GetComponent<PlayerController>() != null)
        {
            // Checks if the player has already collected this or a further checkpoint
            if(other.GetComponent<PlayerController>().currentCheckpoint < checkpointIndex)
            {
                SetCheckpoint();
            }
        }
    }
}
