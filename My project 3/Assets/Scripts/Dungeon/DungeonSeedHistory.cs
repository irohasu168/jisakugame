using System.Collections.Generic;
using UnityEngine;

public static class DungeonSeedHistory
{
    private const string KeyPrefix = "ClearedDungeonSeeds_";

    private static string GetKey(DungeonDifficulty difficulty)
    {
        return KeyPrefix + difficulty;
    }

    public static bool HasCleared(DungeonDifficulty difficulty, int seed)
    {
        List<int> seeds = LoadClearedSeeds(difficulty);
        return seeds.Contains(seed);
    }

    public static void AddCleared(DungeonDifficulty difficulty, int seed)
    {
        List<int> seeds = LoadClearedSeeds(difficulty);

        if (!seeds.Contains(seed))
        {
            seeds.Add(seed);
            SaveClearedSeeds(difficulty, seeds);
            Debug.Log($"[DungeonSeedHistory] Cleared seed saved. difficulty={difficulty}, seed={seed}");
        }
    }

    public static List<int> LoadClearedSeeds(DungeonDifficulty difficulty)
    {
        string key = GetKey(difficulty);
        string raw = PlayerPrefs.GetString(key, "");

        List<int> result = new List<int>();

        if (string.IsNullOrEmpty(raw))
            return result;

        string[] parts = raw.Split(',');

        foreach (string part in parts)
        {
            if (int.TryParse(part, out int seed))
                result.Add(seed);
        }

        return result;
    }

    private static void SaveClearedSeeds(DungeonDifficulty difficulty, List<int> seeds)
    {
        string key = GetKey(difficulty);
        string raw = string.Join(",", seeds);

        PlayerPrefs.SetString(key, raw);
        PlayerPrefs.Save();
    }

    public static void ClearHistory(DungeonDifficulty difficulty)
    {
        PlayerPrefs.DeleteKey(GetKey(difficulty));
        PlayerPrefs.Save();

        Debug.Log($"[DungeonSeedHistory] Cleared history reset. difficulty={difficulty}");
    }

    public static void ClearAllHistory()
    {
        foreach (DungeonDifficulty difficulty in System.Enum.GetValues(typeof(DungeonDifficulty)))
        {
            ClearHistory(difficulty);
        }
    }
}