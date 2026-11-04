using System;
using UnityEngine;

[Serializable]
public class DungeonStageData
{
    [Min(1)]
    public int stageNumber = 1;

    public GameDifficulty stageDifficulty = GameDifficulty.Normal;

    public bool isBossStage = false;
}
