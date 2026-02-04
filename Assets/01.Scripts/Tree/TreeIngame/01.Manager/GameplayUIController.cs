using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameplayUIController : MonoBehaviour
{
    public static GameplayUIController Instance { get; private set; }

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI _appleScoreText;
    [SerializeField] private Image _appleIcon;
    [SerializeField] private Transform _appleScoreTransform;

    [Header("Stats UI")]
    [SerializeField] private TextMeshProUGUI _manualDamageText;
    [SerializeField] private TextMeshProUGUI _autoDamageText;
    [SerializeField] private TextMeshProUGUI _totalApplesText;

    [Header("Tree Health UI")]
    [SerializeField] private Slider _treeHealthSlider;
    [SerializeField] private Image _healthFillImage;
    [SerializeField] private Gradient _healthColorGradient;

    [Header("Fever UI")]
    [SerializeField] private TextMeshProUGUI _clickCountText;
    [SerializeField] private GameObject _feverPanel;
    [SerializeField] private TextMeshProUGUI _feverTimerText;

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
        // 이벤트 구독
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged.AddListener(OnCurrencyChanged);
        }

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.OnDataChanged.AddListener(UpdateAllUI);
        }

        if (_feverPanel != null)
        {
            _feverPanel.SetActive(false);
        }

        UpdateAllUI();
    }

    private void Update()
    {
        UpdateFeverUI();
    }

    private void OnCurrencyChanged(ECurrencyType type, Currency amount)
    {
        if (type == ECurrencyType.Apple)
        {
            UpdateAppleScore(amount.Value);
        }
    }

    private void UpdateAppleScore(double score)
    {
        if (_appleScoreText != null)
        {
            _appleScoreText.text = score.ToFormattedString();
            AnimateScoreIncrease();
        }
    }

    private void AnimateScoreIncrease()
    {
        if (_appleScoreTransform == null) return;

        _appleScoreTransform.DOKill(true);
        _appleScoreTransform.DOPunchScale(
            Vector3.one * (_scorePunchScale - 1f),
            _scoreAnimationDuration, 1, 0.5f);

        if (_appleIcon != null)
        {
            _appleIcon.transform.DORotate(
                new Vector3(0, 0, 360f),
                0.5f, RotateMode.FastBeyond360);
        }
    }

    private void UpdateFeverUI()
    {
        if (GameplayManager.Instance == null) return;

        // 클릭 카운트 표시
        if (_clickCountText != null)
        {
            int currentClicks = GameplayManager.Instance.GetFeverClickCount();
            _clickCountText.text = $"클릭: {currentClicks}/75";
        }

        // 피버 패널 표시/숨김
        bool isFeverActive = GameplayManager.Instance.IsFeverActive();

        if (_feverPanel != null && _feverPanel.activeSelf != isFeverActive)
        {
            _feverPanel.SetActive(isFeverActive);
        }

        // 피버 타이머 표시
        if (_feverTimerText != null && isFeverActive)
        {
            float remainingTime = GameplayManager.Instance.GetFeverTimeRemaining();
            _feverTimerText.text = $"FEVER TIME! {remainingTime:F1}초";
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
        if (GameplayManager.Instance == null) return;

        // 사과 점수
        if (CurrencyManager.Instance != null)
        {
            double apples = CurrencyManager.Instance.Get(ECurrencyType.Apple).Value;
            UpdateAppleScore(apples);
        }

        // 스탯
        if (_manualDamageText != null)
        {
            _manualDamageText.text = $"클릭 파워: {GameplayManager.Instance.ManualDamage.ToFormattedString()}";
        }

        if (_autoDamageText != null)
        {
            _autoDamageText.text = $"자동 파워: {GameplayManager.Instance.AutoDamage.ToFormattedString()}";
        }

        if (_totalApplesText != null)
        {
            _totalApplesText.text = $"총 수확: {GameplayManager.Instance.TotalApplesCollected.ToFormattedString()}개";
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged.RemoveListener(OnCurrencyChanged);
        }

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.OnDataChanged.RemoveListener(UpdateAllUI);
        }
    }
}