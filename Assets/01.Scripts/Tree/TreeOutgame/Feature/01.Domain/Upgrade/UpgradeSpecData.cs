using System;
using UnityEngine;

[Serializable]
public class UpgradeSpecData
{
    [Header("Basic Info")]
    public EUpgradeType Type;
    public string Name;
    public string Description;
    public Sprite Icon;

    [Header("Level Settings")]
    public int MaxLevel = 999;

    [Header("Cost Settings")]
    public double BaseCost = 10;
    public double CostLinearIncrement = 5;
    public double CostMultiplier = 1.15;

    [Header("Effect Settings")]
    public int BaseDamage = 1;
    public int DamageMultiplier = 9;
    public int DamageBonus = 0;

    public Currency CalculateCost(int currentLevel)
    {
        if (currentLevel == 0)
        {
            return new Currency(BaseCost);
        }

        double linearCost = BaseCost + (currentLevel * CostLinearIncrement);
        double expCost = BaseCost * Math.Pow(CostMultiplier, currentLevel * 0.5);
        double finalCost = Math.Max(linearCost, expCost);

        return new Currency(finalCost);
    }

    public int CalculateDamage(int currentLevel)
    {
        if (currentLevel <= 0)
        {
            return BaseDamage;
        }

        int linearDamage = BaseDamage + (currentLevel * DamageMultiplier);
        int bonusDamage = (currentLevel * currentLevel) * DamageBonus;
        return linearDamage + bonusDamage;
    }
}
