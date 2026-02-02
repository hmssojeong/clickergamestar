using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemUI : MonoBehaviour
{
    public TextMeshProUGUI NameTextUI;
    public TextMeshProUGUI DescriptionTextUI;
    public TextMeshProUGUI LevelTextUI;
    public TextMeshProUGUI CostTextUI;
    public Image IconImage; // 아이콘 표시용 추가
    public Image UpgradeButtonImage;
    public Button UpgradeButton;

    public Sprite CanLevelUpSprite;
    public Sprite NotCanLevelUpSprite;

    public Color AffordableColor = Color.green;
    public Color NotAffordableColor = Color.red;

    private Upgrade _upgrade;
    public EUpgradeType Type => _upgrade != null ? _upgrade.SpecData.Type : EUpgradeType.AppleHarvest;

    private void Start()
    {
        if (UpgradeButton != null)
        {
            UpgradeButton.onClick.AddListener(OnLevelUpButtonClicked);
        }
    }

    public void Refresh(Upgrade upgrade)
    {
        if (upgrade == null)
        {
            return;
        }

        _upgrade = upgrade;

        if (NameTextUI != null)
        {
            NameTextUI.text = upgrade.SpecData.Name;
        }

        if (IconImage != null)
        {
            IconImage.sprite = upgrade.SpecData.Icon;
        }

        if (DescriptionTextUI != null)
        {
            DescriptionTextUI.text = string.Format(upgrade.SpecData.Description, upgrade.Damage);
        }

        if (LevelTextUI != null)
        {
            if (upgrade.IsMaxLevel)
            {
                LevelTextUI.text = $"Lv.{upgrade.Level} (Max)";
            }
            else
            {
                LevelTextUI.text = $"Lv.{upgrade.Level}";
            }
        }

        // 레벨업 가능 여부 체크
        bool canLevelUp = UpgradeManager.Instance != null &&
                         UpgradeManager.Instance.CanLevelUp(upgrade.SpecData.Type);

        // 비용 표시 및 색상 변경
        if (CostTextUI != null)
        {
            if (upgrade.IsMaxLevel)
            {
                CostTextUI.text = "MAX";
                CostTextUI.color = Color.yellow;
            }
            else
            {
                CostTextUI.text = upgrade.Cost.ToString();
                CostTextUI.color = canLevelUp ? AffordableColor : NotAffordableColor;
            }
        }

        // 버튼 상태 업데이트
        if (UpgradeButton != null)
        {
            UpgradeButton.interactable = canLevelUp && !upgrade.IsMaxLevel;
        }

        if (UpgradeButtonImage != null)
        {
            UpgradeButtonImage.sprite = canLevelUp ? CanLevelUpSprite : NotCanLevelUpSprite;
        }
    }

    private void OnLevelUpButtonClicked()
    {
        LevelUp();
    }
    public void LevelUp()
    {
        if (_upgrade == null)
        {
            return;
        }

        if (UpgradeManager.Instance == null)
        {
            return;
        }

        if (UpgradeManager.Instance.TryLevelUp(_upgrade.SpecData.Type))
        {
            var updatedUpgrade = UpgradeManager.Instance.Get(_upgrade.SpecData.Type);
            if (updatedUpgrade != null)
            {
                Refresh(updatedUpgrade);
            }
        }
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (UpgradeButton != null)
        {
            UpgradeButton.onClick.RemoveListener(OnLevelUpButtonClicked);
        }
    }
}