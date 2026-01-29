using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Serializable]
    public class CurrencyChangedEvent : UnityEvent<ECurrencyType, Currency> { }
    public CurrencyChangedEvent OnCurrencyChanged = new CurrencyChangedEvent();

    public static event Action OnDataChanged;

    [Header("자동 저장 설정")]
    [SerializeField] private bool _autoSave = true;
    [SerializeField] private float _autoSaveInterval = 30f;
    [SerializeField] private float _saveDebounceTime = 0.6f;

    private Dictionary<ECurrencyType, Currency> _currencies;
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

        if (_needsSave)
        {
            UpdateSaveDebounce();
        }
    }

    private void Initialize()
    {
        _currencies = new Dictionary<ECurrencyType, Currency>();
        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            _currencies[(ECurrencyType)i] = new Currency(0);
        }
        Load();
    }

    public bool CanAfford(ECurrencyType type, Currency cost)
    {
        return Get(type) >= cost;
    }

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

    public void Add(ECurrencyType type, Currency amount)
    {
        _currencies[type] += amount;
        NotifyCurrencyChanged(type);
    }

    public Currency Get(ECurrencyType type)
    {
        return _currencies.TryGetValue(type, out var currency) ? currency : new Currency(0);
    }

    private void NotifyCurrencyChanged(ECurrencyType type)
    {
        OnCurrencyChanged?.Invoke(type, _currencies[type]);
        OnDataChanged?.Invoke();

        _needsSave = true;
        _saveDebounceTimer = _saveDebounceTime;
    }

    private void UpdateAutoSave()
    {
        _autoSaveTimer += Time.deltaTime;
        if (_autoSaveTimer >= _autoSaveInterval)
        {
            _autoSaveTimer = 0f;
            if (_needsSave) Save();
        }
    }

    private void UpdateSaveDebounce()
    {
        _saveDebounceTimer -= Time.deltaTime;
        if (_saveDebounceTimer <= 0f)
        {
            Save();
        }
    }

    public void Save()
    {
        try
        {
            foreach (var pair in _currencies)
            {
                // Currency 클래스의 Value(double)를 문자열로 저장
                PlayerPrefs.SetString($"Currency_{pair.Key}", pair.Value.Value.ToString());
            }
            PlayerPrefs.Save();
            _needsSave = false;
            Debug.Log("모든 화폐 데이터 저장 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"화폐 저장 실패: {e.Message}");
        }
    }

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
                        _currencies[type] = new Currency(Math.Max(0, value));
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
