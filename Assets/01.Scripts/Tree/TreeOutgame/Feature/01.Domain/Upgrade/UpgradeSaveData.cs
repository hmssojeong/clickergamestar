using System;
using System.Collections.Generic;

// Dictionary 기반으로 유연한 저장/로드
[Serializable]
public class UpgradeSaveData
{
    // 업그레이드 레벨 (Dictionary를 직렬화하기 위해 직접 구현)
    public Dictionary<EUpgradeType, int> UpgradeLevels = new Dictionary<EUpgradeType, int>();
    
    public string LastSaveTime;

    /// <summary>기본값 (모든 레벨 0)</summary>
    public static UpgradeSaveData Default
    {
        get
        {
            var data = new UpgradeSaveData
            {
                UpgradeLevels = new Dictionary<EUpgradeType, int>(),
                LastSaveTime = DateTime.Now.ToString("o")
            };

            // 모든 업그레이드 타입을 0으로 초기화
            for (int i = 0; i < (int)EUpgradeType.Count; i++)
            {
                data.UpgradeLevels[(EUpgradeType)i] = 0;
            }

            return data;
        }
    }
}
