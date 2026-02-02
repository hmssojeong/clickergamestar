using UnityEngine;
using System;

public class Upgrade : IReadonlyUpgrade
{
    private readonly UpgradeSpecData _specData;

    public int Level { get; private set; }

    public EUpgradeType Type => _specData.Type;
    public string Name => _specData.Name;
    public string Description => _specData.Description;
    public Currency Cost => _specData.CalculateCost(Level);
    public double Damage => _specData.CalculateDamage(Level);
    public bool IsMaxLevel => Level >= _specData.MaxLevel;
    public Sprite Icon => _specData.Icon;

    internal UpgradeSpecData SpecData => _specData;

    public Upgrade(UpgradeSpecData specData, int level = 0)
    {
        if (specData == null)
            throw new ArgumentNullException(nameof(specData));
        
        if (level < 0)
            throw new ArgumentException($"Level cannot be negative: {level}");

        _specData = specData;
        Level = Mathf.Max(0, level);

        ValidateSpecData(specData);
    }

    private void ValidateSpecData(UpgradeSpecData specData)
    {
        if (specData.MaxLevel < 0)
            throw new ArgumentException($"MaxLevel cannot be negative: {specData.MaxLevel}");
        
        if (specData.BaseCost <= 0)
            throw new ArgumentException($"BaseCost must be positive: {specData.BaseCost}");
        
        if (specData.CostMultiplier <= 0)
            throw new ArgumentException($"CostMultiplier must be positive: {specData.CostMultiplier}");
        
        if (string.IsNullOrEmpty(specData.Name))
            throw new ArgumentException("Name cannot be empty");
        
        if (string.IsNullOrEmpty(specData.Description))
            throw new ArgumentException("Description cannot be empty");
    }

    public bool CanLevelUp()
    {
        return !IsMaxLevel;
    }

    public bool TryLevelUp()
    {
        if (!CanLevelUp())
        {
            Debug.LogWarning($"[Upgrade] {Name} cannot level up - max level reached");
            return false;
        }

        Level++;
        Debug.Log($"[Upgrade] {Name} level up! Lv.{Level} (Damage: {Damage})");
        return true;
    }

    public override string ToString()
    {
        return $"{Name} Lv.{Level}/{_specData.MaxLevel} (Damage: {Damage}, Cost: {Cost})";
    }
}