using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CurrencySaveData
{
    // 1. 재화 데이터
    public Dictionary<ECurrencyType, double> Currencies = new Dictionary<ECurrencyType, double>();

    // 2. 타입별 현재 레벨 저장
    public Dictionary<EUpgradeType, int> UpgradeLevels = new Dictionary<EUpgradeType, int>();

    // 3. 마지막 저장 시간 
    public string LastSaveTime;

    //기본값 (모든 재화 0) 
    public static CurrencySaveData Default
    {
        get
        {
            var data = new CurrencySaveData
            {
                // 변수명 유지: Currencies, LastSaveTime
                Currencies = new Dictionary<ECurrencyType, double>(),
                UpgradeLevels = new Dictionary<EUpgradeType, int>(), // UpgradeLevels 초기화 추가
                LastSaveTime = DateTime.Now.ToString("o")
            };

            // 모든 재화 타입을 0으로 초기화
            for (int i = 0; i < (int)ECurrencyType.Count; i++)
            {
                data.Currencies[(ECurrencyType)i] = 0;
            }

            return data;
        }
    }
}