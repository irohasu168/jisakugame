using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PitchBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnController turn;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text pitchText;

    [Header("Pitch Range")]
    [SerializeField] private float pitchMin = -90f;
    [SerializeField] private float pitchMax = 90f;

    [Header("Options")]
    [SerializeField] private bool resetToCenterOnEnable = false;
    [SerializeField] private bool logValue = true;

    private void Awake()
    {
        if (turn == null)
            turn = FindFirstObjectByType<TurnController>();

        if (slider == null)
            slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (slider == null)
        {
            Debug.LogError("[PitchBarUI] Slider is null.", this);
            return;
        }

        slider.minValue = pitchMin;
        slider.maxValue = pitchMax;
        slider.wholeNumbers = true;
        slider.direction = Slider.Direction.BottomToTop;

        slider.onValueChanged.RemoveListener(OnValueChanged);
        slider.onValueChanged.AddListener(OnValueChanged);

        if (resetToCenterOnEnable)
        {
            slider.SetValueWithoutNotify(0f);
        }

        UpdateText(slider.value);
        SendPitchToTurn(slider.value);

        if (logValue)
        {
            string stateText = turn != null ? turn.State.ToString() : "null";
            Debug.Log($"[PitchBarUI] OnEnable value={slider.value}, state={stateText}");
        }
    }

    private void OnDisable()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        UpdateText(value);
        SendPitchToTurn(value);

        if (logValue)
        {
            string stateText = turn != null ? turn.State.ToString() : "null";
            Debug.Log($"[PitchBarUI] value={value}, state={stateText}");
        }
    }

    private void SendPitchToTurn(float value)
    {
        if (turn == null) return;

        if (turn.State == TurnController.TurnState.InputPitch ||
            turn.State == TurnController.TurnState.ReadyToShoot)
        {
            turn.SetPitch(value);
        }
    }

    private void UpdateText(float value)
    {
        if (pitchText != null)
            pitchText.text = $"Pitch: {value:F0}";
    }
}