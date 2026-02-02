using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

// 업그레이드 시스템 관리자
// Upgrade 도메인 클래스를 사용하여 게임의 업그레이드를 관리
// 업그레이드 시 실제 게임 스탯에 효과 적용
// SaveLoadManager를 통해 저장/로드
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
            DontDestroyOnLoad(gameObject);
            InitializeUpgrades(null); // 기본 0레벨로 초기화
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 업그레이드 초기화 - 도메인 객체 생성
    public void InitializeUpgrades(Dictionary<EUpgradeType, int> savedLevels = null)
    {
        _upgrades.Clear();

        foreach (var specData in _specTable.Datas)
        {
            // 저장된 레벨이 있으면 가져오고, 없으면 0
            int level = (savedLevels != null && savedLevels.ContainsKey(specData.Type))
                        ? savedLevels[specData.Type] : 0;

            _upgrades.Add(specData.Type, new Upgrade(specData, level)); // Upgrade 생성자에 level 매개변수 추가 필요
        }

        ApplyAllUpgradeEffects();

        OnDataChanged?.Invoke();
    }

    // 업그레이드 조회
    public Upgrade Get(EUpgradeType type)
    {
        return _upgrades.TryGetValue(type, out var upgrade) ? upgrade : null;
    }

    // 모든 업그레이드 조회
    public List<Upgrade> GetAll()
    {
        return _upgrades.Values.ToList();
    }

    // 레벨업 가능 여부 확인
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

        return CurrencyManager.Instance.CanAfford(ECurrencyType.Apple, upgrade.Cost);
    }

    // 레벨업 시도
    public bool TryLevelUp(EUpgradeType type)
    {
        if (!_upgrades.TryGetValue(type, out Upgrade upgrade))
        {
            Debug.LogWarning($"업그레이드 타입 {type}을 찾을 수 없습니다.");
            return false;
        }

        // 비용 차감
        if (!CurrencyManager.Instance.TrySpend(ECurrencyType.Apple, upgrade.Cost))
        {
            return false;
        }

        // 레벨업 수행
        if (!upgrade.TryLevelUp())
        {
            // 실패 시 비용 환불
            CurrencyManager.Instance.Add(ECurrencyType.Apple, upgrade.Cost);
            return false;
        }

        // 업그레이드 효과를 게임에 적용
        ApplyUpgradeEffect(type, upgrade);

        OnDataChanged?.Invoke();

        // SaveLoadManager를 통해 자동 저장
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame();
        }

        Debug.Log($"✨ {upgrade.SpecData.Name} 레벨업! (Lv.{upgrade.Level})");
        return true;
    }

    private void ApplyAllUpgradeEffects()
    {
        foreach (var upgrade in _upgrades)
        {
            ApplyUpgradeEffect(upgrade.Key, upgrade.Value);
        }
    }

    // 특정 업그레이드 효과를 게임에 적용
    private void ApplyUpgradeEffect(EUpgradeType type, Upgrade upgrade)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager가 없어 업그레이드 효과를 적용할 수 없습니다.");
            return;
        }

        switch (type)
        {
            case EUpgradeType.AppleHarvest:
                // 수동 클릭 데미지 적용
                GameManager.Instance.ManualDamage = upgrade.Damage;
                Debug.Log($"사과 수확력: {upgrade.Damage}");
                break;

            case EUpgradeType.SquirrelHire:
                // 다람쥐 수 적용
                GameManager.Instance.squirrelCount = (int)upgrade.Damage;
                Debug.Log($"다람쥐 수: {upgrade.Damage}마리");
                break;

            case EUpgradeType.GoldenAppleLuck:
                // 크리티컬 확률 적용 (0~100을 0.0~1.0으로 변환)
                GameManager.Instance.criticalChance = upgrade.Damage / 100.0;
                Debug.Log($"크리티컬 확률: {upgrade.Damage}%");
                break;

            case EUpgradeType.FeverMaster:
                // 피버 배수 적용
                GameManager.Instance.feverMultiplier = upgrade.Damage;
                Debug.Log($"피버 배수: x{upgrade.Damage}");
                break;

            case EUpgradeType.SuperCritical:
                // 크리티컬 배수 적용
                GameManager.Instance.criticalMultiplier = upgrade.Damage;
                Debug.Log($"크리티컬 배수: x{upgrade.Damage}");
                break;

            default:
                Debug.LogWarning($"알 수 없는 업그레이드 타입: {type}");
                break;
        }

        // GameManager의 UI 갱신 트리거
        GameManager.Instance.NotifyDataChanged();
    }

    // 특정 업그레이드의 현재 효과 값 조회
    public double GetUpgradeValue(EUpgradeType type)
    {
        var upgrade = Get(type);
        return upgrade != null ? upgrade.Damage : 0;
    }
}