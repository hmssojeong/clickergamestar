using UnityEngine;
using System;

/// <summary>
/// 업그레이드 타입
/// </summary>
public enum UpgradeType
{
    AppleHarvest,      // 사과 수확 강화 (ManualDamage 증가)
    SquirrelHire,      // 다람쥐 고용 (자동 수확)
    GoldenAppleLuck,   // 황금 사과 행운 (크리티컬 확률)
    FeverMaster,       // 피버 타임 마스터
    SuperCritical      // 슈퍼 크리티컬 (크리티컬 배수)
}

/// <summary>
/// 업그레이드 데이터 클래스 (double 타입 사용)
/// </summary>
[Serializable]
public class UpgradeData
{
    public UpgradeType type;
    public string upgradeName;
    public string description;
    public Sprite icon;
    
    public int currentLevel = 0;
    public int maxLevel = 10;
    
    // 비용 계산 (double 타입)
    public double baseCost;
    public float costMultiplier = 1.5f;
    
    public double GetCurrentCost()
    {
        if (currentLevel >= maxLevel) return 0;
        return Math.Round(baseCost * Math.Pow(costMultiplier, currentLevel));
    }
    
    public bool CanUpgrade(double currentApples)
    {
        return currentLevel < maxLevel && currentApples >= GetCurrentCost();
    }
    
    public bool IsMaxLevel()
    {
        return currentLevel >= maxLevel;
    }
}
