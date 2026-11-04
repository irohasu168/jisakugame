using UnityEngine;

public enum DamageType
{
    BallHit,
    MineExplosion,
    Trap,
    Boss,
    Other
}

public struct DamageInfo
{
    public int Amount;

    public DamageType Type;

    // 文字列の発生源名
    public string Source;

    // 実際に攻撃したオブジェクト
    public GameObject Attacker;

    // 命中位置
    public Vector3 HitPoint;

    // 互換用：古いコードが Point を使っていても動くようにする
    public Vector3 Point;

    // 命中面の向き
    public Vector3 Normal;

    public DamageInfo(int amount, DamageType type, GameObject attacker, Vector3 hitPoint)
    {
        Amount = amount;
        Type = type;
        Source = type.ToString();
        Attacker = attacker;
        HitPoint = hitPoint;
        Point = hitPoint;
        Normal = Vector3.up;
    }

    public DamageInfo(int amount, string source, GameObject attacker, Vector3 hitPoint)
    {
        Amount = amount;
        Source = source;
        Attacker = attacker;
        HitPoint = hitPoint;
        Point = hitPoint;
        Normal = Vector3.up;
        Type = ConvertSourceToType(source);
    }

    private static DamageType ConvertSourceToType(string source)
    {
        switch (source)
        {
            case "BallHit":
                return DamageType.BallHit;

            case "MineExplosion":
                return DamageType.MineExplosion;

            case "Trap":
                return DamageType.Trap;

            case "Boss":
                return DamageType.Boss;

            default:
                return DamageType.Other;
        }
    }
}