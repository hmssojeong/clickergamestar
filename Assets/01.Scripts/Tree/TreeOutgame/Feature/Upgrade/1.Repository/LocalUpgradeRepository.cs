using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 로컬 파일 시스템을 사용한 업그레이드 데이터 저장소
/// JSON 파일로 저장/로드
/// </summary>
public class LocalUpgradeRepository : IUpgradeRepository
{
    private readonly string _filePath;
    private readonly string _userId;

    public LocalUpgradeRepository(string userId)
    {
        _userId = userId;
        _filePath = Path.Combine(Application.persistentDataPath, $"{userId}_upgrade_save.json");
        
        Debug.Log($"[LocalUpgradeRepository] 초기화 - 저장 경로: {_filePath}");
    }

    public void Save(UpgradeSaveData data)
    {
        try
        {
            data.LastSaveTime = DateTime.Now.ToString("o");
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(_filePath, json);
            
            Debug.Log($"[LocalUpgradeRepository] 저장 완료 - 파일: {_filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalUpgradeRepository] 저장 실패: {e.Message}");
            throw;
        }
    }

    public UpgradeSaveData Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                Debug.LogWarning($"[LocalUpgradeRepository] 저장 파일 없음 - 기본값 반환: {_filePath}");
                return UpgradeSaveData.Default;
            }

            string json = File.ReadAllText(_filePath);
            var data = JsonConvert.DeserializeObject<UpgradeSaveData>(json);
            
            Debug.Log($"[LocalUpgradeRepository] 로드 완료 - 파일: {_filePath}");
            return data ?? UpgradeSaveData.Default;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalUpgradeRepository] 로드 실패: {e.Message}");
            return UpgradeSaveData.Default;
        }
    }
}
