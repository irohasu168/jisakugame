using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MinePickup : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private int addAmount = 1;
    [SerializeField] private string ballTag = "Ball";

    [Header("References")]
    [SerializeField] private MineStackManager stackManager;

    [Header("Options")]
    [SerializeField] private bool destroyOnPickup = true;

    private bool picked;

    private void Awake()
    {
        if (stackManager == null)
            stackManager = FindFirstObjectByType<MineStackManager>();

        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (picked)
            return;

        if (!other.CompareTag(ballTag))
            return;

        if (stackManager == null)
        {
            Debug.LogWarning("[MinePickup] MineStackManager is null.");
            return;
        }

        bool added = stackManager.AddMine(addAmount);

        if (!added)
        {
            Debug.Log("[MinePickup] Mine stack is full.");
            return;
        }

        picked = true;

        Debug.Log($"[MinePickup] Picked by {other.name}. add={addAmount}");

        if (destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}