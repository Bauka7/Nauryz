using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [Header("Arrow Physics")]
    private Rigidbody rb;
    private Vector3 direction;
    private float force;
    private OYURangeGameManager gameManager;
    private bool hasHit = false;
    private float lifetime = 15f;
    private float minVelocityThreshold = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = 0.05f; // Легче для лучшего полета
        rb.linearDamping = 0.05f; // Меньше сопротивления
        rb.angularDamping = 0.1f;
        rb.constraints = RigidbodyConstraints.None;

        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 shootDirection, float shootForce, OYURangeGameManager manager)
    {
        direction = shootDirection.normalized;
        force = shootForce;
        gameManager = manager;

        Debug.Log($"🔥 Arrow launched! Direction: {direction}, Force: {force}");

        if (rb != null)
        {
            rb.linearVelocity = direction * force;
            // Правильно ориентируем стрелу: стрела должна смотреть туда, куда летит
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            Debug.Log($"Arrow rotation: {transform.eulerAngles}");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (rb.linearVelocity.magnitude < minVelocityThreshold) return;

        ArcheryTarget target = other.GetComponent<ArcheryTarget>();
        if (target != null)
        {
            hasHit = true;
            int score = target.GetScoreFromHitPoint(transform.position);

            Debug.Log($"🎯 Arrow hit target! Score: {score}, Position: {transform.position}");

            if (gameManager != null)
            {
                gameManager.OnTargetHit(score);
            }

            AttachToTarget(other.transform);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        if (rb.linearVelocity.magnitude < minVelocityThreshold) return;

        hasHit = true;
        Debug.Log($"💥 Arrow hit wall: {collision.gameObject.name}");

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Destroy(gameObject, 5f);
    }

    void AttachToTarget(Transform targetTransform)
    {
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        transform.SetParent(targetTransform);
        Destroy(gameObject, 5f);
    }
}
