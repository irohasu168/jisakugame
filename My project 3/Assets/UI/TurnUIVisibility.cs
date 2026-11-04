using UnityEngine;

public class TurnUIVisibility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnController turn;

    [Header("UI Groups")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private GameObject turnInputRoot;
    [SerializeField] private GameObject debugRoot;

    [Header("Turn Input UI")]
    [SerializeField] private GameObject pitchSlider;
    [SerializeField] private GameObject shootButton;
    [SerializeField] private GameObject cancelButton;

    [Header("Options")]
    [SerializeField] private bool showHudAlways = true;
    [SerializeField] private bool showDebugUI = false;

    [SerializeField] private bool uiLocked = false;

    public void SetUILocked(bool locked)
    {
        uiLocked = locked;

        if (locked)
        {
            if (hudRoot != null)
                hudRoot.SetActive(false);

            if (turnInputRoot != null)
                turnInputRoot.SetActive(false);

            if (debugRoot != null)
                debugRoot.SetActive(false);
        }
    }

    private void Awake()
    {
        if (turn == null)
            turn = FindFirstObjectByType<TurnController>();
    }

    private void Update()
    {

        if (uiLocked)
            return;

        if (turn == null) return;

        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        bool isInputAim = turn.State == TurnController.TurnState.InputAim;
        bool isInputPitch = turn.State == TurnController.TurnState.InputPitch;
        bool isReadyToShoot = turn.State == TurnController.TurnState.ReadyToShoot;
        bool isShotRunning = turn.State == TurnController.TurnState.ShotRunning;
        bool isTurnEnd = turn.State == TurnController.TurnState.TurnEnd;

        // HUDは基本常時表示
        if (hudRoot != null)
            hudRoot.SetActive(showHudAlways);

        // Debug UIは開発時だけ
        if (debugRoot != null)
            debugRoot.SetActive(showDebugUI);

        // 入力UI全体
        bool showTurnInput =
            isInputPitch ||
            isReadyToShoot;

        if (turnInputRoot != null)
            turnInputRoot.SetActive(showTurnInput);

        // PitchSlider：Pitch入力中と発射準備中
        if (pitchSlider != null)
            pitchSlider.SetActive(isInputPitch || isReadyToShoot);

        // ShootButton：ReadyToShootだけ
        if (shootButton != null)
            shootButton.SetActive(isReadyToShoot);

        // CancelButton：Pitch入力中と発射準備中
        if (cancelButton != null)
            cancelButton.SetActive(isInputPitch || isReadyToShoot);
    }
}