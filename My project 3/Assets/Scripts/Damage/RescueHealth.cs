using UnityEngine;

public class RescueHealth : HealthBase
{
    [Header("Rescue behaviour")]
    [SerializeField] private GameObject rescuedVisual;
    [SerializeField] private GameObject trappedVisual;

    public bool IsRescued { get; private set; }

    // 救出成功を外部へ通知
    public System.Action<RescueHealth> OnRescued;

    protected override void OnDead(DamageInfo lastHit)
    {
        if (IsRescued) return;

        IsRescued = true;
        Debug.Log($"[Rescue] Rescued!: {gameObject.name}");

        if (trappedVisual != null) trappedVisual.SetActive(false);
        if (rescuedVisual != null) rescuedVisual.SetActive(true);

        // 当たり判定を消す（もうダメージを受けない）
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        OnRescued?.Invoke(this);
    }
}

