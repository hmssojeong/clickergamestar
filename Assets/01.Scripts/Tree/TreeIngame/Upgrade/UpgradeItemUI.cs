using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public Color affordableColor = new Color(0.2f, 0.8f, 0.2f);
    public Color unaffordableColor = new Color(0.5f, 0.5f, 0.5f);
    public Color maxLevelColor = new Color(1f, 0.84f, 0f);

    [Header("아이콘 크기 설정")]
    public float iconSize = 80f;
    public bool preserveAspect = true;

    // 도메인 객체 참조
    private Upgrade _upgrade;

    public void Initialize(Upgrade upgrade)
    {
        _upgrade = upgrade;

        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }

        // 아이콘 크기 설정
        SetupIcon();

        UpdateUI();
    }

    void SetupIcon()
    {
        if (iconImage != null)
        {
            iconImage.preserveAspect = preserveAspect;

            RectTransform iconRect = iconImage.GetComponent<RectTransform>();
            if (iconRect != null)
            {
                iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            }
        }
    }

    public void UpdateUI()
    {
        if (_upgrade == null) return;

        // 이름
        if (nameText != null)
        {
            nameText.text = _upgrade.Name;
        }

        // 레벨
        if (levelText != null)
        {
            levelText.text = $"Lv.{_upgrade.Level}/{_upgrade.MaxLevel}";
        }

        // 설명
        if (descriptionText != null)
        {
            descriptionText.text = GetDynamicDescription();
        }

        // 최대 레벨 패널
        if (maxLevelPanel != null)
        {
            maxLevelPanel.SetActive(_upgrade.IsMaxLevel);
        }

        // 비용 및 버튼 상태
        UpdateCostAndButton();

        // 아이콘
        if (iconImage != null && _upgrade.Icon != null)
        {
            iconImage.sprite = _upgrade.Icon;
            iconImage.enabled = true;
        }
    }

    private void UpdateCostAndButton()
    {
        if (_upgrade.IsMaxLevel)
        {
            // 최대 레벨
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
        else
        {
            // 구매 가능 레벨
            double cost = _upgrade.CurrentCost;
            double currentApples = GetCurrentApples();
            bool canAfford = _upgrade.CanAfford(currentApples);

            if (costText != null)
            {
                costText.text = $"🍎 {CurrencyFormatter.Format(cost)}";
                costText.color = canAfford ? affordableColor : unaffordableColor;
            }

            if (purchaseButton != null)
            {
                purchaseButton.interactable = canAfford;

                ColorBlock colors = purchaseButton.colors;
                colors.normalColor = canAfford ? affordableColor : unaffordableColor;
                purchaseButton.colors = colors;
            }
        }
    }

    private double GetCurrentApples()
    {
        if (GameManager.Instance == null) return 0;

        var apples = GameManager.Instance.Apples;

        // double 타입인 경우 (가장 일반적)
        if (apples is double doubleValue)
        {
            return doubleValue;
        }

        // Currency 타입인 경우
        try
        {
            // Currency 타입이 존재하는지 확인
            var currencyType = System.Type.GetType("Currency");
            if (currencyType != null && apples.GetType() == currencyType)
            {
                var valueProperty = currencyType.GetProperty("Value");
                if (valueProperty != null)
                {
                    return (double)valueProperty.GetValue(apples);
                }
            }
        }
        catch { }

        // 기본 변환 시도
        try
        {
            return System.Convert.ToDouble(apples);
        }
        catch
        {
            return 0;
        }
    }

    string GetDynamicDescription()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return _upgrade.Description;

        switch (_upgrade.Type)
        {
            case EUpgradeType.AppleHarvest:
                double nextDamage = gm.ManualDamage + 10;
                return $"클릭당 과일점수 증가\n현재: {CurrencyFormatter.Format(gm.ManualDamage)} → 다음: {CurrencyFormatter.Format(nextDamage)}";

            case EUpgradeType.SquirrelHire:
                int currentSquirrels = gm.squirrelCount;
                double currentAutoApples = currentSquirrels * gm.squirrelApplePerSecond;
                double nextAutoApples = (currentSquirrels + 1) * gm.squirrelApplePerSecond;
                return $"자동으로 과일 수확\n현재: 초당 {CurrencyFormatter.Format(currentAutoApples)} → 다음: 초당 {CurrencyFormatter.Format(nextAutoApples)}";

            case EUpgradeType.GoldenAppleLuck:
                double currentChance = gm.criticalChance * 100;
                double nextChance = currentChance + 5.0;
                return $"황금사과 확률 증가\n현재: {currentChance:F0}% → 다음: {nextChance:F0}%";

            case EUpgradeType.FeverMaster:
                return GetFeverMasterDescription(_upgrade.Level, gm);

            case EUpgradeType.SuperCritical:
                double currentMultiplier = gm.criticalMultiplier;
                double nextMultiplier = currentMultiplier + 0.5;
                return $"나무 크리티컬 배수 증가\n현재: {currentMultiplier}배 → 다음: {nextMultiplier}배";

            default:
                return _upgrade.Description;
        }
    }

    private string GetFeverMasterDescription(int level, GameManager gm)
    {
        if (level < 2)
        {
            int currentThreshold = gm.feverThreshold;
            return $"피버 발동 조건 감소\n현재: {currentThreshold}회 → 다음: {currentThreshold - 10}회";
        }
        else if (level < 4)
        {
            double currentMulti = gm.feverMultiplier;
            return $"피버 배수 증가\n현재: {currentMulti}배 → 다음: {currentMulti + 0.5}배";
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
    }

    void OnPurchaseClicked()
    {
        if (UpgradeManager.Instance == null)
        {
            Debug.LogError("UpgradeManager 인스턴스가 없습니다!");
            return;
        }

        if (UpgradeManager.Instance.PurchaseUpgrade(_upgrade.Type))
        {
            UpdateUI();
            PlayPurchaseEffect();

            // 사운드 재생 (SoundManager가 있는 경우에만)
            PlayPurchaseSound();
        }
        else
        {
            Debug.Log($"{_upgrade.Name}: 업그레이드를 구매할 수 없습니다!");

            // 실패 사운드
            PlayErrorSound();
        }
    }

    private void PlayPurchaseSound()
    {
        var soundManager = GameObject.FindObjectOfType<SoundManager>();
        if (soundManager != null)
        {
            try
            {
                var method = soundManager.GetType().GetMethod("PlaySFX");
                if (method != null)
                {
                    // 메서드 호출 시도
                }
            }
            catch
            {
                // 사운드 재생 실패해도 게임은 계속
            }
        }
    }

    private void PlayErrorSound()
    {
        var soundManager = GameObject.FindObjectOfType<SoundManager>();
        if (soundManager != null)
        {
            try
            {
                var method = soundManager.GetType().GetMethod("PlaySFX");
                if (method != null)
                {
                    // 메서드 호출 시도
                }
            }
            catch
            {
                // 사운드 재생 실패해도 게임은 계속
            }
        }
    }


    void PlayPurchaseEffect()
    {
        if (purchaseButton != null)
        {
            StartCoroutine(ButtonScaleEffect());
        }
    }

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