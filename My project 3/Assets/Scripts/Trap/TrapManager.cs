using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnController turn;
    [SerializeField] private StageConfig config;
    [SerializeField] private Transform stageRoot;
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private GameObject trapWallPrefab;

    [Header("Trap Settings")]
    [SerializeField] private int trapLifeTurns = 2; // 企画書：寿命2T[1](https://stackoverflow.com/questions/63895695/namespace-name-cinemachine-could-not-be-found)
    [SerializeField] private int maxTraps = 3;

    [Header("Spawn Rules")]
    [SerializeField] private float safeRadius = 3.0f;      // spawn周りは避ける[1](https://stackoverflow.com/questions/63895695/namespace-name-cinemachine-could-not-be-found)
    [SerializeField] private float wallPadding = 1.0f;     // 壁にめり込まないため内側に寄せる
    [SerializeField] private float yMin = -1.0f;           // 高さ範囲（適宜調整）
    [SerializeField] private float yMax = 1.0f;

    // 生成したトラップと残り寿命
    private readonly List<(GameObject go, int life)> traps = new();

    private void Awake()
    {
        if (turn == null) turn = FindFirstObjectByType<TurnController>();
    }

    private void OnEnable()
    {
        if (turn != null) turn.OnTurnEnded += HandleTurnEnded;
    }

    private void OnDisable()
    {
        if (turn != null) turn.OnTurnEnded -= HandleTurnEnded;
    }

    private void HandleTurnEnded()
    {
        // 1) 既存トラップの寿命を減らして、0なら削除（寿命2T）[1](https://stackoverflow.com/questions/63895695/namespace-name-cinemachine-could-not-be-found)
        for (int i = traps.Count - 1; i >= 0; i--)
        {
            var t = traps[i];
            t.life--;

            if (t.life <= 0)
            {
                if (t.go != null) Destroy(t.go);
                traps.RemoveAt(i);
            }
            else
            {
                traps[i] = t;
            }
        }

        // 2) 新規生成
        if (trapWallPrefab == null || stageRoot == null || config == null) return;
        if (traps.Count >= maxTraps) return;

        Vector3 pos = FindSpawnPosInsideBox();

        // safeRadius内ならリトライ（数回試す）
        for (int tries = 0; tries < 10; tries++)
        {
            if (playerSpawn == null) break;
            if (Vector3.Distance(pos, playerSpawn.position) >= safeRadius) break;
            pos = FindSpawnPosInsideBox();
        }

        var go = Instantiate(trapWallPrefab, pos, Quaternion.identity, stageRoot);
        traps.Add((go, trapLifeTurns));

        Debug.Log("[Trap] Spawned wall trap");
    }

    private Vector3 FindSpawnPosInsideBox()
    {
        // boxSizeから内側範囲を決める（壁に埋まらないよう padding で縮める）
        Vector3 size = config.boxSize;

        float x = Random.Range(-size.x * 0.5f + wallPadding, size.x * 0.5f - wallPadding);
        float z = Random.Range(-size.z * 0.5f + wallPadding, size.z * 0.5f - wallPadding);
        float y = Random.Range(yMin, yMax);

        return new Vector3(x, y, z);
    }
}
