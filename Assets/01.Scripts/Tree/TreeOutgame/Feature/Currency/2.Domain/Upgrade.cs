using UnityEngine;
using System;

public class Upgrade
{
    public readonly EUpgradeType Type;
    public readonly string Name;
    public readonly string Description;
    public readonly int MaxLevel;
    public readonly double BaseCost;
    public readonly double CostMultiplier;
    public readonly Sprite Icon;

    public readonly UpgradeSpecData specData;
    // 게임 중 변경되는 데이터
    public int Level { get; private set; }

    //계산된 속성 (Property)
    public double CurrentCost => CalculateCost(Level);
    public bool IsMaxLevel => Level >= MaxLevel;
    public bool CanLevelUp => !IsMaxLevel;

    // Constructor (유효성 검사)
    public Upgrade(
        EUpgradeType type,
        string name,
        string description,
        int maxLevel,
        double baseCost,
        double costMultiplier,
        Sprite icon = null,
        int initialLevel = 0)
    {
        // 유효성 검사
        if (maxLevel < 0)
            throw new ArgumentException($"최대 레벨은 0보다 커야 합니다: {maxLevel}");
        if (baseCost <= 0)
            throw new ArgumentException($"기본 비용은 0보다 커야 합니다: {baseCost}");
        if (costMultiplier <= 1)
            throw new ArgumentException($"비용 증가율은 1보다 커야 합니다: {costMultiplier}");
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("이름은 비어있을 수 없습니다");
        if (initialLevel < 0 || initialLevel > maxLevel)
            throw new ArgumentException($"초기 레벨은 0~{maxLevel} 사이여야 합니다");

        // 필드 초기화
        Type = type;
        Name = name;
        Description = description;
        MaxLevel = maxLevel;
        BaseCost = baseCost;
        CostMultiplier = costMultiplier;
        Icon = icon;
        Level = initialLevel;
    }

    // 핵심 비즈니스 로직

    // 레벨업 시도
    public bool TryLevelUp()
    {
        if (!CanLevelUp)
        {
            Debug.LogWarning($"{Name}은(는) 이미 최대 레벨입니다.");
            return false;
        }

        Level++;
        Debug.Log($"{Name} 레벨 {Level} 달성!");
        return true;
    }

    // 특정 레벨의 비용 계산
    public double CalculateCost(int level)
    {
        if (level >= MaxLevel) return 0;
        return Math.Round(BaseCost * Math.Pow(CostMultiplier, level));
    }

    // 구매 가능 여부 확인
    public bool CanAfford(double currentCurrency)
    {
        return CanLevelUp && currentCurrency >= CurrentCost;
    }

    // 레벨 직접 설정 (저장 데이터 로드용)
    public void SetLevel(int level)
    {
        if (level < 0 || level > MaxLevel)
        {
            Debug.LogError($"잘못된 레벨: {level}. 범위는 0~{MaxLevel}입니다.");
            return;
        }
        Level = level;
    }

    // 디버깅용 정보 출력
    public override string ToString()
    {
        return $"[{Type}] {Name} Lv.{Level}/{MaxLevel} (Cost: {CurrentCost})";
    }
}