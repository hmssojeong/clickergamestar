using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI _appleScoreText;
    [SerializeField] private Image _appleIcon;
    [SerializeField] private Transform _appleScoreTransform;

    [Header("Stats UI")]
    [SerializeField] private TextMeshProUGUI _manualDamageText;
    [SerializeField] private TextMeshProUGUI _autoDamageText;
    [SerializeField] private TextMeshProUGUI _totalApplesText;

    [Header("Upgrade Buttons")]
    [SerializeField] private Button _manualUpgradeButton;
    [SerializeField] private TextMeshProUGUI _manualUpgradeCostText;
    [SerializeField] private Button _autoUpgradeButton;
    [SerializeField] private TextMeshProUGUI _autoUpgradeCostText;
    [SerializeField] private Button _buyAutoClickerButton;
    [SerializeField] private TextMeshProUGUI _autoClickerCostText;

    [Header("Tree Health UI")]
    [SerializeField] private Slider _treeHealthSlider;
    [SerializeField] private Image _healthFillImage;
    [SerializeField] private Gradient _healthColorGradient;

    [Header("Fever UI (선택사항)")]
    [SerializeField] private TextMeshProUGUI _clickCountText;
    [SerializeField] private GameObject _feverPanel;
    [SerializeField] private TextMeshProUGUI _feverTimerText;

    [Header("Popup UI")]
    [SerializeField] private GameObject _upgradePanel;
    [SerializeField] private GameObject _settingsPanel;

    [Header("Animation Settings")]
    [SerializeField] private float _scoreAnimationDuration = 0.3f;
    [SerializeField] private float _scorePunchScale = 1.2f;

    private bool _hasFeverManager = false;
    private bool _wasFeverActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // GameManager 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAppleChanged.AddListener(UpdateAppleScore);
            GameManager.Instance.OnManualDamageChanged.AddListener(UpdateManualDamage);
            GameManager.Instance.OnAutoDamageChanged.AddListener(UpdateAutoDamage);
        }
        else
        {
            Debug.LogWarning("GameManager 인스턴스를 찾을 수 없습니다!");
        }

        // FeverManager 존재 확인
        CheckFeverManager();

        // 버튼 이벤트 연결
        if (_manualUpgradeButton != null)
            _manualUpgradeButton.onClick.AddListener(OnManualUpgradeClicked);
        if (_autoUpgradeButton != null)
            _autoUpgradeButton.onClick.AddListener(OnAutoUpgradeClicked);
        if (_buyAutoClickerButton != null)
            _buyAutoClickerButton.onClick.AddListener(OnBuyAutoClickerClicked);

        // 피버 패널 초기화
        if (_feverPanel != null)
        {
            _feverPanel.SetActive(false);
        }

        UpdateAllUI();
    }

    private void CheckFeverManager()
    {
        _hasFeverManager = FeverManager.Instance != null;

        if (!_hasFeverManager)
        {
            // 피버 UI 숨기기
            if (_clickCountText != null)
                _clickCountText.gameObject.SetActive(false);
            if (_feverPanel != null)
                _feverPanel.SetActive(false);
            if (_feverTimerText != null)
                _feverTimerText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (_hasFeverManager)
        {
            UpdateFeverUI();
        }
    }

    private double GetApplesValue()
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

        try
        {
            return System.Convert.ToDouble(apples);
        }
        catch
        {
            return 0;
        }
    }

    private bool IsGreaterOrEqual(object value1, object value2)
    {
        try
        {
            double d1 = System.Convert.ToDouble(value1);
            double d2 = System.Convert.ToDouble(value2);
            return d1 >= d2;
        }
        catch
        {
            return false;
        }
    }

    private void UpdateFeverUI()
    {
        if (FeverManager.Instance == null)
        {
            _hasFeverManager = false;
            return;
        }

        try
        {
            if (_clickCountText != null)
            {
                int currentClicks = FeverManager.Instance.CurrentClicks;
                int threshold = FeverManager.Instance.GetClicksNeeded();
                _clickCountText.text = $"클릭: {currentClicks}/{threshold}";
            }

            bool isFeverActive = FeverManager.Instance.IsFeverActive;

            if (_feverPanel != null && _feverPanel.activeSelf != isFeverActive)
            {
                _feverPanel.SetActive(isFeverActive);

                if (isFeverActive && !_wasFeverActive)
                {
                    OnFeverStart();
                }
                else if (!isFeverActive && _wasFeverActive)
                {
                    OnFeverEnd();
                }

                _wasFeverActive = isFeverActive;
            }

            if (_feverTimerText != null && isFeverActive)
            {
                float remainingTime = GetFeverRemainingTime();
                _feverTimerText.text = $"FEVER TIME! {remainingTime:F1}초";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"FeverManager UI 업데이트 실패: {e.Message}");
            _hasFeverManager = false;
        }
    }

    private float GetFeverRemainingTime()
    {
        try
        {
            var feverType = FeverManager.Instance.GetType();
            var field = feverType.GetField("_feverTimeRemaining",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                return (float)field.GetValue(FeverManager.Instance);
            }
        }
        catch { }

        return 0f;
    }

    private void OnFeverStart()
    {
        Debug.Log("FEVER TIME 시작!");
    }

    private void OnFeverEnd()
    {
        Debug.Log("피버 타임 종료!");
    }

    private void UpdateAppleScore(double score)
    {
        if (_appleScoreText != null)
        {
            _appleScoreText.text = CurrencyFormatter.Format(score);
            AnimateScoreIncrease();
        }
    }

    private void AnimateScoreIncrease()
    {
        if (_appleScoreTransform == null) return;

        _appleScoreTransform.DOKill(true);
        _appleScoreTransform.DOPunchScale(Vector3.one * (_scorePunchScale - 1f), _scoreAnimationDuration, 1, 0.5f);

        if (_appleIcon != null)
        {
            _appleIcon.transform.DORotate(new Vector3(0, 0, 360f), 0.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuad);
        }
    }

    private void UpdateManualDamage(double damage)
    {
        if (_manualDamageText != null)
        {
            _manualDamageText.text = $"클릭 파워: {CurrencyFormatter.Format(damage)}";
        }
        UpdateUpgradeButtons();
    }

    private void UpdateAutoDamage(double damage)
    {
        if (_autoDamageText != null)
        {
            _autoDamageText.text = $"자동 파워: {CurrencyFormatter.Format(damage)}";
        }
        UpdateUpgradeButtons();
    }


    private void UpdateUpgradeButtons()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        double currentApples = GetApplesValue();

        // 수동 업그레이드 버튼
        if (_manualUpgradeCostText != null && _manualUpgradeButton != null)
        {
            _manualUpgradeCostText.text = $"{CurrencyFormatter.Format(gm.ManualUpgradeCost)} 🍎";
            _manualUpgradeButton.interactable = IsGreaterOrEqual(currentApples, gm.ManualUpgradeCost);
        }

        // 자동 업그레이드 버튼
        if (_autoUpgradeCostText != null && _autoUpgradeButton != null)
        {
            _autoUpgradeCostText.text = $"{CurrencyFormatter.Format(gm.AutoUpgradeCost)} 🍎";
            _autoUpgradeButton.interactable = IsGreaterOrEqual(currentApples, gm.AutoUpgradeCost);
        }

        // 자동 클리커 버튼
        if (_autoClickerCostText != null && _buyAutoClickerButton != null)
        {
            _buyAutoClickerButton.interactable = IsGreaterOrEqual(currentApples, gm.AutoClickerCost);

            if (gm.HasAutoClicker)
            {
                _autoClickerCostText.text = $"Lv.{gm.AutoClickerLevel}";
            }
            else
            {
                _autoClickerCostText.text = $"{CurrencyFormatter.Format(gm.AutoClickerCost)} 🍎";
            }
        }
    }

    public void UpdateTreeHealth(double healthPercent)
    {
        if (_treeHealthSlider != null)
        {
            _treeHealthSlider.value = (float)healthPercent;

            if (_healthFillImage != null && _healthColorGradient != null)
            {
                float normalizedHealth = (float)healthPercent / 100f;
                _healthFillImage.color = _healthColorGradient.Evaluate(normalizedHealth);
            }
        }
    }


    public void UpdateAllUI()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogWarning("GameManager가 없어 UI를 업데이트할 수 없습니다!");
            return;
        }

        double applesValue = GetApplesValue();
        UpdateAppleScore(applesValue);
        UpdateManualDamage(gm.ManualDamage);
        UpdateAutoDamage(gm.AutoDamage);

        if (_totalApplesText != null)
        {
            _totalApplesText.text = $"총 수확: {CurrencyFormatter.Format(gm.TotalApplesCollected)}개";
        }

        UpdateUpgradeButtons();
    }

    private void OnManualUpgradeClicked()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.UpgradeManualDamage())
        {
            PlayUpgradeSuccessEffect(_manualUpgradeButton.transform);
        }
        else
        {
            PlayUpgradeFailEffect(_manualUpgradeButton.transform);
        }
    }

    private void OnAutoUpgradeClicked()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.UpgradeAutoDamage())
        {
            PlayUpgradeSuccessEffect(_autoUpgradeButton.transform);
        }
        else
        {
            PlayUpgradeFailEffect(_autoUpgradeButton.transform);
        }
    }

    private void OnBuyAutoClickerClicked()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.BuyAutoClicker())
        {
            PlayUpgradeSuccessEffect(_buyAutoClickerButton.transform);
        }
        else
        {
            PlayUpgradeFailEffect(_buyAutoClickerButton.transform);
        }
    }

    private void PlayUpgradeSuccessEffect(Transform buttonTransform)
    {
        if (buttonTransform == null) return;
        buttonTransform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1, 0.5f);
    }

    private void PlayUpgradeFailEffect(Transform buttonTransform)
    {
        if (buttonTransform == null) return;
        buttonTransform.DOShakePosition(0.5f, 10f, 20, 90f);
    }

    public void ToggleUpgradePanel()
    {
        if (_upgradePanel != null)
        {
            bool isActive = !_upgradePanel.activeSelf;
            _upgradePanel.SetActive(isActive);

            if (isActive)
            {
                UpdateAllUI();
            }
        }
    }

    public void ToggleSettingsPanel()
    {
        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(!_settingsPanel.activeSelf);
        }
    }

    public void ShowDamageText(Vector3 position, double damage)
    {
        if (FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ShowDamage(position, damage);
        }
    }

    private void OnDestroy()
    {
        // GameManager 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAppleChanged.RemoveListener(UpdateAppleScore);
            GameManager.Instance.OnManualDamageChanged.RemoveListener(UpdateManualDamage);
            GameManager.Instance.OnAutoDamageChanged.RemoveListener(UpdateAutoDamage);
        }
    }
}
