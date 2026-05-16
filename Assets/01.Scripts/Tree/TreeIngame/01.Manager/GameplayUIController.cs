using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Animation Settings")]
    [SerializeField] private float _scoreAnimationDuration = 0.3f;
    [SerializeField] private float _scorePunchScale = 1.2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        Destroy(gameObject);
    }

    private void Start()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged.AddListener(OnCurrencyChanged);
        }

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.OnDataChanged += UpdateAllUI;
        }

        UpdateAllUI();
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged.RemoveListener(OnCurrencyChanged);
        }

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.OnDataChanged -= UpdateAllUI;
        }
    }

    public void UpdateTreeHealth(double healthPercent)
    {
        if (_treeHealthSlider == null)
        {
            return;
        }

        _treeHealthSlider.value = (float)healthPercent;

        if (_healthFillImage != null && _healthColorGradient != null)
        {
            float normalizedHealth = (float)healthPercent / 100f;
            _healthFillImage.color = _healthColorGradient.Evaluate(normalizedHealth);
        }
    }

    public void UpdateAllUI()
    {
        if (GameplayManager.Instance == null)
        {
            return;
        }

        if (CurrencyManager.Instance != null)
        {
            double apples = CurrencyManager.Instance.Get(ECurrencyType.Apple).Value;
            UpdateAppleScore(apples);
        }

        if (_manualDamageText != null)
        {
            _manualDamageText.text = $"Manual Power: {GameplayManager.Instance.ManualDamage.ToFormattedString()}";
        }

        if (_autoDamageText != null)
        {
            _autoDamageText.text = $"Auto Power: {GameplayManager.Instance.AutoDamage.ToFormattedString()}";
        }

        if (_totalApplesText != null)
        {
            _totalApplesText.text = $"Total Harvest: {GameplayManager.Instance.TotalApplesCollected.ToFormattedString()}";
        }
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
        if (_appleScoreText == null)
        {
            return;
        }

        _appleScoreText.text = score.ToFormattedString();
        AnimateScoreIncrease();
    }

    private void AnimateScoreIncrease()
    {
        if (_appleScoreTransform == null)
        {
            return;
        }

        _appleScoreTransform.DOKill(true);
        _appleScoreTransform.DOPunchScale(
            Vector3.one * (_scorePunchScale - 1f),
            _scoreAnimationDuration,
            1,
            0.5f);

        if (_appleIcon != null)
        {
            _appleIcon.transform.DORotate(
                new Vector3(0f, 0f, 360f),
                0.5f,
                RotateMode.FastBeyond360);
        }
    }
}
