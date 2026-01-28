using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 개별 업그레이드 아이템 UI 컴포넌트 (사용자 GameManager와 통합)
/// </summary>
public class UpgradeItemUI : MonoBehaviour
{
    [Header("UI 참조")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public Button purchaseButton;
    public Image iconImage;
    public GameObject maxLevelPanel;
    
    [Header("색상 설정")]
    public Color affordableColor = new Color(0.2f, 0.8f, 0.2f);  // 구매 가능 (초록)
    public Color unaffordableColor = new Color(0.5f, 0.5f, 0.5f); // 구매 불가 (회색)
    public Color maxLevelColor = new Color(1f, 0.84f, 0f);        // 최대 레벨 (금색)
    
    private UpgradeData upgradeData;
    
    /// <summary>
    /// 업그레이드 데이터로 초기화
    /// </summary>
    public void Initialize(UpgradeData data)
    {
        upgradeData = data;
        
        // 구매 버튼 이벤트 연결
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }
        
        UpdateUI();
    }
    
    /// <summary>
    /// UI 업데이트
    /// </summary>
    public void UpdateUI()
    {
        if (upgradeData == null) return;
        
        // 이름
        if (nameText != null)
        {
            nameText.text = upgradeData.upgradeName;
        }
        
        // 레벨
        if (levelText != null)
        {
            levelText.text = $"Lv.{upgradeData.currentLevel}/{upgradeData.maxLevel}";
        }
        
        // 설명 (레벨에 따라 동적으로 변경)
        if (descriptionText != null)
        {
            descriptionText.text = GetDynamicDescription();
        }
        
        // 최대 레벨 여부
        bool isMaxLevel = upgradeData.IsMaxLevel();
        
        if (maxLevelPanel != null)
        {
            maxLevelPanel.SetActive(isMaxLevel);
        }
        
        // 비용 및 구매 가능 여부
        if (!isMaxLevel)
        {
            double cost = upgradeData.GetCurrentCost();
            bool canAfford = upgradeData.CanUpgrade(GameManager.Instance.Apples);
            
            if (costText != null)
            {
                costText.text = $"🍎 {FormatNumber(cost)}";
                costText.color = canAfford ? affordableColor : unaffordableColor;
            }
            
            if (purchaseButton != null)
            {
                purchaseButton.interactable = canAfford;
                
                // 버튼 색상 변경
                ColorBlock colors = purchaseButton.colors;
                colors.normalColor = canAfford ? affordableColor : unaffordableColor;
                purchaseButton.colors = colors;
            }
        }
        else
        {
            if (costText != null)
            {
                costText.text = "MAX";
                costText.color = maxLevelColor;
            }
            
            if (purchaseButton != null)
            {
                purchaseButton.interactable = false;
            }
        }
        
        // 아이콘 (옵션)
        if (iconImage != null && upgradeData.icon != null)
        {
            iconImage.sprite = upgradeData.icon;
        }
    }
    
    /// <summary>
    /// 레벨에 따른 동적 설명 생성
    /// </summary>
    string GetDynamicDescription()
    {
        GameManager gm = GameManager.Instance;
        
        switch (upgradeData.type)
        {
            case UpgradeType.AppleHarvest:
                double nextDamage = gm.ManualDamage + 10;
                return $"클릭당 데미지 증가\n현재: {FormatNumber(gm.ManualDamage)} → 다음: {FormatNumber(nextDamage)}";
                
            case UpgradeType.SquirrelHire:
                int currentSquirrels = gm.squirrelCount;
                double currentAutoApples = currentSquirrels * gm.squirrelApplePerSecond;
                double nextAutoApples = (currentSquirrels + 1) * gm.squirrelApplePerSecond;
                return $"자동으로 사과 수확\n현재: 초당 {FormatNumber(currentAutoApples)} → 다음: 초당 {FormatNumber(nextAutoApples)}";
                
            case UpgradeType.GoldenAppleLuck:
                double currentChance = gm.criticalChance * 100;
                double nextChance = currentChance + 5.0d;
                return $"크리티컬 확률 증가\n현재: {currentChance:F0}% → 다음: {nextChance:F0}%";
                
            case UpgradeType.FeverMaster:
                int level = upgradeData.currentLevel;
                if (level < 2)
                {
                    int currentThreshold = gm.feverThreshold;
                    return $"피버 발동 조건 감소\n현재: {currentThreshold}회 → 다음: {currentThreshold - 10}회";
                }
                else if (level < 4)
                {
                    double currentMulti = gm.feverMultiplier;
                    return $"피버 배수 증가\n현재: {currentMulti}배 → 다음: {currentMulti + 0.5d}배";
                }
                else if (level == 4)
                {
                    float currentDuration = gm.feverDuration;
                    return $"피버 지속시간 증가\n현재: {currentDuration}초 → 다음: {currentDuration * 1.5f}초";
                }
                else
                {
                    return "피버 타임 마스터 완성!";
                }
                
            case UpgradeType.SuperCritical:
                double currentMultiplier = gm.criticalMultiplier;
                double nextMultiplier = currentMultiplier + 0.5d;
                return $"크리티컬 배수 증가\n현재: {currentMultiplier}배 → 다음: {nextMultiplier}배";
                
            default:
                return upgradeData.description;
        }
    }
    
    /// <summary>
    /// 숫자 포맷팅 (큰 숫자도 읽기 쉽게)
    /// </summary>
    string FormatNumber(double number)
    {
        if (number >= 1000000000000) // 1조 이상
            return (number / 1000000000000).ToString("0.##") + "T";
        else if (number >= 1000000000) // 10억 이상
            return (number / 1000000000).ToString("0.##") + "B";
        else if (number >= 1000000) // 100만 이상
            return (number / 1000000).ToString("0.##") + "M";
        else if (number >= 1000) // 1천 이상
            return (number / 1000).ToString("0.##") + "K";
        else
            return number.ToString("0");
    }
    
    /// <summary>
    /// 구매 버튼 클릭 처리
    /// </summary>
    void OnPurchaseClicked()
    {
        if (UpgradeManager.Instance.PurchaseUpgrade(upgradeData.type))
        {
            UpdateUI();
            
            // 구매 성공 이펙트 (옵션)
            PlayPurchaseEffect();
        }
        else
        {
            // 구매 실패 피드백
            Debug.Log("업그레이드를 구매할 수 없습니다!");
        }
    }
    
    /// <summary>
    /// 구매 성공 이펙트 (옵션)
    /// </summary>
    void PlayPurchaseEffect()
    {
        // 버튼 스케일 애니메이션이나 파티클 효과 추가 가능
        if (purchaseButton != null)
        {
            StartCoroutine(ButtonScaleEffect());
        }
    }
    
    /// <summary>
    /// 버튼 스케일 이펙트
    /// </summary>
    System.Collections.IEnumerator ButtonScaleEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.1f;
        
        float duration = 0.1f;
        float elapsed = 0f;
        
        // 확대
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }
        
        elapsed = 0f;
        
        // 축소
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
}
