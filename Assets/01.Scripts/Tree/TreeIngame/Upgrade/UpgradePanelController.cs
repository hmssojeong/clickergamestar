using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UpgradePanelController : MonoBehaviour
{
    [Header("패널 설정")]
    public RectTransform panelRectTransform;
    public float collapsedHeight = 600f;
    public float expandedHeight = 1294f;
    public float animationDuration = 0.3f;

    [Header("UI 참조")]
    public Button expandButton;
    public TextMeshProUGUI expandButtonText;
    public ScrollRect scrollRect;
    public Transform upgradeItemsContainer;

    [Header("업그레이드 아이템 프리팹")]
    public GameObject upgradeItemPrefab;

    private bool isExpanded = false;
    private Coroutine animationCoroutine;

    private List<UpgradeItemUI> _itemUIList = new List<UpgradeItemUI>();


    void Start()
    {
        if (expandButton != null)
        {
            expandButton.onClick.AddListener(TogglePanel);
        }

        SetPanelHeight(collapsedHeight, immediate: true);
        UpdateExpandButtonText();

        CreateUpgradeItems();

        UpgradeManager.OnDataChanged += RefreshAllItems;
    }

    private void OnDestroy()
    {
        UpgradeManager.OnDataChanged -= RefreshAllItems;
    }

    public void TogglePanel()
    {
        isExpanded = !isExpanded;
        float targetHeight = isExpanded ? expandedHeight : collapsedHeight;

        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimatePanelHeight(targetHeight));
        UpdateExpandButtonText();
    }

    IEnumerator AnimatePanelHeight(float targetHeight)
    {
        float startHeight = panelRectTransform.sizeDelta.y;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            t = 1f - Mathf.Pow(1f - t, 3f); // Ease Out Cubic

            float currentHeight = Mathf.Lerp(startHeight, targetHeight, t);
            SetPanelHeight(currentHeight);
            yield return null;
        }

        SetPanelHeight(targetHeight);
        animationCoroutine = null;
    }

    void SetPanelHeight(float height, bool immediate = false)
    {
        if (panelRectTransform != null)
        {
            panelRectTransform.sizeDelta = new Vector2(panelRectTransform.sizeDelta.x, height);
        }
    }

    void UpdateExpandButtonText()
    {
        if (expandButtonText != null)
        {
            expandButtonText.text = isExpanded ? "▼" : "▲";
        }
    }

    void CreateUpgradeItems()
    {
        if (upgradeItemPrefab == null || upgradeItemsContainer == null) return;
        if (UpgradeManager.Instance == null) return;

        // 기존 아이템 삭제 및 리스트 초기화
        foreach (Transform child in upgradeItemsContainer) Destroy(child.gameObject);
        _itemUIList.Clear();

        foreach (var upgrade in UpgradeManager.Instance.AllUpgrades.Values)
        {
            GameObject itemObj = Instantiate(upgradeItemPrefab, upgradeItemsContainer);
            UpgradeItemUI itemUI = itemObj.GetComponent<UpgradeItemUI>();

            if (itemUI != null)
            {
                itemUI.Refresh(upgrade);
                _itemUIList.Add(itemUI);
            }
        }
    }

    public void RefreshAllItems()
    {
        foreach (var itemUI in _itemUIList)
        {
            if (itemUI != null)
            {
                var upgradeData = UpgradeManager.Instance.Get(itemUI.Type);
                if (upgradeData != null)
                {
                    itemUI.Refresh(upgradeData); 
                }
            }
        }
    }
}
