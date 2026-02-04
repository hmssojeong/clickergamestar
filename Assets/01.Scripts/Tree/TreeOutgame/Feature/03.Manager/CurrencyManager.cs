using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Serializable]
    public class CurrencyChangedEvent : UnityEvent<ECurrencyType, Currency> { }
    public CurrencyChangedEvent OnCurrencyChanged = new CurrencyChangedEvent();

    public static event Action OnDataChanged;

    [Header("Repository Settings")]
    [SerializeField] private bool _useFirebase = false;

    private ICurrencyRepository _repository;
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

        _repository = new FirebaseCurrencyRepository();
    }

    private void Initialize()
    {
        _currencies = new Dictionary<ECurrencyType, Currency>();
        for (int i = 0; i < (int)ECurrencyType.Count; i++)
        {
            _currencies[(ECurrencyType)i] = new Currency(0);
        }

        InitializeRepository();
    }

    private void InitializeRepository()
    {
        string userId = AccountManager.Instance?.Email ?? "guest";

        _repository = _useFirebase
                ? (ICurrencyRepository)new FirebaseCurrencyRepository()
                : new LocalCurrencyRepository();

        Debug.Log($"Repository 초기화");
    }

    public bool CanAfford(ECurrencyType type, Currency cost)
    {
        return Get(type) >= cost;
    }

    public bool TrySpend(ECurrencyType type, Currency cost)
    {
        if (!CanAfford(type, cost))
        {
            return false;
        }

        _currencies[type] -= cost;
        NotifyCurrencyChanged(type);
        return true;
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

    public void Set(ECurrencyType type, Currency amount)
    {
        _currencies[type] = amount;
        NotifyCurrencyChanged(type);
    }

    public async UniTask SaveToRepository()
    {
        var saveData = new CurrencySaveData();

        foreach (var pair in _currencies)
        {
            saveData.Currencies[pair.Key] = pair.Value.Value;
        }

        _repository.Save(saveData).Forget();
        await UniTask.Yield();

        Debug.Log("Repository에 저장 완료");
    }

    public async UniTask LoadFromRepository()
    {
        var saveData = await _repository.Load();
        LoadFromData(saveData.Currencies);

        Debug.Log("Repository에서 로드 완료");
    }

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
    }

    private void NotifyCurrencyChanged(ECurrencyType type)
    {
        OnCurrencyChanged?.Invoke(type, _currencies[type]);
        OnDataChanged?.Invoke();
    }
}