using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    // 화폐 변경 이벤트 (타입별)
    [Serializable]
    public class CurrencyChangedEvent : UnityEvent<ECurrencyType, Currency> { }
    public CurrencyChangedEvent OnCurrencyChanged = new CurrencyChangedEvent();

    public static event Action OnDataChanged;

    [Header("자동 저장 설정")]
    [SerializeField] private bool _autoSave = true;
    [SerializeField] private float _autoSaveInterval = 30f; // 30초마다 자동 저장
    [SerializeField] private float _saveDebounceTime = 0.6f; // 마지막 변경 후 0.6초 대기


    // 화폐 데이터 (Dictionary로 관리)
    private Dictionary<ECurrencyType, Currency> _currencies;

    // 자동 저장 타이머
    private float _autoSaveTimer;
    private float _saveDebounceTimer;
    private bool _needsSave;
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

    private void Update()
    {
        if (_autoSave)
        {
            UpdateAutoSave();
        }
    }

    private void OnApplicationQuit()
    {
        // 종료 시 저장
        if (_needsSave)
        {
            Save();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        // 앱 백그라운드 진입 시 저장
        if (pause && _needsSave)
        {
            Save();
        }
    }

    private void Initialize()
    {
        _currencies = new Dictionary<ECurrencyType, Currency>();

        // 모든 화폐 타입 초기화
        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            ECurrencyType type = (ECurrencyType)i;
            _currencies[type] = new Currency(0);
        }

        // 저장된 데이터 로드
        Load();

        Debug.Log("CurrencyManager 초기화 완료");
    }

    public Currency Get(ECurrencyType type)
    {
        if (!_currencies.ContainsKey(type))
        {
            Debug.LogWarning($"존재하지 않는 화폐 타입: {type}");
            return new Currency(0);
        }

        return _currencies[type];
    }

    // 화폐 조회
    public double GetValue(ECurrencyType type)
    {
        return Get(type).Value;
    }

    // 화폐 추가
    public void Add(ECurrencyType type, Currency amount)
    {
        if (!_currencies.ContainsKey(type))
        {
            Debug.LogError($"존재하지 않는 화폐 타입: {type}");
            return;
        }

        try
        {
            _currencies[type] = _currencies[type] + amount;
            OnCurrencyUpdated(type);

            Debug.Log($"{type} 추가: +{amount} (현재: {_currencies[type]})");
        }
        catch (Exception e)
        {
            Debug.LogError($"화폐 추가 실패: {e.Message}");
        }
    }

    // 화폐 추가
    public void Add(ECurrencyType type, double amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"음수 추가 시도: {amount}. 대신 Spend를 사용하세요.");
            return;
        }

        Add(type, new Currency(amount));
    }

    // 화폐 소비
    public bool Spend(ECurrencyType type, Currency amount)
    {
        if (!_currencies.ContainsKey(type))
        {
            Debug.LogError($"존재하지 않는 화폐 타입: {type}");
            return false;
        }

        // 충분한지 확인
        if (_currencies[type] < amount)
        {
            Debug.LogWarning($"{type} 부족: 필요 {amount}, 보유 {_currencies[type]}");
            return false;
        }

        try
        {
            _currencies[type] = _currencies[type] - amount;
            OnCurrencyUpdated(type);

            Debug.Log($"{type} 소비: -{amount} (현재: {_currencies[type]})");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"화폐 소비 실패: {e.Message}");
            return false;
        }
    }

    // 화폐 소비 
    public bool Spend(ECurrencyType type, double amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"음수 소비 시도: {amount}");
            return false;
        }

        return Spend(type, new Currency(amount));
    }

    /// 화폐 설정
    public void Set(ECurrencyType type, Currency amount)
    {
        if (!_currencies.ContainsKey(type))
        {
            Debug.LogError($"존재하지 않는 화폐 타입: {type}");
            return;
        }

        try
        {
            _currencies[type] = amount;
            OnCurrencyUpdated(type);

            Debug.Log($"{type} 설정: {amount}");
        }
        catch (Exception e)
        {
            Debug.LogError($"화폐 설정 실패: {e.Message}");
        }
    }

    // 화폐 설정
    public void Set(ECurrencyType type, double amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"음수 설정 시도: {amount}. 0으로 설정합니다.");
            amount = 0;
        }

        Set(type, new Currency(amount));
    }

    public bool Has(ECurrencyType type, Currency amount)
    {
        return Get(type) >= amount;
    }

    public bool Has(ECurrencyType type, double amount)
    {
        if (amount < 0) return false;
        return Has(type, new Currency(amount));
    }

    private void OnCurrencyUpdated(ECurrencyType type)
    {
        // 이벤트 발행
        OnCurrencyChanged?.Invoke(type, _currencies[type]);
        OnDataChanged?.Invoke();

        // 저장 필요 플래그
        _needsSave = true;
        _saveDebounceTimer = _saveDebounceTime;
    }

    // 자동 저장 업데이트
    private void UpdateAutoSave()
    {
        if (_saveDebounceTimer > 0)
        {
            _saveDebounceTimer -= Time.deltaTime;

            if (_saveDebounceTimer <= 0 && _needsSave)
            {
                Save();
            }
        }

        // 주기적 자동 저장
        _autoSaveTimer += Time.deltaTime;
        if (_autoSaveTimer >= _autoSaveInterval && _needsSave)
        {
            Save();
            _autoSaveTimer = 0;
        }
    }

    // 저장
    public void Save()
    {
        try
        {
            foreach (var kvp in _currencies)
            {
                ECurrencyType type = kvp.Key;
                Currency currency = kvp.Value;

                string key = $"Currency_{type}";
                // double을 고정밀도(G17)로 문자열 변환하여 저장
                PlayerPrefs.SetString(key, currency.Value.ToString("G17"));
            }

            PlayerPrefs.Save();
            _needsSave = false;

            Debug.Log("화폐 데이터 저장 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"화폐 저장 실패: {e.Message}");
        }
    }

    // 로드
    public void Load()
    {
        try
        {
            for (int i = 0; i < (int)ECurrencyType.Count; i++)
            {
                ECurrencyType type = (ECurrencyType)i;
                string key = $"Currency_{type}";

                if (PlayerPrefs.HasKey(key))
                {
                    string valueStr = PlayerPrefs.GetString(key, "0");

                    if (double.TryParse(valueStr, out double value))
                    {
                        if (value >= 0)
                        {
                            _currencies[type] = new Currency(value);
                        }
                        else
                        {
                            Debug.LogWarning($"{type} 음수 데이터 발견: {value}. 0으로 초기화합니다.");
                            _currencies[type] = new Currency(0);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"{type} 파싱 실패: {valueStr}. 0으로 초기화합니다.");
                        _currencies[type] = new Currency(0);
                    }
                }
            }

            _needsSave = false;

            Debug.Log("화폐 데이터 로드 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"화폐 로드 실패: {e.Message}");
        }
    }
}