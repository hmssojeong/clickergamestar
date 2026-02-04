public class CriticalSystem
{
    private double _criticalChance;
    private double _criticalMultiplier;

    public double CriticalChance => _criticalChance;
    public double CriticalMultiplier => _criticalMultiplier;

    public CriticalSystem(double criticalChance = 0.1, double criticalMultiplier = 2.0)
    {
        SetCriticalChance(criticalChance);
        SetCriticalMultiplier(criticalMultiplier);
    }

    public void SetCriticalChance(double chance)
    {
        if (chance < 0 || chance > 1)
        {
            throw new System.ArgumentException("크리티컬 확률은 0~1 사이여야 합니다.");
        }
        _criticalChance = chance;
    }

    public void SetCriticalMultiplier(double multiplier)
    {
        if (multiplier < 1)
        {
            throw new System.ArgumentException("크리티컬 배수는 1 이상이어야 합니다.");
        }
        _criticalMultiplier = multiplier;
    }
}