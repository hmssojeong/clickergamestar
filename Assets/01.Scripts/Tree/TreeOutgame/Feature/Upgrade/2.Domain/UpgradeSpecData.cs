using System;
using UnityEngine;

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
    public double CostLinearIncrement = 5;
    public double CostMultiplier = 1.15;

    [Header("효과 설정")]
    public double BaseDamage = 1;
    public double DamageMultiplier = 9;
    public double DamageBonus = 0.5;

    // 특정 레벨의 비용 계산
    public Currency CalculateCost(int currentLevel)
    {
        // 비용 공식: BaseCost * (CostMultiplier ^ currentLevel)
        double cost = BaseCost * Math.Pow(CostMultiplier, currentLevel);
        return new Currency(cost);
    }

    // 특정 레벨의 데미지 계산
    public double CalculateDamage(int currentLevel)
    {
        // 데미지 공식: BaseDamage + (currentLevel * DamageMultiplier)
        return BaseDamage + (currentLevel * DamageMultiplier);
    }
}
