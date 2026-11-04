using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WarpPortalGimmick : MonoBehaviour
{
    [Header("Warp")]
    [SerializeField] private Transform exitPoint;
    [SerializeField] private float exitOffset = 0.8f;
    [SerializeField] private float cooldown = 0.4f;

    [Header("Target")]
    [SerializeField] private string ballTag = "Ball";

    private float lastWarpTime = -999f;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastWarpTime < cooldown)
            return;

        if (!other.CompareTag(ballTag))
            return;

        if (exitPoint == null)
            return;

        Rigidbody rb = other.attachedRigidbody;

        Vector3 velocity = rb != null
            ? rb.velocity
            : Vector3.zero;

        Vector3 exitPos =
            exitPoint.position +
            exitPoint.forward.normalized * exitOffset;

        other.transform.position = exitPos;

        if (rb != null)
        {
            rb.velocity = exitPoint.forward.normalized * velocity.magnitude;
        }

        lastWarpTime = Time.time;

        WarpPortalGimmick paired =
            exitPoint.GetComponent<WarpPortalGimmick>();

        if (paired != null)
            paired.SetCooldownNow();

        Debug.Log($"[WarpPortal] Warped to {exitPoint.name}");
    }

    public void SetCooldownNow()
    {
        lastWarpTime = Time.time;
    }
}