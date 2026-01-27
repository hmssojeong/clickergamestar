using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _statsText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Button _buyButton;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Image _costIcon;

    [Header("Data")]
    private string _itemId;
    private int _currentLevel;
    private int _cost;
    private string _itemName;
    private long _dps; // DPS (Damage Per Second)


    // 아이템 데이터로 UI 초기화
    public void Initialize(string id, string itemName, Sprite icon, int level, long dps, int cost, bool isHired)
    {
        _itemId = id;
        _itemName = itemName;
        _currentLevel = level;
        _dps = dps;
        _cost = cost;

        // UI 업데이트
        if (_icon != null)
            _icon.sprite = icon;

        if (_nameText != null)
            _nameText.text = itemName;

        UpdateStats();
        UpdateButton(isHired);

        // 버튼 이벤트 연결
        if (_buyButton != null)
        {
            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
    }

    //스탯 정보 업데이트
    private void UpdateStats()
    {
        if (_statsText != null)
        {
            // DPS 표시 (B = Billion, M = Million, K = Thousand)
            string dpsText = FormatNumber(_dps);
            _statsText.text = $"{dpsText} DPS";
        }

        if (_levelText != null)
        {
            // 레벨 표시
            if (_currentLevel > 0)
            {
                _levelText.text = $"Lvl {_currentLevel}";
            }
            else
            {
                _levelText.text = "";
            }
        }
    }

    // 버튼 상태 업데이트
    private void UpdateButton(bool isHired)
    {
        if (_buttonText != null)
        {
            if (isHired)
            {
                _buttonText.text = "LVL UP";
            }
            else
            {
                _buttonText.text = "HIRE";
            }
        }

        if (_costText != null)
        {
            // 비용 표시
            _costText.text = FormatNumber(_cost);
        }

        if (_buyButton != null)
        {
            // 구매 가능 여부 확인
            bool canAfford = GameManager.Instance.Apples >= _cost;
            _buyButton.interactable = canAfford;
        }
    }

    // 구매 버튼 클릭
    private void OnBuyButtonClicked()
    {
        if (UpgradeManager.Instance != null)
        {
            // UpgradeManager에게 알림
            UpgradeManager.Instance.PurchaseUpgrade(_itemId);
        }
    }

    /// 숫자를 K, M, B, T 형식으로 변환
    private string FormatNumber(long number)
    {
        if (number >= 1000000000000) // Trillion
            return (number / 1000000000000.0).ToString("0.#") + "T";
        if (number >= 1000000000) // Billion
            return (number / 1000000000.0).ToString("0.#") + "B";
        if (number >= 1000000) // Million
            return (number / 1000000.0).ToString("0.#") + "M";
        if (number >= 1000) // Thousand
            return (number / 1000.0).ToString("0.#") + "K";

        return number.ToString();
    }

    // UI 새로고침 (외부에서 호출)
    public void Refresh(int newLevel, long newDps, int newCost, bool isHired)
    {
        _currentLevel = newLevel;
        _dps = newDps;
        _cost = newCost;

        UpdateStats();
        UpdateButton(isHired);
    }
}