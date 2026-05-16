public interface IReadonlyUpgrade
{
    EUpgradeType Type { get; }
    string Name { get; }
    string Description { get; }
    int Level { get; }
    Currency Cost { get; }
    int Damage { get; }
    bool IsMaxLevel { get; }
    UnityEngine.Sprite Icon { get; }
}
