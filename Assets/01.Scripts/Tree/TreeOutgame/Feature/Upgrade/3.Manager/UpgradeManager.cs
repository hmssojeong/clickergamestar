using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// 업그레이드 시스템 관리자
/// Repository 패턴을 사용하여 데이터 저장/로드
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    public static event Action OnDataChanged;

    [Header("설정")]
    [SerializeField] private UpgradeSpecTableSO _specTable;

    private IUpgradeRepository _repository;
    private Dictionary<EUpgradeType, Upgrade> _upgrades = new();

    public IReadOnlyDictionary<EUpgradeType, Upgrade> AllUpgrades => _upgrades;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeRepository();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Repository 초기화 (Local 또는 Firebase)
    private void InitializeRepository()
    {
        string userId = AccountManager.Instance?.Email ?? "guest";

/*        _repository = _useFirebase
            ? (IUpgradeRepository)new FirebaseUpgradeRepository(userId)
            : new LocalUpgradeRepository(userId);

        Debug.Log($"[UpgradeManager] Repository 초기화 완료 - 모드: {(_useFirebase ? "Firebase" : "Local")}, UserID: {userId}");*/
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

            _upgrades.Add(specData.Type, new Upgrade(specData, level));
        }

        ApplyAllUpgradeEffects();
        OnDataChanged?.Invoke();
        
        Debug.Log($"[UpgradeManager] 업그레이드 초기화 완료 - 총 {_upgrades.Count}개");
    }

    // 업그레이드 데이터 저장
    public void SaveUpgrades()
    {
        var saveData = new UpgradeSaveData();

        foreach (var upgrade in _upgrades)
        {
            saveData.UpgradeLevels[upgrade.Key] = upgrade.Value.Level;
        }

        _repository.Save(saveData);
        Debug.Log("[UpgradeManager] 업그레이드 저장 완료");
    }

    // 업그레이드 데이터 로드
    public void LoadUpgrades()
    {
        var saveData = _repository.Load();
        InitializeUpgrades(saveData.UpgradeLevels);
        Debug.Log("[UpgradeManager] 업그레이드 로드 완료");
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
            Debug.LogWarning($"[UpgradeManager] 업그레이드 타입 {type}을 찾을 수 없습니다.");
            return false;
        }

        // 비용 차감
        if (!CurrencyManager.Instance.TrySpend(ECurrencyType.Apple, upgrade.Cost))
        {
            Debug.LogWarning($"[UpgradeManager] 재화 부족 - 필요: {upgrade.Cost}");
            return false;
        }

        // 레벨업 수행
        if (!upgrade.TryLevelUp())
        {
            // 실패 시 비용 환불
            CurrencyManager.Instance.Add(ECurrencyType.Apple, upgrade.Cost);
            Debug.LogError($"[UpgradeManager] 레벨업 실패 - 비용 환불");
            return false;
        }

        // 업그레이드 효과를 게임에 적용
        ApplyUpgradeEffect(type, upgrade);

        OnDataChanged?.Invoke();

        Debug.Log($"✨ [UpgradeManager] {upgrade.SpecData.Name} 레벨업! (Lv.{upgrade.Level})");
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