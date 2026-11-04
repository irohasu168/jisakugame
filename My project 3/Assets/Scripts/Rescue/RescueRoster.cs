using System.Collections.Generic;
using UnityEngine;

public class RescueRoster : MonoBehaviour
{
    private static RescueRoster instance;

    [Header("Roster")]
    [SerializeField] private int maxAllies = 3;

    public List<string> Allies { get; private set; } = new List<string>();
    public int MaxAllies => maxAllies;
    public int Count => Allies.Count;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // 今は同一シーン内でダンジョンを進めるので DontDestroyOnLoad は不要
        // DontDestroyOnLoad(gameObject);
    }

    public void AddRescued(string allyId)
    {
        if (string.IsNullOrEmpty(allyId))
            return;

        if (!Allies.Contains(allyId))
        {
            Allies.Add(allyId);
        }

        while (Allies.Count > maxAllies)
        {
            Allies.RemoveAt(0);
        }

        Debug.Log($"[RescueRoster] Allies = {Allies.Count}/{maxAllies}");
    }

    public void AddRescued(RescueHealth rescue)
    {
        if (rescue == null)
            return;

        AddRescued(rescue.gameObject.name);
    }

    public void RemoveRescued(string allyId)
    {
        if (string.IsNullOrEmpty(allyId))
            return;

        Allies.Remove(allyId);

        Debug.Log($"[RescueRoster] Removed. Allies = {Allies.Count}/{maxAllies}");
    }

    public void Clear()
    {
        Allies.Clear();

        Debug.Log("[RescueRoster] Cleared");
    }

    public bool HasAlly(string allyId)
    {
        return Allies.Contains(allyId);
    }
}