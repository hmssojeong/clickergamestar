using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UpgradePanelController : MonoBehaviour
{
    [Header("패널 설정")]
    public RectTransform panelRectTransform;
    public float collapsedHeight = 600f;
    public float expandedHeight = 1200f;
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

    void Start()
    {
        if (expandButton != null)
        {
            expandButton.onClick.AddListener(TogglePanel);
        }

        SetPanelHeight(collapsedHeight, immediate: true);
        UpdateExpandButtonText();

        CreateUpgradeItems();
    }

    public void TogglePanel()
    {
        isExpanded = !isExpanded;

        float targetHeight = isExpanded ? expandedHeight : collapsedHeight;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

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
            float t = elapsed / animationDuration;

            t = 1f - Mathf.Pow(1f - t, 3f);

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
            Vector2 sizeDelta = panelRectTransform.sizeDelta;
            sizeDelta.y = height;
            panelRectTransform.sizeDelta = sizeDelta;
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
        if (upgradeItemPrefab == null || upgradeItemsContainer == null)
        {
            Debug.LogError("업그레이드 아이템 프리팹 또는 컨테이너가 설정되지 않았습니다!");
            return;
        }

        if (UpgradeManager.Instance == null)
        {
            Debug.LogError("UpgradeManager 인스턴스가 없습니다!");
            return;
        }

        // 기존 아이템 삭제
        foreach (Transform child in upgradeItemsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var upgrade in UpgradeManager.Instance.AllUpgrades)
        {
            GameObject itemObj = Instantiate(upgradeItemPrefab, upgradeItemsContainer);
            UpgradeItemUI itemUI = itemObj.GetComponent<UpgradeItemUI>();

            if (itemUI != null)
            {
                itemUI.Initialize(upgrade);
            }
        }
    }

    [ContextMenu("Preview Upgrade Items")]
    void PreviewUpgradeItemsInEditor()
    {
        if (upgradeItemPrefab == null || upgradeItemsContainer == null)
        {
            Debug.LogError("프리팹이나 컨테이너가 설정되지 않았습니다!");
            return;
        }

        ClearPreviewItems();

        string[] sampleNames = { "사과 수확 강화", "다람쥐 고용", "황금 사과 행운", "피버 타임 마스터", "슈퍼 크리티컬" };

        for (int i = 0; i < 5; i++)
        {
#if UNITY_EDITOR
            GameObject itemObj = UnityEditor.PrefabUtility.InstantiatePrefab(upgradeItemPrefab, upgradeItemsContainer) as GameObject;
#else
            GameObject itemObj = Instantiate(upgradeItemPrefab, upgradeItemsContainer);
#endif

            itemObj.name = $"UpgradeItem_{i + 1}_{sampleNames[i]}";
        }

        Debug.Log("에디터 미리보기 아이템 생성 완료!");
    }

    [ContextMenu("Clear Preview Items")]
    void ClearPreviewItems()
    {
        if (upgradeItemsContainer == null) return;


        while (upgradeItemsContainer.childCount > 0)
        {
            DestroyImmediate(upgradeItemsContainer.GetChild(0).gameObject);
        }

        foreach (Transform child in upgradeItemsContainer)
        {
            Destroy(child.gameObject);
        }

    }

    public void RefreshAllItems()
    {
        foreach (Transform child in upgradeItemsContainer)
        {
            UpgradeItemUI itemUI = child.GetComponent<UpgradeItemUI>();
            if (itemUI != null)
            {
                itemUI.UpdateUI();
            }
        }
    }

    void Update()
    {
        RefreshAllItems();
    }
}