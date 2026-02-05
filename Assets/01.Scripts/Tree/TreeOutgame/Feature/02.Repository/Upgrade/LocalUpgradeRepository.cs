using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;

/// <summary>
/// 로컬 파일 시스템을 사용한 업그레이드 데이터 저장소
/// JSON 파일로 저장/로드
/// </summary>
public class LocalUpgradeRepository : IUpgradeRepository
{
    private const string SaveKey = "UpgradeData";

    public async UniTaskVoid Save(UpgradeSaveData data)
    {
        try
        {
            data.LastSaveTime = DateTime.Now.ToString("o");
            string json = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();

            await UniTask.Yield();

            Debug.Log("[LocalUpgradeRepository] PlayerPrefs에 업그레이드 저장 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalUpgradeRepository] 저장 실패: {e.Message}");
            throw;
        }
    }

    public async UniTask<UpgradeSaveData> Load()
    {
        try
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                Debug.LogWarning("[LocalUpgradeRepository] 저장된 데이터 없음 - 기본값 반환");
                return UpgradeSaveData.Default;
            }

            string json = PlayerPrefs.GetString(SaveKey);
            var data = JsonConvert.DeserializeObject<UpgradeSaveData>(json);

            await UniTask.Yield();

            Debug.Log("[LocalUpgradeRepository] PlayerPrefs에서 업그레이드 로드 완료");
            return data ?? UpgradeSaveData.Default;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalUpgradeRepository] 로드 실패: {e.Message}");
            return UpgradeSaveData.Default;
        }
    }
}