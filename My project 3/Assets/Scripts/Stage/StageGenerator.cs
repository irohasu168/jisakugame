using System;
using System.Collections.Generic;
using UnityEngine;

public class StageGenerator : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private StageConfig config;

    [Header("Hierarchy")]
    [SerializeField] private Transform stageRoot;
    [SerializeField] private Transform playerSpawn;

    [Header("Wall Prefabs")]
    [SerializeField] private GameObject wallVisualStraightPrefab;
    [SerializeField] private GameObject wallVisualCornerPrefab;

    [Header("Block Prefabs")]
    [SerializeField] private GameObject normalBlockPrefab;
    [SerializeField] private GameObject breakableBlockPrefab;

    [Header("Other Prefabs")]
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private GameObject rescuePrefab;

    [Header("Physics")]
    [SerializeField] private PhysicMaterial bounceMaterial;

    [Header("Seed")]
    [SerializeField] private int stageSeed = 12345;
    [SerializeField] private bool generateOnStart = false;

    private System.Random rng;
    private StageDifficultySetting difficulty;

    private readonly HashSet<Vector3Int> occupied = new HashSet<Vector3Int>();

    public Action<int> OnStageGenerated;

    private void Start()
    {
        if (generateOnStart)
            Generate(stageSeed);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            stageSeed = UnityEngine.Random.Range(1, int.MaxValue);
            Generate(stageSeed);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Generate(stageSeed);
        }
#endif
    }

    public void Generate(int seed)
    {
        if (config == null || stageRoot == null || playerSpawn == null)
        {
            Debug.LogError("[StageGenerator] config / stageRoot / playerSpawn is missing.", this);
            return;
        }

        ClearStage();
        occupied.Clear();

        rng = new System.Random(seed);

        DifficultyManager difficultyManager = FindFirstObjectByType<DifficultyManager>();

        GameDifficulty currentDifficulty =
            difficultyManager != null
                ? difficultyManager.CurrentDifficulty
                : config.defaultDifficulty;

        difficulty = config.GetDifficultySetting(currentDifficulty);

        if (difficulty == null)
        {
            Debug.LogError("[StageGenerator] Difficulty setting is null.", this);
            return;
        }

        Debug.Log($"[StageGenerator] Generate difficulty={currentDifficulty}, seed={seed}");

        BuildBox(config.boxSize, config.wallThickness);

        PlaceBlocks();
        PlaceMines();
        PlaceRescues();

        Debug.Log($"[StageGenerator] Stage Generated. seed={seed}");

        OnStageGenerated?.Invoke(seed);
    }

    private void ClearStage()
    {
        if (stageRoot == null)
            return;

        for (int i = stageRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(stageRoot.GetChild(i).gameObject);
        }
    }

    // --------------------------------------------------
    // Stage Box
    // --------------------------------------------------

    private void BuildBox(Vector3 size, float floorThickness)
    {
        Vector3 center = Vector3.zero;

        // 床は見える状態で生成
        CreateFloor(
            "Floor",
            center + new Vector3(0f, -size.y * 0.5f, 0f),
            new Vector3(size.x, floorThickness, size.z)
        );

        // 天井は透明Colliderだけ生成
        // BallがPitchで上に飛びすぎてもステージ外へ抜けないようにする
        CreateInvisibleCeilingCollider(
            "CeilingCollider",
            center + new Vector3(0f, size.y * 0.5f, 0f),
            new Vector3(size.x, floorThickness, size.z)
        );

        // カメラを邪魔しにくい低い見た目用の壁
        CreateWallVisuals(size);

        // Ballを止めるための透明な高めCollider壁
        CreateWallColliders(size);
    }

    private void CreateFloor(string name, Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

        go.name = name;
        go.transform.SetParent(stageRoot, false);
        go.transform.position = pos;
        go.transform.localScale = scale;

        Collider col = go.GetComponent<Collider>();

        if (col != null && bounceMaterial != null)
            col.material = bounceMaterial;
    }

    private void CreateInvisibleCeilingCollider(string name, Vector3 pos, Vector3 scale)
    {
        GameObject go = new GameObject(name);

        go.transform.SetParent(stageRoot, false);
        go.transform.position = pos;
        go.transform.localScale = scale;

        BoxCollider col = go.AddComponent<BoxCollider>();

        if (bounceMaterial != null)
            col.material = bounceMaterial;

        // あえてStageWall Layerにはしない
        // Cinemachine Colliderが天井を障害物扱いしてカメラが暴れるのを避けるため
    }

    private void CreateWallVisuals(Vector3 size)
    {
        Transform root = CreateRoot("WallVisualRoot");

        float y = -size.y * 0.5f + config.wallVisualHeight * 0.5f;

        float halfX = size.x * 0.5f;
        float halfZ = size.z * 0.5f;

        float visualHeight = config.wallVisualHeight;
        float visualThickness = config.wallVisualThickness;

        // Z方向の外周
        CreateWallVisual(
            "WallVisual_North",
            new Vector3(0f, y, halfZ),
            new Vector3(size.x, visualHeight, visualThickness),
            root
        );

        CreateWallVisual(
            "WallVisual_South",
            new Vector3(0f, y, -halfZ),
            new Vector3(size.x, visualHeight, visualThickness),
            root
        );

        // X方向の外周
        CreateWallVisual(
            "WallVisual_Left",
            new Vector3(-halfX, y, 0f),
            new Vector3(visualThickness, visualHeight, size.z),
            root
        );

        CreateWallVisual(
            "WallVisual_Right",
            new Vector3(halfX, y, 0f),
            new Vector3(visualThickness, visualHeight, size.z),
            root
        );

        // 角パーツ
        CreateWallCornerVisual(
            "WallVisual_Corner_FL",
            new Vector3(-halfX, y, halfZ),
            new Vector3(visualThickness, visualHeight, visualThickness),
            root
        );

        CreateWallCornerVisual(
            "WallVisual_Corner_FR",
            new Vector3(halfX, y, halfZ),
            new Vector3(visualThickness, visualHeight, visualThickness),
            root
        );

        CreateWallCornerVisual(
            "WallVisual_Corner_BL",
            new Vector3(-halfX, y, -halfZ),
            new Vector3(visualThickness, visualHeight, visualThickness),
            root
        );

        CreateWallCornerVisual(
            "WallVisual_Corner_BR",
            new Vector3(halfX, y, -halfZ),
            new Vector3(visualThickness, visualHeight, visualThickness),
            root
        );
    }

    private void CreateWallVisual(string name, Vector3 pos, Vector3 scale, Transform root)
    {
        GameObject go;

        if (wallVisualStraightPrefab != null)
        {
            go = Instantiate(wallVisualStraightPrefab, pos, Quaternion.identity, root);
            go.name = name;
            go.transform.localScale = scale;
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
        }

        RemoveColliders(go);
    }

    private void CreateWallCornerVisual(string name, Vector3 pos, Vector3 scale, Transform root)
    {
        GameObject go;

        if (wallVisualCornerPrefab != null)
        {
            go = Instantiate(wallVisualCornerPrefab, pos, Quaternion.identity, root);
            go.name = name;
            go.transform.localScale = scale;
        }
        else if (wallVisualStraightPrefab != null)
        {
            go = Instantiate(wallVisualStraightPrefab, pos, Quaternion.identity, root);
            go.name = name;
            go.transform.localScale = scale;
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
        }

        RemoveColliders(go);
    }

    private void CreateWallColliders(Vector3 size)
    {
        Transform root = CreateRoot("WallColliderRoot");

        // 透明Collider壁はステージ全体の高さをカバーする
        float y = 0f;

        float halfX = size.x * 0.5f;
        float halfZ = size.z * 0.5f;

        float colliderHeight = Mathf.Max(config.wallColliderHeight, size.y);
        float colliderThickness = config.wallColliderThickness;

        CreateInvisibleWallCollider(
            "WallCollider_North",
            new Vector3(0f, y, halfZ),
            new Vector3(size.x, colliderHeight, colliderThickness),
            root
        );

        CreateInvisibleWallCollider(
            "WallCollider_South",
            new Vector3(0f, y, -halfZ),
            new Vector3(size.x, colliderHeight, colliderThickness),
            root
        );

        CreateInvisibleWallCollider(
            "WallCollider_Left",
            new Vector3(-halfX, y, 0f),
            new Vector3(colliderThickness, colliderHeight, size.z),
            root
        );

        CreateInvisibleWallCollider(
            "WallCollider_Right",
            new Vector3(halfX, y, 0f),
            new Vector3(colliderThickness, colliderHeight, size.z),
            root
        );
    }

    private void CreateInvisibleWallCollider(string name, Vector3 pos, Vector3 scale, Transform root)
    {
        GameObject go = new GameObject(name);

        go.transform.SetParent(root, false);
        go.transform.position = pos;
        go.transform.localScale = scale;

        BoxCollider col = go.AddComponent<BoxCollider>();

        col.isTrigger = false;

        if (bounceMaterial != null)
            col.material = bounceMaterial;

        int stageWallLayer = LayerMask.NameToLayer("StageWall");

        if (stageWallLayer >= 0)
            go.layer = stageWallLayer;
    }

    private Transform CreateRoot(string name)
    {
        GameObject root = new GameObject(name);

        root.transform.SetParent(stageRoot, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        return root.transform;
    }

    private void RemoveColliders(GameObject target)
    {
        if (target == null)
            return;

        Collider[] colliders = target.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            if (col != null)
                Destroy(col);
        }
    }

    // --------------------------------------------------
    // Grid helpers
    // --------------------------------------------------

    private Vector3 GridInnerMin()
    {
        Vector3 size = config.boxSize;

        return new Vector3(
            -size.x * 0.5f + config.gridPadding.x,
            -size.y * 0.5f + config.gridPadding.y,
            -size.z * 0.5f + config.gridPadding.z
        );
    }

    private Vector3 CellToWorld(Vector3Int cell)
    {
        Vector3 min = GridInnerMin();

        return min + new Vector3(
            cell.x * config.cellSize.x,
            cell.y * config.cellSize.y,
            cell.z * config.cellSize.z
        );
    }

    private bool IsInSafeArea(Vector3 worldPos, float radius)
    {
        return Vector3.Distance(worldPos, playerSpawn.position) < radius;
    }

    private bool TryOccupy(Vector3Int cell)
    {
        if (occupied.Contains(cell))
            return false;

        occupied.Add(cell);
        return true;
    }

    private float Next01()
    {
        return (float)rng.NextDouble();
    }

    // --------------------------------------------------
    // Blocks
    // --------------------------------------------------

    private void PlaceBlocks()
    {
        for (int y = 0; y < config.gridY; y++)
        {
            for (int x = 0; x < config.gridX; x++)
            {
                for (int z = 0; z < config.gridZ; z++)
                {
                    if (Next01() > difficulty.blockFillRate)
                        continue;

                    Vector3Int cell = new Vector3Int(x, y, z);
                    Vector3 pos = CellToWorld(cell);

                    if (IsInSafeArea(pos, difficulty.safeRadius))
                        continue;

                    if (!TryOccupy(cell))
                        continue;

                    GameObject selectedBlockPrefab = ChooseBlockPrefab();

                    if (selectedBlockPrefab == null)
                    {
                        Debug.LogWarning("[StageGenerator] Block prefab is null. Skipped block.");
                        continue;
                    }

                    Spawn(
                        selectedBlockPrefab,
                        "Block",
                        pos,
                        config.cellSize * 0.9f,
                        applyBounce: true
                    );
                }
            }
        }
    }

    private GameObject ChooseBlockPrefab()
    {
        bool useBreakable =
            breakableBlockPrefab != null &&
            difficulty != null &&
            Next01() < difficulty.breakableBlockRate;

        if (useBreakable)
            return breakableBlockPrefab;

        return normalBlockPrefab;
    }

    // --------------------------------------------------
    // Mines
    // --------------------------------------------------

    private void PlaceMines()
    {
        if (minePrefab == null)
            return;

        int count = 0;

        for (int y = 0; y < config.gridY && count < difficulty.mineMaxCount; y++)
        {
            for (int x = 0; x < config.gridX && count < difficulty.mineMaxCount; x++)
            {
                for (int z = 0; z < config.gridZ && count < difficulty.mineMaxCount; z++)
                {
                    if (Next01() > difficulty.mineFillRate)
                        continue;

                    Vector3Int cell = new Vector3Int(x, y, z);

                    if (occupied.Contains(cell))
                        continue;

                    Vector3 pos = CellToWorld(cell);

                    if (IsInSafeArea(pos, difficulty.safeRadius))
                        continue;

                    if (IsInSafeArea(pos, difficulty.mineMinDistanceFromSpawn))
                        continue;

                    if (!TryOccupy(cell))
                        continue;

                    Spawn(
                        minePrefab,
                        "Mine",
                        pos,
                        Vector3.one,
                        applyBounce: false
                    );

                    count++;
                }
            }
        }

        Debug.Log($"[StageGenerator] Mines placed={count}");
    }

    // --------------------------------------------------
    // Rescues
    // --------------------------------------------------

    private void PlaceRescues()
    {
        if (rescuePrefab == null)
            return;

        int targetCount = rng.Next(
            difficulty.rescueMinCount,
            difficulty.rescueMaxCount + 1
        );

        int placed = 0;

        List<Vector3Int> candidates = new List<Vector3Int>();

        for (int y = 0; y < config.gridY; y++)
        {
            for (int x = 0; x < config.gridX; x++)
            {
                for (int z = 0; z < config.gridZ; z++)
                {
                    Vector3Int cell = new Vector3Int(x, y, z);

                    if (occupied.Contains(cell))
                        continue;

                    Vector3 pos = CellToWorld(cell);

                    if (IsInSafeArea(pos, difficulty.safeRadius))
                        continue;

                    if (IsInSafeArea(pos, difficulty.rescueMinDistanceFromSpawn))
                        continue;

                    candidates.Add(cell);
                }
            }
        }

        Shuffle(candidates);

        for (int i = 0; i < candidates.Count && placed < targetCount; i++)
        {
            Vector3Int cell = candidates[i];

            if (!TryOccupy(cell))
                continue;

            Vector3 pos = CellToWorld(cell);

            Spawn(
                rescuePrefab,
                "Rescue",
                pos,
                Vector3.one,
                applyBounce: false
            );

            placed++;
        }

        Debug.Log($"[StageGenerator] Rescues placed={placed}/{targetCount}");
    }

    private void Shuffle(List<Vector3Int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);

            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // --------------------------------------------------
    // Spawn
    // --------------------------------------------------

    private void Spawn(
        GameObject prefab,
        string fallbackName,
        Vector3 pos,
        Vector3 scale,
        bool applyBounce
    )
    {
        GameObject go;

        if (prefab != null)
        {
            go = Instantiate(prefab, pos, Quaternion.identity, stageRoot);
            go.name = prefab.name;
            go.transform.localScale = scale;
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = fallbackName;
            go.transform.SetParent(stageRoot, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
        }

        if (applyBounce)
        {
            Collider col = go.GetComponent<Collider>();

            if (col != null && bounceMaterial != null)
                col.material = bounceMaterial;
        }
    }
}