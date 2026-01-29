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
    [SerializeField] private Transform _appleScoreTransform; // 애니메이션용

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

    [Header("Popup UI")]
    [SerializeField] private GameObject _upgradePanel;
    [SerializeField] private GameObject _settingsPanel;

    [Header("Animation Settings")]
    [SerializeField] private float _scoreAnimationDuration = 0.3f;
    [SerializeField] private float _scorePunchScale = 1.2f;

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAppleChanged.AddListener(UpdateAppleScore);
            GameManager.Instance.OnManualDamageChanged.AddListener(UpdateManualDamage);
            GameManager.Instance.OnAutoDamageChanged.AddListener(UpdateAutoDamage);
        }

        // 버튼 이벤트 연결
        _manualUpgradeButton?.onClick.AddListener(OnManualUpgradeClicked);
        _autoUpgradeButton?.onClick.AddListener(OnAutoUpgradeClicked);
        _buyAutoClickerButton?.onClick.AddListener(OnBuyAutoClickerClicked);

        // 초기 UI 업데이트
        UpdateAllUI();
    }

    private void UpdateAppleScore(double score)
    {
        if (_appleScoreText != null)
        {
            _appleScoreText.text = CurrencyFormatter.Format(score);

            // 점수 증가 애니메이션
            AnimateScoreIncrease();
        }
    }

    private void AnimateScoreIncrease()
    {
        if (_appleScoreTransform == null) return;

        // DOTween으로 펀치 애니메이션
        _appleScoreTransform.DOKill(true);

        _appleScoreTransform.DOPunchScale(Vector3.one * (_scorePunchScale - 1f), _scoreAnimationDuration, 1, 0.5f);

        // 아이콘 회전
        if (_appleIcon != null)
        {
            _appleIcon.transform.DORotate(new Vector3(0, 0, 360f), 0.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuad);
        }
    }

    // 수동
    private void UpdateManualDamage(double damage)
    {
        if (_manualDamageText != null)
        {
            _manualDamageText.text = $"클릭 파워: {CurrencyFormatter.Format(damage)}";
        }

        UpdateUpgradeButtons();
    }

    // 자동
    private void UpdateAutoDamage(double damage)
    {
        if (_autoDamageText != null)
        {
            _autoDamageText.text = $"자동 파워: {CurrencyFormatter.Format(damage)}";
        }

        UpdateUpgradeButtons();
    }

    // 업그레이드 버튼 UI 업데이트
    private void UpdateUpgradeButtons()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // 수동 업그레이드 버튼
        if (_manualUpgradeCostText != null)
        {
            _manualUpgradeCostText.text = $"{CurrencyFormatter.Format(gm.ManualUpgradeCost)} 🍎";
            _manualUpgradeButton.interactable = gm.Apples >= gm.ManualUpgradeCost;
        }

        // 자동 업그레이드 버튼
        if (_autoUpgradeCostText != null)
        {
            _autoUpgradeCostText.text = $"{CurrencyFormatter.Format(gm.AutoUpgradeCost)} 🍎";
            _autoUpgradeButton.interactable = gm.Apples >= gm.AutoUpgradeCost;
        }

        // 자동 클리커 구매 버튼
        if (_autoClickerCostText != null)
        {
            _buyAutoClickerButton.interactable = gm.Apples >= gm.AutoClickerCost;

            // 이미 구매했으면 레벨 표시, 아니면 [단위 적용] 비용 표시
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

    // 나무 체력 UI 업데이트
    public void UpdateTreeHealth(double healthPercent)
    {
        if (_treeHealthSlider != null)
        {
            _treeHealthSlider.value = (float)healthPercent;

            // 체력에 따른 색상 변화
            if (_healthFillImage != null && _healthColorGradient != null)
            {
                float normalizedHealth = (float)healthPercent / 100f; // 0~100을 0~1로 변환
                _healthFillImage.color = _healthColorGradient.Evaluate(normalizedHealth);
            }
        }
    }

    public void UpdateAllUI()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        UpdateAppleScore(gm.Apples);
        UpdateManualDamage(gm.ManualDamage);
        UpdateAutoDamage(gm.AutoDamage);

        if (_totalApplesText != null)
        {
            _totalApplesText.text = $"총 수확: {CurrencyFormatter.Format(gm.TotalApplesCollected)}개";
        }

        UpdateUpgradeButtons();
    }

    // 수동 업그레이드 버튼 클릭
    private void OnManualUpgradeClicked()
    {
        if (GameManager.Instance.UpgradeManualDamage())
        {
            // 성공 효과
            PlayUpgradeSuccessEffect(_manualUpgradeButton.transform);
        }
        else
        {
            // 실패 효과
            PlayUpgradeFailEffect(_manualUpgradeButton.transform);
        }
    }

    // 자동 업그레이드 버튼 클릭
    private void OnAutoUpgradeClicked()
    {
        if (GameManager.Instance.UpgradeAutoDamage())
        {
            PlayUpgradeSuccessEffect(_autoUpgradeButton.transform);
        }
        else
        {
            PlayUpgradeFailEffect(_autoUpgradeButton.transform);
        }
    }

    // 자동 클리커 구매 버튼 클릭
    private void OnBuyAutoClickerClicked()
    {
        if (GameManager.Instance.BuyAutoClicker())
        {
            PlayUpgradeSuccessEffect(_buyAutoClickerButton.transform);
        }
        else
        {
            PlayUpgradeFailEffect(_buyAutoClickerButton.transform);
        }
    }

    // 업그레이드 성공 효과
    private void PlayUpgradeSuccessEffect(Transform buttonTransform)
    {
        buttonTransform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1, 0.5f);

        // 파티클이나 사운드 재생
        // EffectManager.Instance.PlayUpgradeSuccess();
    }

    // 업그레이드 실패 효과 (돈 부족)
    private void PlayUpgradeFailEffect(Transform buttonTransform)
    {
        // 좌우 흔들림
        buttonTransform.DOShakePosition(0.5f, 10f, 20, 90f);

        // 사운드 재생
        // AudioManager.Instance.PlayFailSound();
    }

    // 업그레이드 패널 토글
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

    // 설정 패널 토글
    public void ToggleSettingsPanel()
    {
        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(!_settingsPanel.activeSelf);
        }
    }

    // 데미지 텍스트 표시 (나무 위에 떠오르는 효과)
    public void ShowDamageText(Vector3 position, double damage)
    {
        // TODO: Floating Text 프리팹을 사용하여 데미지 표시
        // FloatingTextManager.Instance.ShowText(position, damage.ToString(), Color.red);
        if (FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ShowDamage(position, damage);
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAppleChanged.RemoveListener(UpdateAppleScore);
            GameManager.Instance.OnManualDamageChanged.RemoveListener(UpdateManualDamage);
            GameManager.Instance.OnAutoDamageChanged.RemoveListener(UpdateAutoDamage);
        }
    }
}