using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SlowZoneGimmick : MonoBehaviour
{
    [Header("Slow")]
    [SerializeField] private float slowRate = 0.55f;
    [SerializeField] private float minSpeed = 1.5f;

    [Header("Target")]
    [SerializeField] private string ballTag = "Ball";

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(ballTag))
            return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
            return;

        Vector3 velocity = rb.velocity;

        if (velocity.sqrMagnitude <= 0.0001f)
            return;

        velocity *= slowRate;

        if (velocity.magnitude < minSpeed)
            velocity = velocity.normalized * minSpeed;

        rb.velocity = velocity;

        Debug.Log($"[SlowZone] Slowed ball. speed={rb.velocity.magnitude:F2}");
    }
}