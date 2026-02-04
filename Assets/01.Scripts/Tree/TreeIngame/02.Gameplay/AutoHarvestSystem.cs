
public class AutoHarvestSystem
{
    private int _squirrelCount;
    private readonly double _squirrelApplePerSecond;

    public int SquirrelCount => _squirrelCount;
    public double ApplePerSecond => _squirrelCount * _squirrelApplePerSecond;

    public AutoHarvestSystem(double squirrelApplePerSecond = 50.0)
    {
        _squirrelCount = 0;
        _squirrelApplePerSecond = squirrelApplePerSecond;
    }

    public void SetSquirrelCount(int count)
    {
        if (count < 0)
        {
            throw new System.ArgumentException("다람쥐 수는 0 이상이어야 합니다.");
        }
        _squirrelCount = count;
    }

    public double CalculateAutoApples()
    {
        return ApplePerSecond;
    }
}