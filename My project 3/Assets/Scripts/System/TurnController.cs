using System;
using UnityEngine;

public class TurnController : MonoBehaviour
{
    public enum TurnState { InputAim, InputPitch, ReadyToShoot, ShotRunning, TurnEnd }
    public TurnState State { get; private set; } = TurnState.InputAim;
    public Action OnTurnEnded;
    [SerializeField] private BallController ball;

    // STEP1で確定した値
    public Vector3 AimDirXZ { get; private set; } = Vector3.forward;
    public float AimPower { get; private set; } = 12f;

    // STEP2で設定する値
    public float PitchDeg { get; private set; } = 35f; // 初期値は好みでOK

    [Header("Pitch Range")]
    [SerializeField] private float pitchMin = -90f;
    [SerializeField] private float pitchMax = 90f;

    public Vector3 CurrentAimDirXZ => AimDirXZ;
    public float CurrentAimPower => AimPower;
    public float CurrentPitchDeg => PitchDeg;
    public float CurrentAimPower01 => Mathf.Clamp01(AimPower / 20f);
    private void OnEnable()
    {
        if (ball != null) ball.OnShotStopped += HandleShotStopped;
    }

    private void OnDisable()
    {
        if (ball != null) ball.OnShotStopped -= HandleShotStopped;
    }
    private Vector3 ComputeShotDirection(Vector3 dirXZ, float pitchDeg)
    {
        dirXZ.y = 0f;

        if (dirXZ.sqrMagnitude <= 0.0001f)
            dirXZ = Vector3.forward;

        dirXZ.Normalize();

        float rad = pitchDeg * Mathf.Deg2Rad;

        Vector3 dir =
            dirXZ * Mathf.Cos(rad) +
            Vector3.up * Mathf.Sin(rad);

        dir.Normalize();

        return dir;
    }
    // --- STEP1：指を離した瞬間にYaw+Power確定 ---
    public void SetAim(Vector3 dirXZ, float power)
    {
        AimDirXZ = dirXZ;
        AimPower = power;
    }

    public void ConfirmAim()
    {
        if (State != TurnState.InputAim) return;
        State = TurnState.InputPitch; // 次はPitchバーへ[1](https://discussions.unity.com/t/package-manager-cache-cleanup/932897)
        Debug.Log("State -> InputPitch (Aim confirmed)");
    }

    // --- STEP2：Pitchバーで仰角を設定 ---
    public void SetPitch(float pitchDeg)
    {
        PitchDeg = Mathf.Clamp(pitchDeg, pitchMin, pitchMax);

        Debug.Log($"[Turn] SetPitch called: raw={pitchDeg}, clamped={PitchDeg}");

        if (State == TurnState.InputPitch)
        {
            State = TurnState.ReadyToShoot;
            Debug.Log("[Turn] State -> ReadyToShoot");
        }
    }

    // --- キャンセル：STEP1へ戻る（誤操作救済）[1](https://discussions.unity.com/t/package-manager-cache-cleanup/932897) ---
    public void CancelToAim()
    {
        if (State == TurnState.ShotRunning) return;
        State = TurnState.InputAim;
        Debug.Log("State -> InputAim (Canceled)");
    }

    // --- STEP3：発射ボタンでショット実行 ---
    public void Shoot()
    {
        Debug.Log($"[Turn] Shoot called. State={State}");

        if (ball == null)
        {
            Debug.LogError("[Turn] ball is null.");
            return;
        }

        if (State != TurnState.ReadyToShoot)
        {
            Debug.Log($"[Turn] Shoot blocked. State={State}");
            return;
        }

        State = TurnState.ShotRunning;

        Vector3 dir = ComputeShotDirection(AimDirXZ, PitchDeg);

        ball.Launch(dir, AimPower);

        Debug.Log($"[Turn] Shoot OK power={AimPower:F2} pitch={PitchDeg:F0} dir={dir}");
    }

    private void HandleShotStopped()
    {
        State = TurnState.TurnEnd;
        // Phase1/2は簡易：次ターンへ
        OnTurnEnded?.Invoke();
        State = TurnState.InputAim;
    }
}