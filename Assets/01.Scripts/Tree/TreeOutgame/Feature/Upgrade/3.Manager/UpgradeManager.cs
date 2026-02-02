using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

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
/*
        _repository = _useFirebase
            ? (IUpgradeRepository)new FirebaseUpgradeRepository(userId)
            : new LocalUpgradeRepository(userId);*/

        Debug.Log($"[UpgradeManager] Repository initialized - Mode: {(_useFirebase ? "Firebase" : "Local")}, UserID: {userId}");
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
        
        Debug.Log($"[UpgradeManager] Upgrades initialized - Total: {_upgrades.Count}");
    }

    public void SaveUpgrades()
    {
        var saveData = new UpgradeSaveData();

        foreach (var upgrade in _upgrades)
        {
            saveData.UpgradeLevels[upgrade.Key] = upgrade.Value.Level;
        }

        _repository.Save(saveData);
        Debug.Log("[UpgradeManager] Upgrades saved");
    }

    public void LoadUpgrades()
    {
        var saveData = _repository.Load();
        InitializeUpgrades(saveData.UpgradeLevels);
        Debug.Log("[UpgradeManager] Upgrades loaded");
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
            Debug.LogWarning($"[UpgradeManager] CanLevelUp - Upgrade type {type} not found");
            return false;
        }

        if (!upgrade.CanLevelUp())
        {
            Debug.LogWarning($"[UpgradeManager] CanLevelUp - {upgrade.Name} max level reached");
            return false;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("[UpgradeManager] CanLevelUp - CurrencyManager.Instance is null!");
            return false;
        }

        bool canAfford = CurrencyManager.Instance.CanAfford(ECurrencyType.Apple, upgrade.Cost);
        Debug.Log($"[UpgradeManager] CanLevelUp - {upgrade.Name}: Have={CurrencyManager.Instance.Get(ECurrencyType.Apple).Value}, Need={upgrade.Cost.Value}, CanBuy={canAfford}");
        
        return canAfford;
    }

    public bool TryLevelUp(EUpgradeType type)
    {
        if (!_upgrades.TryGetValue(type, out Upgrade upgrade))
        {
            Debug.LogWarning($"[UpgradeManager] Upgrade type {type} not found");
            return false;
        }

        if (!CurrencyManager.Instance.TrySpend(ECurrencyType.Apple, upgrade.Cost))
        {
            Debug.LogWarning($"[UpgradeManager] Insufficient currency - Required: {upgrade.Cost}");
            return false;
        }

        if (!upgrade.TryLevelUp())
        {
            CurrencyManager.Instance.Add(ECurrencyType.Apple, upgrade.Cost);
            Debug.LogError($"[UpgradeManager] Level up failed - Currency refunded");
            return false;
        }

        ApplyUpgradeEffect(type, upgrade);

        OnDataChanged?.Invoke();

        Debug.Log($"✨ [UpgradeManager] {upgrade.Name} level up! (Lv.{upgrade.Level})");
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
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager not found - cannot apply upgrade effect");
            return;
        }

        switch (type)
        {
            case EUpgradeType.AppleHarvest:
                GameManager.Instance.ManualDamage = upgrade.Damage;
                Debug.Log($"Apple harvest power: {upgrade.Damage}");
                break;

            case EUpgradeType.SquirrelHire:
                GameManager.Instance.squirrelCount = upgrade.Level;
                GameManager.Instance.AutoDamage = upgrade.Damage;
                Debug.Log($"Squirrel count: {upgrade.Level}, Auto damage: {upgrade.Damage}");
                break;

            case EUpgradeType.GoldenAppleLuck:
                GameManager.Instance.criticalChance = upgrade.Damage / 100.0;
                Debug.Log($"Critical chance: {upgrade.Damage}%");
                break;

            case EUpgradeType.FeverMaster:
                GameManager.Instance.feverMultiplier = upgrade.Damage;
                Debug.Log($"Fever multiplier: x{upgrade.Damage}");
                break;

            case EUpgradeType.SuperCritical:
                GameManager.Instance.criticalMultiplier = upgrade.Damage;
                Debug.Log($"Critical multiplier: x{upgrade.Damage}");
                break;

            default:
                Debug.LogWarning($"Unknown upgrade type: {type}");
                break;
        }

        GameManager.Instance.NotifyDataChanged();
    }

    public double GetUpgradeValue(EUpgradeType type)
    {
        var upgrade = Get(type);
        return upgrade != null ? upgrade.Damage : 0;
    }
}