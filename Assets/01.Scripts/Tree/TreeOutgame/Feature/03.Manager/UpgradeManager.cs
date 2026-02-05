using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    public static event Action OnDataChanged;

    [Header("설정")]
    [SerializeField] private UpgradeSpecTableSO _specTable;
    [SerializeField] private bool _useFirebase = false;

    private IUpgradeRepository _repository;
    private Dictionary<EUpgradeType, Upgrade> _upgrades = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeRepository();
            InitializeUpgrades();
        }
        else
        {
            Destroy(gameObject);
        }
    }

private void InitializeRepository()
    {
        string userId = AccountManager.Instance?.Email ?? "guest";

        _repository = _useFirebase
            ? (IUpgradeRepository)new FirebaseUpgradeRepository()
            : new LocalUpgradeRepository();

        Debug.Log($"Repository 초기화 완료 - Firebase: {_useFirebase}");
    }

    public void InitializeUpgrades(Dictionary<EUpgradeType, int> savedLevels = null)
    {
        _upgrades.Clear();

        foreach (var specData in _specTable.Datas)
        {
            int level = (savedLevels != null && savedLevels.ContainsKey(specData.Type))
                        ? savedLevels[specData.Type] : 0;

            _upgrades.Add(specData.Type, new Upgrade(specData, level));
        }

        ApplyAllUpgradeEffects();
        OnDataChanged?.Invoke();
    }

    public async UniTask SaveUpgrades()
    {
        var saveData = new UpgradeSaveData();

        foreach (var pair in _upgrades)
        {
            saveData.UpgradeLevels[pair.Key.ToString()] = pair.Value.Level;
        }

        _repository.Save(saveData).Forget();
        await UniTask.Yield();

        Debug.Log("Upgrades 저장 완료");
    }

    public async UniTask LoadUpgrades()
    {
        var saveData = await _repository.Load();

        // string을 enum으로 변환
        var loadedLevels = new Dictionary<EUpgradeType, int>();

        foreach (var pair in saveData.UpgradeLevels)
        {
            if (Enum.TryParse<EUpgradeType>(pair.Key, out EUpgradeType type))
            {
                loadedLevels[type] = pair.Value;
            }
        }

        InitializeUpgrades(loadedLevels);
        ApplyAllUpgradeEffects();

        Debug.Log("Upgrades 로드 완료");
    }

    public IReadonlyUpgrade Get(EUpgradeType type)
    {
        return _upgrades.TryGetValue(type, out var upgrade) ? upgrade : null;
    }

    public List<IReadonlyUpgrade> GetAll()
    {
        return _upgrades.Values.Cast<IReadonlyUpgrade>().ToList();
    }

    public bool CanLevelUp(EUpgradeType type)
    {
        if (!_upgrades.TryGetValue(type, out Upgrade upgrade))
        {
            return false;
        }

        if (!upgrade.CanLevelUp())
        {
            return false;
        }

        if (CurrencyManager.Instance == null)
        {
            return false;
        }

        return CurrencyManager.Instance.CanAfford(ECurrencyType.Apple, upgrade.Cost);
    }

    public bool TryLevelUp(EUpgradeType type)
    {
        if (!_upgrades.TryGetValue(type, out Upgrade upgrade))
        {
            Debug.LogWarning($"Upgrade type {type} not found");
            return false;
        }

        if (!CurrencyManager.Instance.TrySpend(ECurrencyType.Apple, upgrade.Cost))
        {
            Debug.LogWarning($"재화 부족 - 필요: {upgrade.Cost}");
            return false;
        }

        if (!upgrade.TryLevelUp())
        {
            CurrencyManager.Instance.Add(ECurrencyType.Apple, upgrade.Cost);
            Debug.LogError($"레벨업 실패 - 재화 환불");
            return false;
        }

        ApplyUpgradeEffect(type, upgrade);
        OnDataChanged?.Invoke();

        Debug.Log($"{upgrade.Name} 레벨업! (Lv.{upgrade.Level})");
        return true;
    }

    private void ApplyAllUpgradeEffects()
    {
        foreach (var upgrade in _upgrades)
        {
            ApplyUpgradeEffect(upgrade.Key, upgrade.Value);
        }
    }

    private void ApplyUpgradeEffect(EUpgradeType type, Upgrade upgrade)
    {
        if (GameplayManager.Instance == null)
        {
            Debug.LogWarning("GameplayManager가 없어 효과를 적용할 수 없습니다.");
            return;
        }

        switch (type)
        {
            case EUpgradeType.AppleHarvest:
                GameplayManager.Instance.SetManualDamage(upgrade.Damage);
                break;

            case EUpgradeType.SquirrelHire:
                GameplayManager.Instance.SetSquirrelCount(upgrade.Level);
                GameplayManager.Instance.SetAutoDamage(upgrade.Damage);
                break;

            case EUpgradeType.GoldenAppleLuck:
                GameplayManager.Instance.SetCriticalChance(upgrade.Damage / 100.0);
                break;

            case EUpgradeType.FeverMaster:
                // FeverSystem은 초기화 시점에 설정
                break;

            case EUpgradeType.SuperCritical:
                GameplayManager.Instance.SetCriticalMultiplier(upgrade.Damage);
                break;

            default:
                Debug.LogWarning($"알 수 없는 업그레이드 타입: {type}");
                break;
        }
    }

    public double GetUpgradeValue(EUpgradeType type)
    {
        var upgrade = Get(type);
        return upgrade != null ? upgrade.Damage : 0;
    }
}