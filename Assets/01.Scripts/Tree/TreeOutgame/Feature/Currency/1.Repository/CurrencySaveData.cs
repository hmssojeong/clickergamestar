using UnityEngine;
using System;
using System.Collections.Generic;
public class CurrencySaveData
{
    // 재화
    public Dictionary<ECurrencyType, double> Currencies = new();

    // 타입별 현재 레벨 저장
    public Dictionary<EUpgradeType, int> UpgradeLevels = new();

    public static CurrencySaveData Default => new CurrencySaveData();
}
