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
    public double CostLinearIncrement = 5; // 초반 구간의 일정한 비용 상승 담당
    public double CostMultiplier = 1.15;

    [Header("효과 설정")]
    public double BaseDamage = 1;
    public double DamageMultiplier = 9;
    public double DamageBonus = 0.5;

    // 특정 레벨의 비용 계산
    public Currency CalculateCost(int currentLevel)
    {
        /*        // 비용 공식: BaseCost * (CostMultiplier ^ currentLevel)
                double cost = BaseCost * Math.Pow(CostMultiplier, currentLevel);
                return new Currency(cost);*/

        if (currentLevel == 0)
        {
            return new Currency(BaseCost);
        }
        
        // 선형 계산: 초기 비용 + (레벨 * 증가량)
        double linearCost = BaseCost + (currentLevel * CostLinearIncrement);

        // 지수 계산: 초기 비용 * (배수 ^ (레벨 * 0.5)) -> 성장을 조금 더 완만하게 제어
        double expCost = BaseCost * Math.Pow(CostMultiplier, currentLevel * 0.5);

        // 두 값 중 더 높은 값을 선택하여 비용 인플레이션 방어
        double finalCost = Math.Max(linearCost, expCost);

        return new Currency(finalCost);
    }

    // 특정 레벨의 데미지 계산 (선형 + 제곱 보너스)
    public double CalculateDamage(int currentLevel)
    {
        /*        // 데미지 공식: BaseDamage + (currentLevel * DamageMultiplier)
                return BaseDamage + (currentLevel * DamageMultiplier);*/
        if (currentLevel == 0) return BaseDamage;

        // 기본 선형 데미지 계산
        double linearDamage = BaseDamage + (currentLevel * DamageMultiplier);

        // 레벨의 제곱에 비례하는 보너스 데미지 (후반 성장 동력)
        double bonusDamage = (currentLevel * currentLevel) * DamageBonus;

        return linearDamage + bonusDamage;
    }
}

