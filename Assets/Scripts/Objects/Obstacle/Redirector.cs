using UnityEngine;

public class Redirector : MonoBehaviour
{
    private float VELO_LAUNCH_CUTOFF = 3f;

    // NOTE: This should probably be 0 if the redirector is pointing up
    // (unless you want the player to be able to get infinite height...)
    [SerializeField] private float ADDED_VELO = 4f;

    [SerializeField] private Transform ringTransform;
    [SerializeField] private Transform playerTransform; // TODO: Find a better way to get the player

    private void OnTriggerEnter(Collider other)
    {
        // If the collider has a PlayerItemController, they are surely a player.
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null) {
            return;
        }

        float playerMagnitude = player.GetVelocity().magnitude;
        if (playerMagnitude >= VELO_LAUNCH_CUTOFF) {
            Vector3 newVelo = transform.forward * (playerMagnitude + ADDED_VELO);
            player.SetVelocity(newVelo.x, newVelo.y, newVelo.z);
        }
    }
    void Update()
    {
        Vector3 direction = playerTransform.position - ringTransform.position;

        if (direction.magnitude > 2f)
        {
            ringTransform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
