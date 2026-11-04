using UnityEngine;
using System;

public class BlockHealth : HealthBase
{
    [Header("Block behaviour")]
    [SerializeField] private bool destroyOnDead = true;

    protected override void OnDead(DamageInfo lastHit)
    {
        Debug.Log($"[Block] Destroyed: {gameObject.name}");

        OnAnyBlockDestroyed?.Invoke(this);

        if (destroyOnDead)
        {
            Destroy(gameObject);
        }
        else
        {
            // ”j‰ó‚µ‚È‚¢•ûj‚È‚çAŒ©‚½–Ú‚¾‚¯•Ï‚¦‚é“™
            gameObject.SetActive(false);
        }
    }
    public static event Action<BlockHealth> OnAnyBlockDestroyed;

}
