using UnityEngine;

public class BoostPad : MonoBehaviour
{
    [SerializeField] private float boostStrength = 20.0f;
    [SerializeField] private float boostHeightStrength = 8.0f;

    private void OnTriggerEnter(Collider other)
    {
        // If the collider has a PlayerItemController, they are surely a player.
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null) {
            return;
        }

        Vector3 addVelo = transform.forward * boostStrength;
        addVelo += Vector3.up * boostHeightStrength;

        player.SetVelocity(addVelo.x, addVelo.y, addVelo.z);
    }
}
