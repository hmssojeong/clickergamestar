using System;

[Serializable]
public class GameStateSaveData
{
    public double TotalApplesCollected = 0d;

    public double CriticalChance = 0.1d;
    public double CriticalMultiplier = 2.0d;

    public int SquirrelCount = 0;
    // Legacy field from the old passive auto-harvest design.
    public double SquirrelApplePerSecond = 50d;

    public int FeverThreshold = 75;
    public double FeverMultiplier = 3d;
    public float FeverDuration = 10f;

    public static GameStateSaveData Default => new GameStateSaveData();
}
