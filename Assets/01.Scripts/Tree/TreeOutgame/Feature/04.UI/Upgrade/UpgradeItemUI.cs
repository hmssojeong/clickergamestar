using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemUI : MonoBehaviour
{
    public TextMeshProUGUI NameTextUI;
    public TextMeshProUGUI DescriptionTextUI;
    public TextMeshProUGUI LevelTextUI;
    public TextMeshProUGUI CostTextUI;
    public Image IconImage;
    public Image UpgradeButtonImage;
    public Button UpgradeButton;

    public Sprite CanLevelUpSprite;
    public Sprite NotCanLevelUpSprite;

    public Color AffordableColor = Color.green;
    public Color NotAffordableColor = Color.red;

    private IReadonlyUpgrade _upgrade;
    public EUpgradeType Type => _upgrade != null ? _upgrade.Type : EUpgradeType.AppleHarvest;

    private void Start()
    {
        if (UpgradeButton != null)
        {
            UpgradeButton.onClick.AddListener(OnLevelUpButtonClicked);
        }
    }

    public void Refresh(IReadonlyUpgrade upgrade)
    {
        if (upgrade == null)
        {
            return;
        }

        _upgrade = upgrade;

        if (NameTextUI != null)
        {
            NameTextUI.text = upgrade.Name;
        }

        if (IconImage != null)
        {
            IconImage.sprite = upgrade.Icon;
        }

        if (DescriptionTextUI != null)
        {
            DescriptionTextUI.text = string.Format(upgrade.Description, upgrade.Damage.ToString("N0"));
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

        bool canLevelUp = UpgradeManager.Instance != null &&
                         UpgradeManager.Instance.CanLevelUp(upgrade.Type);

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

        if (UpgradeManager.Instance.TryLevelUp(_upgrade.Type))
        {
            var updatedUpgrade = UpgradeManager.Instance.Get(_upgrade.Type);
            if (updatedUpgrade != null)
            {
                Refresh(updatedUpgrade);
            }
        }
    }

    private void OnDestroy()
    {
        if (UpgradeButton != null)
        {
            UpgradeButton.onClick.RemoveListener(OnLevelUpButtonClicked);
        }
    }
}
