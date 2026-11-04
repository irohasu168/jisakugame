using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Stage Config")]
public class StageConfig : ScriptableObject
{
    [Header("Stage Box Size (World Units)")]
    public Vector3 boxSize = new Vector3(14f, 8f, 18f);

    [Header("Wall Thickness")]
    public float wallThickness = 0.5f;

    [Header("Wall Visual")]
    public float wallVisualHeight = 0.6f;
    public float wallVisualThickness = 0.8f;

    [Header("Wall Collider")]
    public float wallColliderHeight = 3.0f;
    public float wallColliderThickness = 0.5f;

    [Header("Grid Blocks")]
    public int gridX = 9;
    public int gridY = 4;
    public int gridZ = 11;
    public Vector3 cellSize = new Vector3(1.2f, 1.2f, 1.2f);

    [Header("Offsets")]
    public Vector3 gridPadding = new Vector3(1.0f, 1.0f, 1.0f);

    [Header("Default Difficulty")]
    public GameDifficulty defaultDifficulty = GameDifficulty.Normal;

    [Header("Difficulty Settings")]
    public StageDifficultySetting[] difficultySettings;

    public StageDifficultySetting GetDifficultySetting(GameDifficulty difficulty)
    {
        if (difficultySettings != null)
        {
            foreach (var setting in difficultySettings)
            {
                if (setting != null && setting.difficulty == difficulty)
                    return setting;
            }
        }

        Debug.LogWarning($"[StageConfig] Difficulty setting not found: {difficulty}. Using first setting.");

        if (difficultySettings != null && difficultySettings.Length > 0)
            return difficultySettings[0];

        return null;
    }
}

[Serializable]
public class StageDifficultySetting
{
    public GameDifficulty difficulty = GameDifficulty.Normal;

    [Header("Blocks")]
    [Range(0f, 1f)] public float blockFillRate = 0.55f;

    [Header("Block Types")]
    [Range(0f, 1f)] public float breakableBlockRate = 0.35f;

    [Header("Safe Area")]
    public float safeRadius = 3.0f;

    [Header("Mines")]
    [Range(0f, 1f)] public float mineFillRate = 0.08f;
    public int mineMaxCount = 12;
    public float mineMinDistanceFromSpawn = 3.0f;

    [Header("Rescue")]
    public int rescueMinCount = 0;
    public int rescueMaxCount = 2;
    public float rescueMinDistanceFromSpawn = 3.5f;
}