using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [SerializeField] private GameDifficulty currentDifficulty = GameDifficulty.Normal;

    public GameDifficulty CurrentDifficulty => currentDifficulty;

    public void SetDifficulty(GameDifficulty difficulty)
    {
        currentDifficulty = difficulty;
        Debug.Log($"[Difficulty] Changed to {currentDifficulty}");
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetDifficulty(GameDifficulty.Beginner);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetDifficulty(GameDifficulty.Easy);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetDifficulty(GameDifficulty.Normal);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetDifficulty(GameDifficulty.Hard);
    }
#endif
}