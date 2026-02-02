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
            CurrencyManager.Instance.OnCurrencyChanged.AddListener(OnCurrencyChanged);
        }
       
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnDataChanged.AddListener(UpdateAllUI);
        }

        if(_feverPanel != null)
        {
            _feverPanel.SetActive(false);
        }
    
        UpdateAllUI();
    }

    private void Update()
    {
        UpdateFeverUI();
    }

    // 재화 변경 시 호출
    private void OnCurrencyChanged(ECurrencyType type, Currency amount)
    {
        if(type == ECurrencyType.Apple)
        {
            UpdateAppleScore(amount.Value);
        }
    }

    // 사과 점수 UI 업데이트
    private void UpdateAppleScore(double score)
    {
        if(_appleScoreText != null)
        {
            _appleScoreText.text = score.ToFormattedString();
            AnimateScoreIncrease();
        }
    }

    private void AnimateScoreIncrease()
    {
        if(_appleScoreTransform == null)
        {
            return;
        }

        _appleScoreTransform.DOKill(true);
        _appleScoreTransform.DOPunchScale(Vector3.one * (_scorePunchScale - 1f), _scoreAnimationDuration, 1, 0.5f);

        if(_appleIcon != null)
        {
            _appleIcon.transform.DORotate(new Vector3(0, 0, 360f), 0.5f, RotateMode.FastBeyond360);
        }
    }
   
    private void UpdateFeverUI()
    {
        if(GameManager.Instance == null)
        {
            return;
        }

        if(_clickCountText != null)
        {
            int currentClicks = GameManager.Instance.clickCount;
            int threshold =  GameManager.Instance.feverThreshold;
            _clickCountText.text = $"클릭: {currentClicks}/{threshold}";
        }

        bool isFeverActive = GameManager.Instance.isFeverActive;

        if (_feverPanel != null && _feverPanel.activeSelf != isFeverActive)
        {
            _feverPanel.SetActive(isFeverActive);
        }

        if (_feverTimerText != null && isFeverActive)
        {
            float remainingTime = GameManager.Instance.GetFeverRemainingTime();
            _feverTimerText.text = $"FEVER TIME! {remainingTime:F1}초";
        }
    }

    // 나무 체력 UI 업데이트
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

    // 모든 UI 업데이트
    public void UpdateAllUI()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogWarning("GameManager가 없어 UI를 업데이트할 수 없습니다!");
            return;
        }

        // 사과 점수 업데이트
        if (CurrencyManager.Instance != null)
        {
            double apples = CurrencyManager.Instance.Get(ECurrencyType.Apple).Value;
            UpdateAppleScore(apples);
        }

        // 스탯 업데이트
        if (_manualDamageText != null)
        {
            _manualDamageText.text = $"클릭 파워:{gm.ManualDamage.ToFormattedString()}";
        }

        if (_autoDamageText != null)
        {
            _autoDamageText.text = $"자동 파워: {gm.AutoDamage.ToFormattedString()}";
        }

        if (_totalApplesText != null)
        {
            _totalApplesText.text = $"총 수확: {gm.TotalApplesCollected.ToFormattedString()}개";
        }
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
        // CurrencyManager 이벤트 구독 해제
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged.RemoveListener(OnCurrencyChanged);
        }

        // GameManager 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDataChanged.RemoveListener(UpdateAllUI);
        }
    }
}
