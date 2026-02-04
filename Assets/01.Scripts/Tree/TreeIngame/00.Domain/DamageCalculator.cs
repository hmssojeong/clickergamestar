using UnityEngine;

public static class DamageCalculator
{
    public static double CalculateBaseDamage(
        EClickType clickType,
        double manualDamage,
        double autoDamage)
    {
        return clickType == EClickType.Manual ? manualDamage : autoDamage;
    }

    public static (bool isCritical, double damage) CalculateCriticalDamage(
        double baseDamage,
        double criticalChance,
        double criticalMultiplier)
    {
        bool isCritical = Random.value < criticalChance;
        double damage = isCritical ? baseDamage * criticalMultiplier : baseDamage;

        return (isCritical, damage);
    }

    public static double ApplyFeverMultiplier(double damage, bool isFeverActive, double feverMultiplier)
    {
        return isFeverActive ? damage * feverMultiplier : damage;
    }

    public static (double finalDamage, bool isCritical) CalculateFinalDamage(
        EClickType clickType,
        double manualDamage,
        double autoDamage,
        double criticalChance,
        double criticalMultiplier,
        bool isFeverActive,
        double feverMultiplier)
    {
        // 1. 기본 데미지
        double baseDamage = CalculateBaseDamage(clickType, manualDamage, autoDamage);

        // 2. 크리티컬 적용
        var (isCritical, damageAfterCritical) = CalculateCriticalDamage(
            baseDamage, criticalChance, criticalMultiplier);

        // 3. 피버 배율 적용
        double finalDamage = ApplyFeverMultiplier(
            damageAfterCritical, isFeverActive, feverMultiplier);

        return (finalDamage, isCritical);
    }
}