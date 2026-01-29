using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 업그레이드 시스템 관리자
// Upgrade 도메인 클래스를 사용하여 게임의 업그레이드를 관리합니다.
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("아이콘 설정")]
    public Sprite appleHarvestIcon;
    public Sprite squirrelHireIcon;
    public Sprite goldenAppleLuckIcon;
    public Sprite feverMasterIcon;
    public Sprite superCriticalIcon;

    [SerializeField] private UpgradeSpecTableSO _specTable;

    private Dictionary<EUpgradeType, Upgrade> _upgrades;

    public IEnumerable<Upgrade> AllUpgrades => _upgrades.Values;

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

        foreach(var specData in _specTable.Datas)
        {

        }
    }

    // 업그레이드 초기화 - 도메인 객체 생성
    void InitializeUpgrades()
    {
        _upgrades = new Dictionary<EUpgradeType, Upgrade>();

        // 1. 사과 수확 강화
        AddUpgrade(new Upgrade(
            type: EUpgradeType.AppleHarvest,
            name: "사과 수확 강화",
            description: "더 큰 사과를 수확합니다\n클릭당 데미지 +10",
            maxLevel: 10,
            baseCost: 500,
            costMultiplier: 1.8,
            icon: appleHarvestIcon
        ));

        // 2. 다람쥐 고용
        AddUpgrade(new Upgrade(
            type: EUpgradeType.SquirrelHire,
            name: "다람쥐 고용",
            description: "부지런한 다람쥐가 자동으로 사과를 모아줍니다",
            maxLevel: 5,
            baseCost: 1000,
            costMultiplier: 2.5,
            icon: squirrelHireIcon
        ));

        // 3. 황금 사과 행운
        AddUpgrade(new Upgrade(
            type: EUpgradeType.GoldenAppleLuck,
            name: "황금 사과 행운",
            description: "황금 사과가 나올 확률이 증가합니다",
            maxLevel: 5,
            baseCost: 2000,
            costMultiplier: 2.0,
            icon: goldenAppleLuckIcon
        ));

        // 4. 피버 타임 마스터
        AddUpgrade(new Upgrade(
            type: EUpgradeType.FeverMaster,
            name: "피버 타임 마스터",
            description: "피버 타임을 더욱 강력하게 만듭니다",
            maxLevel: 5,
            baseCost: 3000,
            costMultiplier: 3.0,
            icon: feverMasterIcon
        ));

        // 5. 슈퍼 크리티컬
        AddUpgrade(new Upgrade(
            type: EUpgradeType.SuperCritical,
            name: "슈퍼 크리티컬",
            description: "크리티컬 발동 시 더 많은 사과를 획득합니다",
            maxLevel: 5,
            baseCost: 5000,
            costMultiplier: 2.5,
            icon: superCriticalIcon
        ));

        // 저장된 레벨 불러오기
        LoadUpgradeLevels();
    }

    //업그레이드 추가 (딕셔너리에 등록)
    private void AddUpgrade(Upgrade upgrade)
    {
        if (_upgrades.ContainsKey(upgrade.Type))
        {
            Debug.LogWarning($"중복된 업그레이드 타입: {upgrade.Type}");
            return;
        }
        _upgrades.Add(upgrade.Type, upgrade);
    }

    // 업그레이드 구매 - 도메인 로직 활용
    public bool PurchaseUpgrade(EUpgradeType type)
    {
        // 1. 업그레이드 찾기
        if (!_upgrades.TryGetValue(type, out Upgrade upgrade))
        {
            Debug.LogError($"업그레이드를 찾을 수 없습니다: {type}");
            return false;
        }

        // 2. 구매 가능 여부 확인 (도메인 로직)
        if (!upgrade.CanAfford(GameManager.Instance.Apples))
        {
            Debug.Log($"{upgrade.Name}: 사과가 부족하거나 최대 레벨입니다!");
            return false;
        }

        // 3. 비용 차감
        double cost = upgrade.CurrentCost;
        if (!GameManager.Instance.SpendApples(cost))
        {
            return false;
        }

        // 4. 레벨업 (도메인 로직)
        if (!upgrade.TryLevelUp())
        {
            // 실패 시 비용 환불
            GameManager.Instance.AddApples(cost);
            return false;
        }

        // 5. 게임 효과 적용
        ApplyUpgradeEffect(upgrade);

        // 6. 저장
        SaveUpgradeLevel(type, upgrade.Level);

        return true;
    }

    // 업그레이드 효과를 GameManager에 적용
    private void ApplyUpgradeEffect(Upgrade upgrade)
    {
        GameManager gm = GameManager.Instance;

        switch (upgrade.Type)
        {
            case EUpgradeType.AppleHarvest:
                gm.ManualDamage += 10;
                gm.OnManualDamageChanged?.Invoke(gm.ManualDamage);
                break;

            case EUpgradeType.SquirrelHire:
                gm.squirrelCount++;
                Debug.Log($"다람쥐 수: {gm.squirrelCount}마리");
                break;

            case EUpgradeType.GoldenAppleLuck:
                gm.criticalChance += 0.05;
                Debug.Log($"크리티컬 확률: {gm.criticalChance * 100:F0}%");
                break;

            case EUpgradeType.FeverMaster:
                ApplyFeverMasterEffect(upgrade.Level, gm);
                break;

            case EUpgradeType.SuperCritical:
                gm.criticalMultiplier += 0.5;
                Debug.Log($"크리티컬 배수: {gm.criticalMultiplier}배");
                break;
        }
    }

    // 피버 마스터 레벨별 효과
    private void ApplyFeverMasterEffect(int level, GameManager gm)
    {
        if (level <= 2)
        {
            gm.feverThreshold -= 10;
            Debug.Log($"피버 발동 조건: {gm.feverThreshold}회 클릭");
        }
        else if (level <= 4)
        {
            gm.feverMultiplier += 0.5;
            Debug.Log($"피버 배수: {gm.feverMultiplier}배");
        }
        else if (level == 5)
        {
            gm.feverDuration *= 1.5f;
            Debug.Log($"피버 지속시간: {gm.feverDuration}초");
        }
    }

    // 업그레이드 조회
    public Upgrade GetUpgrade(EUpgradeType type)
    {
        _upgrades.TryGetValue(type, out Upgrade upgrade);
        return upgrade;
    }

    // 저장
    private void SaveUpgradeLevel(EUpgradeType type, int level)
    {
        PlayerPrefs.SetInt($"Upgrade_{type}", level);
        PlayerPrefs.Save();
    }

    // 불러오기
    private void LoadUpgradeLevels()
    {
        foreach (var upgrade in _upgrades.Values)
        {
            int savedLevel = PlayerPrefs.GetInt($"Upgrade_{upgrade.Type}", 0);

            // 저장된 레벨만큼 효과 재적용
            for (int i = 0; i < savedLevel; i++)
            {
                upgrade.TryLevelUp();
                ApplyUpgradeEffect(upgrade);
            }
        }
    }
}