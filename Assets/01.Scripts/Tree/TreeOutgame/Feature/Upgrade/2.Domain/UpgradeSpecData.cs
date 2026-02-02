using System;
using UnityEngine;

/// <summary>
/// 업그레이드 스펙 데이터
/// 업그레이드의 기본 정보와 계산 공식을 포함
/// </summary>
[Serializable]
public class UpgradeSpecData
{
    [Header("기본 정보")]
    public EUpgradeType Type;
    public string Name;
    public string Description;
    public Sprite Icon;

    [Header("레벨 설정")]
    public int MaxLevel = 999;

    [Header("비용 설정")]
    public double BaseCost = 10;
    public double CostMultiplier = 1.15;

    [Header("효과 설정")]
    public double BaseDamage = 1;
    public double DamageMultiplier = 9;

    /// <summary>
    /// 특정 레벨의 비용 계산
    /// </summary>
    public Currency CalculateCost(int currentLevel)
    {
        // 비용 공식: BaseCost * (CostMultiplier ^ currentLevel)
        double cost = BaseCost * Math.Pow(CostMultiplier, currentLevel);
        return new Currency(cost);
    }

    /// <summary>
    /// 특정 레벨의 데미지 계산
    /// </summary>
    public double CalculateDamage(int currentLevel)
    {
        // 데미지 공식: BaseDamage + (currentLevel * DamageMultiplier)
        return BaseDamage + (currentLevel * DamageMultiplier);
    }
}
