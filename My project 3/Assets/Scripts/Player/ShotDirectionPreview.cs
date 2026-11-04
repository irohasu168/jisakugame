using UnityEngine;

public class ShotDirectionPreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnController turn;
    [SerializeField] private Transform shooter;
    [SerializeField] private AimIndicator aimIndicator;

    [Header("Power")]
    [SerializeField] private float maxPowerForPreview = 20f;

    [Header("Debug")]
    [SerializeField] private bool logPreview = false;

    private void Awake()
    {
        if (turn == null)
            turn = FindFirstObjectByType<TurnController>();
    }

    private void Update()
    {
        if (turn == null || shooter == null || aimIndicator == null)
            return;

        // 横方向のAim中は AimInputController が表示を管理する
        if (turn.State == TurnController.TurnState.InputAim)
            return;

        bool shouldShow =
            turn.State == TurnController.TurnState.InputPitch ||
            turn.State == TurnController.TurnState.ReadyToShoot;

        if (!shouldShow)
        {
            aimIndicator.SetVisible(false);
            return;
        }

        Vector3 dir = ComputeShotDirection(
            turn.CurrentAimDirXZ,
            turn.CurrentPitchDeg
        );

        float power01 = Mathf.Clamp01(turn.CurrentAimPower / maxPowerForPreview);

        if (logPreview)
        {
            Debug.Log(
                $"[ShotPreview] shooterPos={shooter.position}, " +
                $"pitch={turn.CurrentPitchDeg}, dir={dir}, dirY={dir.y:F2}"
            );
        }

        // ここが重要：Pitch用の矢印も shooter.position から出す
        aimIndicator.SetVisible(true);
        aimIndicator.UpdateIndicator(shooter.position, dir, power01);
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
}