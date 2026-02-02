using UnityEngine;
using System;

public class Upgrade
{
    // 기획 데이터
    // 1. 기획 테이블의 데이터를 가져온다.
    public readonly UpgradeSpecData SpecData;

    // 게임 중 변경되는 데이터
    public int Level { get; private set; }
    public Currency Cost => SpecData.BaseCost + Math.Pow(SpecData.CostMultiplier, Level);   // 지수 공식 : 기본 비용 + 증가량 ^ 레벨 
    public double Damage => SpecData.BaseDamage + Level + SpecData.DamageMultiplier;          // 선형 공식 : 기본 비용 + 레벨 * 증가량 
    public bool IsMaxLevel => Level >= SpecData.MaxLevel;

    // Constructor (유효성 검사)
    public Upgrade(UpgradeSpecData specData, int level = 0)
    {
        SpecData = specData;
        Level = level;

        // 유효성 검사
        if (specData.MaxLevel < 0)
            throw new ArgumentException($"최대 레벨은 0보다 커야 합니다: {specData.MaxLevel}");
        if (specData.BaseCost <= 0) throw new System.ArgumentException($"기본 비용은 0보다 커야 합니다: {specData.BaseCost}");
/*        if (specData.BaseDamage <= 0) throw new System.ArgumentException($"기본 대미지는 0보다 커야 합니다: {specData.BaseDamage}");*/
        if (specData.CostMultiplier <= 0) throw new System.ArgumentException($"비용 증가량은 0보다 커야 합니다: {specData.CostMultiplier}");
/*        if (specData.DamageMultiplier <= 0) throw new System.ArgumentException($"대미지 증가량은 0보다 커야 합니다: {specData.DamageMultiplier}");*/
        if (string.IsNullOrEmpty(specData.Name)) throw new System.ArgumentException("이름은 비어있을 수 없습니다");
        if (string.IsNullOrEmpty(specData.Description)) throw new System.ArgumentException("설명은 비어있을 수 없습니다");

    }

    public bool CanLevelUp()
    {
        return !IsMaxLevel;
    }

    // 레벨업 시도
    public bool TryLevelUp()
    {
        if (!CanLevelUp()) return false;

        Level++;

        return true;
    }

}