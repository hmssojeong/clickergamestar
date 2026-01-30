using System;

/// <summary>
/// GameManager의 게임 진행 상태를 저장하는 데이터 클래스
/// </summary>
[Serializable]
public class GameStateSaveData
{
    // 게임 진행 통계
    public double TotalApplesCollected = 0d;

    // 크리티컬 시스템
    public double CriticalChance = 0.1d;
    public double CriticalMultiplier = 2.0d;

    // 다람쥐 시스템
    public int SquirrelCount = 0;
    public double SquirrelApplePerSecond = 50d;

    // 피버 시스템
    public int FeverThreshold = 75;
    public double FeverMultiplier = 2.5d;
    public float FeverDuration = 10f;

    public static GameStateSaveData Default => new GameStateSaveData();
}
