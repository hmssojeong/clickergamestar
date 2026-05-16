using UnityEngine;

public struct ClickInfo
{
    public EClickType Type;
    public double Damage;
    public Vector2 Position;

    public ClickInfo(Vector2 position, EClickType clickType) : this()
    {
        Position = position;
        Type = clickType;
    }
}
