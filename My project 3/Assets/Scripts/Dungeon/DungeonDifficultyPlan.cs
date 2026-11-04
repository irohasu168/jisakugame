using System;
using UnityEngine;

[Serializable]
public class DungeonDifficultyPlan
{
    public DungeonDifficulty dungeonDifficulty = DungeonDifficulty.Normal;

    [Tooltip("•K‚¸10ŒÂ‚É‚·‚é")]
    public DungeonStageData[] stages = new DungeonStageData[10];
}