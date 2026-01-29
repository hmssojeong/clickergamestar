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

    private Upgrade _upgrade;
    public EUpgradeType Type => _upgrade != null ? _upgrade.SpecData.Type : EUpgradeType.AppleHarvest;

    public void Refresh(Upgrade upgrade)
    {
        _upgrade = upgrade;

        NameTextUI.text = upgrade.SpecData.Name;

        if (IconImage != null) IconImage.sprite = upgrade.SpecData.Icon;

        DescriptionTextUI.text = string.Format(upgrade.SpecData.Description, upgrade.Damage);

        LevelTextUI.text = $"Lv.{upgrade.Level}";
        CostTextUI.text = upgrade.Cost.ToString();

        bool canLevelUp = UpgradeManager.Instance.CanLevelUp(upgrade.SpecData.Type);

        CostTextUI.color = canLevelUp ? Color.green : Color.red;
        UpgradeButtonImage.sprite = canLevelUp ? CanLevelUpSprite : NotCanLevelUpSprite;
        UpgradeButton.interactable = canLevelUp;
    }

    public void LevelUp()
    {
        if (_upgrade == null) return;

        if (UpgradeManager.Instance.TryLevelUp(_upgrade.SpecData.Type))
        {
            // 리프레시를 통해 UI 갱신
            Refresh(_upgrade);
            // todo: 이펙트, 애니메이션, 트위닝
        }
    }
}
