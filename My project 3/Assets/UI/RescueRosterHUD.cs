using UnityEngine;
using TMPro;

public class RescueRosterHUD : MonoBehaviour
{
    [SerializeField] private RescueRoster roster;
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        if (roster == null)
            roster = FindFirstObjectByType<RescueRoster>();

        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (roster == null || text == null) return;

        text.text = $"Allies: {roster.Allies.Count}/{roster.MaxAllies}";
    }
}