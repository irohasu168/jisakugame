using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageWallGimmick : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageCooldown = 0.3f;

    [Header("Physics Penalty")]
    [SerializeField] private float velocityPenaltyRate = 0.35f;
    [SerializeField] private float knockbackPower = 3f;

    [Header("Target")]
    [SerializeField] private string ballTag = "Ball";

    [Header("Debug")]
    [SerializeField] private bool logHit = true;

    private float lastDamageTime = -999f;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other, null, "Trigger");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null)
            return;

        TryDamage(collision.collider, collision, "Collision");
    }

    private void TryDamage(Collider hitCollider, Collision collision, string hitType)
    {
        if (hitCollider == null)
            return;

        bool isBall =
            hitCollider.CompareTag(ballTag) ||
            hitCollider.GetComponentInParent<PlayerHealth>() != null;

        if (!isBall)
        {
            if (logHit)
            {
                Debug.Log($"[DamageWall] Ignore {hitCollider.name}. tag={hitCollider.tag}");
            }

            return;
        }

        if (Time.time - lastDamageTime < damageCooldown)
            return;

        PlayerHealth playerHealth =
            hitCollider.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning($"[DamageWall] PlayerHealth not found on {hitCollider.name}");
            return;
        }

        Rigidbody rb = hitCollider.attachedRigidbody;

        if (rb != null)
        {
            rb.velocity *= velocityPenaltyRate;

            Vector3 knockbackDir = Vector3.zero;

            if (collision != null && collision.contacts.Length > 0)
            {
                knockbackDir = collision.contacts[0].normal;
            }
            else
            {
                Vector3 fromWallToBall = hitCollider.transform.position - transform.position;
                fromWallToBall.y = 0f;

                if (fromWallToBall.sqrMagnitude > 0.0001f)
                {
                    knockbackDir = fromWallToBall.normalized;
                }
            }

            if (knockbackDir.sqrMagnitude > 0.0001f)
            {
                rb.AddForce(knockbackDir * knockbackPower, ForceMode.Impulse);
            }
        }

        playerHealth.ApplyDamage(damage, "DamageWall");

        lastDamageTime = Time.time;

        if (logHit)
        {
            Debug.Log(
                $"[DamageWall] HitType={hitType}, Target={hitCollider.name}, Damage={damage}, " +
                $"PlayerHP={playerHealth.CurrentHP}/{playerHealth.MaxHP}"
            );
        }
    }
}