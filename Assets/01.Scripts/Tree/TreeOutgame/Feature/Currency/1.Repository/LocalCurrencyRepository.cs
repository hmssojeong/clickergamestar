using Newtonsoft.Json;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

// 로컬 PlayerPrefs를 사용한 재화 데이터 저장소
// 데이터의 영속성(저장/불러오기)에 대한 책임은 '레포지토리'가 담당
// 비즈니스 로직과 분리

// Repository 패턴의 장점:
// - 1) 코드가 깔끔해지고 유지보수가 쉬워진다
// - 2) 저장소 구현을 쉽게 교체할 수 있다 (Local ↔ Firebase)
// - 3) 테스트가 용이해진다 (Mock Repository 사용 가능)
public class LocalCurrencyRepository : ICurrencyRepository
{
    private const string SaveKey = "GameUserData";

    public async UniTaskVoid Save(CurrencySaveData saveData)
    {
        try
        {
            saveData.LastSaveTime = DateTime.Now.ToString("o");
            string json = JsonConvert.SerializeObject(saveData);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
            
            // PlayerPrefs.Save()는 동기 메서드이지만,
            // 일관성을 위해 약간의 대기 시간 추가
            await UniTask.Yield();
            
            Debug.Log("[LocalCurrencyRepository] PlayerPrefs에 재화 저장 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalCurrencyRepository] 저장 실패: {e.Message}");
            throw;
        }
    }

    public async UniTask<CurrencySaveData> Load()
    {
        try
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                Debug.LogWarning("[LocalCurrencyRepository] 저장된 데이터 없음 - 기본값 반환");
                return CurrencySaveData.Default;
            }

            string json = PlayerPrefs.GetString(SaveKey);
            var data = JsonConvert.DeserializeObject<CurrencySaveData>(json);
            
            // PlayerPrefs.GetString()는 동기 메서드이지만,
            // 일관성을 위해 약간의 대기 시간 추가
            await UniTask.Yield();
            
            Debug.Log("[LocalCurrencyRepository] PlayerPrefs에서 재화 로드 완료");
            return data ?? CurrencySaveData.Default;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalCurrencyRepository] 로드 실패: {e.Message}");
            return CurrencySaveData.Default;
        }
    }
}