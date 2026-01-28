using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// 업그레이드 패널의 UI를 제어하는 컨트롤러
/// </summary>
public class UpgradePanelController : MonoBehaviour
{
    [Header("패널 설정")]
    public RectTransform panelRectTransform;
    public float collapsedHeight = 600f;  // 축소 시 높이 (2개 보임)
    public float expandedHeight = 1200f;  // 확장 시 높이 (5개 보임)
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
        // 확장 버튼 이벤트 연결
        if (expandButton != null)
        {
            expandButton.onClick.AddListener(TogglePanel);
        }
        
        // 초기 상태 설정
        SetPanelHeight(collapsedHeight, immediate: true);
        UpdateExpandButtonText();
        
        // 업그레이드 아이템 생성
        CreateUpgradeItems();
    }
    
    /// <summary>
    /// 패널 확장/축소 토글
    /// </summary>
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
    
    /// <summary>
    /// 패널 높이 애니메이션
    /// </summary>
    IEnumerator AnimatePanelHeight(float targetHeight)
    {
        float startHeight = panelRectTransform.sizeDelta.y;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Ease Out Cubic 곡선
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            float currentHeight = Mathf.Lerp(startHeight, targetHeight, t);
            SetPanelHeight(currentHeight);
            
            yield return null;
        }
        
        SetPanelHeight(targetHeight);
        animationCoroutine = null;
    }
    
    /// <summary>
    /// 패널 높이 설정
    /// </summary>
    void SetPanelHeight(float height, bool immediate = false)
    {
        if (panelRectTransform != null)
        {
            Vector2 sizeDelta = panelRectTransform.sizeDelta;
            sizeDelta.y = height;
            panelRectTransform.sizeDelta = sizeDelta;
        }
    }
    
    /// <summary>
    /// 확장 버튼 텍스트 업데이트
    /// </summary>
    void UpdateExpandButtonText()
    {
        if (expandButtonText != null)
        {
            expandButtonText.text = isExpanded ? "▼" : "▲";
        }
    }
    
    /// <summary>
    /// 업그레이드 아이템들 생성
    /// </summary>
    void CreateUpgradeItems()
    {
        if (upgradeItemPrefab == null || upgradeItemsContainer == null)
        {
            Debug.LogError("업그레이드 아이템 프리팹 또는 컨테이너가 설정되지 않았습니다!");
            return;
        }
        
        // 기존 아이템 삭제
        foreach (Transform child in upgradeItemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 5가지 업그레이드 아이템 생성
        foreach (var upgradeData in UpgradeManager.Instance.allUpgrades)
        {
            GameObject itemObj = Instantiate(upgradeItemPrefab, upgradeItemsContainer);
            UpgradeItemUI itemUI = itemObj.GetComponent<UpgradeItemUI>();
            
            if (itemUI != null)
            {
                itemUI.Initialize(upgradeData);
            }
        }
    }
    
    /// <summary>
    /// 모든 업그레이드 아이템 UI 갱신
    /// </summary>
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
        // 매 프레임마다 UI 갱신 (점수 변동 반영)
        RefreshAllItems();
    }
}
