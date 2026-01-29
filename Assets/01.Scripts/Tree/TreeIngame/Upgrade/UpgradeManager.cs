using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

// 업그레이드 시스템 관리자
// Upgrade 도메인 클래스를 사용하여 게임의 업그레이드를 관리합니다.
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    public static event Action OnDataChanged;

    [SerializeField] private UpgradeSpecTableSO _specTable;

    private Dictionary<EUpgradeType, Upgrade> _upgrades = new();

    public IReadOnlyDictionary<EUpgradeType, Upgrade> AllUpgrades => _upgrades;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeUpgrades();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 업그레이드 초기화 - 도메인 객체 생성
    void InitializeUpgrades()
    {
        foreach (var specData in _specTable.Datas)
        {
            if(_upgrades.ContainsKey(specData.Type))
            {
                throw new Exception($"There is already an upgrade with type {specData.Type}");
            }

            _upgrades.Add(specData.Type, new Upgrade(specData));
        }

        OnDataChanged?.Invoke();
    }

    public Upgrade Get(EUpgradeType type) => _upgrades[type] ?? null;
    public List<Upgrade> GetAll() =>_upgrades.Values.ToList();

    public bool CanLevelUp(EUpgradeType type)
    {
        if(!_upgrades.TryGetValue(type, out Upgrade upgrade))
        {
            return false;
        }

        if(!upgrade.CanLevelUp())
        {
            return false;
        }

        return CurrencyManager.Instance.CanAfford(ECurrencyType.Apple, upgrade.Cost);
    }

    public bool TryLevelUp(EUpgradeType type)
    {
        if (!_upgrades.TryGetValue(type,out Upgrade upgrade))
        {
            return false;
        }

        if(!CurrencyManager.Instance.TrySpend(ECurrencyType.Apple, upgrade.Cost))
        {
            return false;
        }

        if(!upgrade.TryLevelUp())
        {
            return false;
        }

        OnDataChanged?.Invoke();

        return true;
    }
}