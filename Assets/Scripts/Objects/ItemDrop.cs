using UnityEngine;
using UnityEngine.InputSystem;

public class ItemDrop : MonoBehaviour
{
    // By default, all fields are "None". These can be set in the editor to pick an item to give on collision with the player
    [SerializeField] private PrimaryItems primaryItem = PrimaryItems.None;
    [SerializeField] private SecondaryItems secondaryItem = SecondaryItems.None;
    [SerializeField] private JumpItems jumpItem = JumpItems.None;
    [SerializeField] private bool singleUse = false; // Item drop disappears after pickup

    private void Awake()
    {
        // To fix mistakes, use the first set item. Ideally only one item is set in the scene, though.
        if (jumpItem != JumpItems.None && (secondaryItem != SecondaryItems.None || primaryItem != PrimaryItems.None)) {
            jumpItem = JumpItems.None;
        }

        if (secondaryItem != SecondaryItems.None && primaryItem != PrimaryItems.None) {
            secondaryItem = SecondaryItems.None;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the collider has a PlayerItemController, they are surely a player, so we can give them the item.
        PlayerItemController player = other.GetComponent<PlayerItemController>();

        if (player == null) {
            return;
        }

        if (primaryItem != PrimaryItems.None) {
            player.ObtainPrimaryItem(primaryItem);
        }
        else if (secondaryItem != SecondaryItems.None) {
            // TODO: Obtain secondary item
        }
        else if (jumpItem != JumpItems.None) {
            // TODO: Obtain jump item
        }

        if (singleUse) {
            Destroy(gameObject);
        }
    }
}
