using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    
    [Header("업그레이드 데이터")]
    public List<UpgradeData> allUpgrades = new List<UpgradeData>();

    [Header("아이콘 설정")]
    public Sprite appleHarvestIcon;   
    public Sprite squirrelHireIcon;    
    public Sprite goldenAppleLuckIcon; 
    public Sprite feverMasterIcon; 
    public Sprite superCriticalIcon;

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
    
    void InitializeUpgrades()
    {
        allUpgrades = new List<UpgradeData>
        {
            // 1. 사과 수확 강화 - ManualDamage 증가
            new UpgradeData
            {
                type = UpgradeType.AppleHarvest,
                upgradeName = "사과 수확 강화",
                description = "더 큰 사과를 수확합니다\n클릭당 데미지 +10",
                icon = appleHarvestIcon,
                baseCost = 500,
                costMultiplier = 1.8f,
                maxLevel = 10
            },
            
            // 2. 다람쥐 고용 - 자동 수확
            new UpgradeData
            {
                type = UpgradeType.SquirrelHire,
                upgradeName = "다람쥐 고용",
                description = "부지런한 다람쥐가 자동으로 사과를 모아줍니다\n초당 +50 사과",
                icon = squirrelHireIcon,
                baseCost = 1000,
                costMultiplier = 2.5f,
                maxLevel = 5
            },
            
            // 3. 황금 사과 행운 - 크리티컬 확률 증가
            new UpgradeData
            {
                type = UpgradeType.GoldenAppleLuck,
                upgradeName = "황금 사과 행운",
                description = "황금 사과가 나올 확률이 증가합니다\n크리티컬 확률 +5%",
                icon = goldenAppleLuckIcon,
                baseCost = 2000,
                costMultiplier = 2.0f,
                maxLevel = 5
            },
            
            // 4. 피버 타임 마스터
            new UpgradeData
            {
                type = UpgradeType.FeverMaster,
                upgradeName = "피버 타임 마스터",
                description = "피버 타임을 더욱 강력하게 만듭니다",
                icon = feverMasterIcon,
                baseCost = 3000,
                costMultiplier = 3.0f,
                maxLevel = 5
            },
            
            // 5. 슈퍼 크리티컬 - 크리티컬 배수 증가
            new UpgradeData
            {
                type = UpgradeType.SuperCritical,
                upgradeName = "슈퍼 크리티컬",
                description = "크리티컬 발동 시 더 많은 사과를 획득합니다\n크리티컬 배수 +0.5배",
                icon = superCriticalIcon,
                baseCost = 5000,
                costMultiplier = 2.5f,
                maxLevel = 5
            }
        };
        
        // 저장된 레벨 불러오기
        LoadUpgradeLevels();
    }
    
    public bool PurchaseUpgrade(UpgradeType type)
    {
        UpgradeData upgrade = allUpgrades.FirstOrDefault(u => u.type == type);
        
        if (upgrade == null)
        {
            Debug.LogError($"업그레이드 타입을 찾을 수 없습니다: {type}");
            return false;
        }
        
        // 구매 가능 여부 확인
        if (!upgrade.CanUpgrade(GameManager.Instance.Apples))
        {
            Debug.Log("사과가 부족하거나 최대 레벨입니다!");
            return false;
        }
        
        // 비용 차감
        double cost = upgrade.GetCurrentCost();
        if (!GameManager.Instance.SpendApples(cost))
        {
            return false;
        }
        
        // 레벨 업
        upgrade.currentLevel++;
        
        // 효과 적용
        ApplyUpgradeEffect(type, upgrade.currentLevel);
        
        // 레벨 저장
        SaveUpgradeLevel(type, upgrade.currentLevel);
        
        Debug.Log($"{upgrade.upgradeName} 레벨 {upgrade.currentLevel} 달성!");
        return true;
    }
    
    void ApplyUpgradeEffect(UpgradeType type, int level)
    {
        GameManager gm = GameManager.Instance;
        
        switch (type)
        {
            case UpgradeType.AppleHarvest:
                // ManualDamage +10 증가
                gm.ManualDamage += 10;
                gm.OnManualDamageChanged?.Invoke(gm.ManualDamage);
                Debug.Log($"수동 데미지: {gm.ManualDamage}");
                break;
                
            case UpgradeType.SquirrelHire:
                // 다람쥐 1마리 추가
                gm.squirrelCount++;
                Debug.Log($"다람쥐 수: {gm.squirrelCount}마리 (초당 {gm.squirrelCount * gm.squirrelApplePerSecond} 사과)");
                break;
                
            case UpgradeType.GoldenAppleLuck:
                // 크리티컬 확률 +5% 증가
                gm.criticalChance += 0.05d;
                double totalCritChance = gm.criticalChance * 100;
                Debug.Log($"크리티컬 확률: {totalCritChance:F0}%");
                break;
                
            case UpgradeType.FeverMaster:
                // 레벨에 따라 다른 효과
                if (level <= 2)
                {
                    // 레벨 1-2: 피버 발동 조건 -10 감소
                    gm.feverThreshold -= 10;
                    Debug.Log($"피버 발동 조건: {gm.feverThreshold}회 클릭");
                }
                else if (level <= 4)
                {
                    // 레벨 3-4: 피버 배수 +0.5 증가
                    gm.feverMultiplier += 0.5d;
                    Debug.Log($"피버 배수: {gm.feverMultiplier}배");
                }
                else if (level == 5)
                {
                    // 레벨 5: 피버 지속시간 +50%
                    gm.feverDuration *= 1.5f;
                    Debug.Log($"피버 지속시간: {gm.feverDuration}초");
                }
                break;
                
            case UpgradeType.SuperCritical:
                // 크리티컬 배수 +0.5 증가
                gm.criticalMultiplier += 0.5d;
                Debug.Log($"크리티컬 배수: {gm.criticalMultiplier}배");
                break;
        }
    }
    
    public UpgradeData GetUpgradeData(UpgradeType type)
    {
        return allUpgrades.FirstOrDefault(u => u.type == type);
    }
    
    // 업그레이드 저장
    void SaveUpgradeLevel(UpgradeType type, int level)
    {
        PlayerPrefs.SetInt($"Upgrade_{type}", level);
        PlayerPrefs.Save();
    }
    
    void LoadUpgradeLevels()
    {
        foreach (var upgrade in allUpgrades)
        {
            int savedLevel = PlayerPrefs.GetInt($"Upgrade_{upgrade.type}", 0);
            
            // 저장된 레벨만큼 효과 적용
            for (int i = 0; i < savedLevel; i++)
            {
                upgrade.currentLevel++;
                ApplyUpgradeEffect(upgrade.type, upgrade.currentLevel);
            }
        }
    }
    
    // 모든 업그레이드 리셋 (테스트용)
    public void ResetAllUpgrades()
    {
        foreach (var upgrade in allUpgrades)
        {
            upgrade.currentLevel = 0;
            PlayerPrefs.DeleteKey($"Upgrade_{upgrade.type}");
        }
        
        // GameManager 보너스 초기화
        GameManager gm = GameManager.Instance;
        gm.criticalChance = 0.1d;
        gm.criticalMultiplier = 2.0d;
        gm.squirrelCount = 0;
        gm.feverThreshold = 75;
        gm.feverMultiplier = 2.5d;
        gm.feverDuration = 10f;
        
        Debug.Log("모든 업그레이드가 초기화되었습니다!");
    }
}
