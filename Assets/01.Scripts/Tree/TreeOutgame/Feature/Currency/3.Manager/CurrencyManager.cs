using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// 게임 내 재화를 관리하는 매니저
// - Repository 패턴을 사용하여 데이터 저장/로드
// - SaveLoadManager를 통해 저장되므로 직접 저장하지 않음
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Serializable]
    public class CurrencyChangedEvent : UnityEvent<ECurrencyType, Currency> { }
    public CurrencyChangedEvent OnCurrencyChanged = new CurrencyChangedEvent();

    public static event Action OnDataChanged;

    private Dictionary<ECurrencyType, Currency> _currencies;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        _currencies = new Dictionary<ECurrencyType, Currency>();
        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            _currencies[(ECurrencyType)i] = new Currency(0);
        }
    }

    // 특정 재화를 구매할 수 있는지 확인
    public bool CanAfford(ECurrencyType type, Currency cost)
    {
        return Get(type) >= cost;
    }


    // 재화를 소비 시도
    public bool TrySpend(ECurrencyType type, Currency cost)
    {
        if (!CanAfford(type, cost))
        {
            Debug.LogWarning($"{type} 부족: 필요 {cost}, 보유 {Get(type)}");
            return false;
        }

        try
        {
            _currencies[type] -= cost;
            NotifyCurrencyChanged(type);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"{type} 소비 실패: {e.Message}");
            return false;
        }
    }

    // 재화 추가
    public void Add(ECurrencyType type, Currency amount)
    {
        _currencies[type] += amount;
        NotifyCurrencyChanged(type);
    }

    // 재화 조회
    public Currency Get(ECurrencyType type)
    {
        return _currencies.TryGetValue(type, out var currency) ? currency : new Currency(0);
    }

    // 재화 직접 설정 (로드 시 사용)

    public void Set(ECurrencyType type, Currency amount)
    {
        _currencies[type] = amount;
        NotifyCurrencyChanged(type);
    }

    // SaveLoadManager로부터 데이터를 로드
    public void LoadFromData(Dictionary<ECurrencyType, double> data)
    {
        foreach (var pair in data)
        {
            _currencies[pair.Key] = new Currency(Math.Max(0, pair.Value));
        }

        // 모든 재화 변경 알림
        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            ECurrencyType type = (ECurrencyType)i;
            OnCurrencyChanged?.Invoke(type, _currencies[type]);
        }

        OnDataChanged?.Invoke();
        Debug.Log("재화 데이터 로드 완료");
    }

    private void NotifyCurrencyChanged(ECurrencyType type)
    {
        OnCurrencyChanged?.Invoke(type, _currencies[type]);
        OnDataChanged?.Invoke();

        // SaveLoadManager를 통해 자동 저장 (디바운스 적용)
        if (SaveLoadManager.Instance != null)
        {
            // 디바운스를 위해 약간의 지연 후 저장
            CancelInvoke(nameof(RequestSave));
            Invoke(nameof(RequestSave), 0.5f);
        }
    }

    private void RequestSave()
    {
        SaveLoadManager.Instance?.SaveGame();
    }
}
