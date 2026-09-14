using UnityEngine;

public class RocketProj : MonoBehaviour
{
    [SerializeField] private float speed = 50f;
    [SerializeField] private float smokeCooldown = 0.05f;
    [SerializeField] private PrefabDatabase prefabs;

    private float activeSmokeCooldown = 0f;

    private BoxCollider box;

    private Vector3 originalPos;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();
        originalPos = transform.position;
    }

    private void Explode() {
        Object.Destroy(gameObject);

        for (int i = 0; i < 8; i++) {
            GameObject obj = Instantiate(
                prefabs.smoke,
                transform.TransformPoint(new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                )),
                transform.rotation
            );
            Smoke scr = obj.GetComponent<Smoke>();
            if (scr) {
                scr.setProps(1f, Random.Range(2f, 2.5f), Random.Range(3.5f, 5f));
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<CharacterController>() != null) {
            return; // Don't collide with player
        }

        Explode();
    }

    void Update()
    {
        // If a rocket gets too far, just blow it up
        if (Vector3.Distance(transform.position, originalPos) >= 250f) {
            Explode();
        }

        if (activeSmokeCooldown <= 0.0f) {
            activeSmokeCooldown = smokeCooldown;
            Instantiate(
                prefabs.smoke,
                transform.position,
                transform.rotation
            );
        }

        Vector3 direction = transform.forward;

        Vector3 center = transform.TransformPoint(box.center);
        Vector3 halfExtents = Vector3.Scale(box.size, transform.lossyScale) * 0.5f;

        float distance = speed * Time.deltaTime;

        if (Physics.BoxCast(
            center,
            halfExtents,
            direction,
            out RaycastHit hit,
            transform.rotation,
            distance
        )) {
            transform.position += direction * Mathf.Max(0f, hit.distance - 0.001f);
            Explode();
        }
        else {
            transform.position += direction * distance;
        }

        activeSmokeCooldown -= Time.deltaTime;
    }
}
