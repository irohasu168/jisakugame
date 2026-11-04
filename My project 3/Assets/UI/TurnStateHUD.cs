using UnityEngine;
using TMPro;

public class TurnStateHUD : MonoBehaviour
{
    [SerializeField] private TurnController turn;
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        if (turn == null)
            turn = FindFirstObjectByType<TurnController>();

        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (turn == null || text == null) return;

        text.text = $"Turn: {turn.State}";
    }
}
