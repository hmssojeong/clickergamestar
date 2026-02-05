using System;
using System.Collections.Generic;
using Firebase.Firestore;

// Dictionary 기반으로 유연한 저장/로드
[Serializable]
[FirestoreData]
public class UpgradeSaveData
{
    [FirestoreProperty]
    public Dictionary<string, int> UpgradeLevels { get; set; } = new Dictionary<string, int>();

    [FirestoreProperty]
    public string LastSaveTime { get; set; }

    // Firebase 필수: 기본 생성자
    public UpgradeSaveData()
    {
        UpgradeLevels = new Dictionary<string, int>();
    }

    public static UpgradeSaveData Default
    {
        get
        {
            var data = new UpgradeSaveData();
            data.LastSaveTime = DateTime.Now.ToString("o");

            // string 키로 초기화
            for (int i = 0; i < (int)EUpgradeType.Count; i++)
            {
                EUpgradeType type = (EUpgradeType)i;
                data.UpgradeLevels[type.ToString()] = 0;
            }

            return data;
        }
    }
}
