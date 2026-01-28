using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 나무 클릭 처리 예제 스크립트
/// 이 스크립트를 나무 Button에 추가하거나, 기존 스크립트를 수정하세요
/// </summary>
public class TreeClickHandler : MonoBehaviour
{
    [Header("UI 참조")]
    public Text appleScoreText;        // 사과 점수 텍스트
    public Text clickCountText;        // 클릭 카운트 텍스트
    public GameObject feverPanel;      // 피버 타임 패널
    public Text feverTimerText;        // 피버 타이머 텍스트
    
    private Button treeButton;
    
    void Start()
    {
        // 나무 버튼 가져오기
        treeButton = GetComponent<Button>();
        if (treeButton != null)
        {
            treeButton.onClick.AddListener(OnTreeClicked);
        }
        
        // GameManager 이벤트 구독
        GameManager.Instance.OnAppleChanged.AddListener(UpdateAppleUI);
        GameManager.Instance.OnFeverStartEvent.AddListener(OnFeverStart);
        GameManager.Instance.OnFeverEndEvent.AddListener(OnFeverEnd);
        
        // 초기 UI 업데이트
        UpdateAppleUI(GameManager.Instance.Apples);
        UpdateClickCountUI();
        
        if (feverPanel != null)
        {
            feverPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        // 클릭 카운트 UI 업데이트
        UpdateClickCountUI();
        
        // 피버 타이머 UI 업데이트
        if (GameManager.Instance.isFeverActive && feverTimerText != null)
        {
            float remainingTime = GameManager.Instance.GetFeverRemainingTime();
            feverTimerText.text = $"🔥 FEVER TIME! {remainingTime:F1}초 🔥";
        }
    }
    
    /// <summary>
    /// 나무 클릭 시 호출
    /// </summary>
    void OnTreeClicked()
    {
        GameManager.Instance.OnTreeClick();
        
        // 클릭 애니메이션 (옵션)
        StartCoroutine(TreeClickAnimation());
    }
    
    /// <summary>
    /// 사과 점수 UI 업데이트
    /// </summary>
    void UpdateAppleUI(double apples)
    {
        if (appleScoreText != null)
        {
            appleScoreText.text = $"🍎 {FormatNumber(apples)}";
        }
    }
    
    /// <summary>
    /// 클릭 카운트 UI 업데이트
    /// </summary>
    void UpdateClickCountUI()
    {
        if (clickCountText != null)
        {
            int currentClicks = GameManager.Instance.clickCount;
            int threshold = GameManager.Instance.feverThreshold;
            clickCountText.text = $"클릭: {currentClicks}/{threshold}";
        }
    }
    
    /// <summary>
    /// 피버 시작 시 호출
    /// </summary>
    void OnFeverStart()
    {
        if (feverPanel != null)
        {
            feverPanel.SetActive(true);
        }
        
        Debug.Log("🔥 FEVER TIME 시작! 🔥");
    }
    
    /// <summary>
    /// 피버 종료 시 호출
    /// </summary>
    void OnFeverEnd()
    {
        if (feverPanel != null)
        {
            feverPanel.SetActive(false);
        }
        
        Debug.Log("피버 타임 종료!");
    }
    
    /// <summary>
    /// 나무 클릭 애니메이션 (옵션)
    /// </summary>
    System.Collections.IEnumerator TreeClickAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 0.95f;
        
        float duration = 0.1f;
        float elapsed = 0f;
        
        // 축소
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }
        
        elapsed = 0f;
        
        // 확대
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// 숫자 포맷팅
    /// </summary>
    string FormatNumber(double number)
    {
        if (number >= 1000000000000)
            return (number / 1000000000000).ToString("0.##") + "T";
        else if (number >= 1000000000)
            return (number / 1000000000).ToString("0.##") + "B";
        else if (number >= 1000000)
            return (number / 1000000).ToString("0.##") + "M";
        else if (number >= 1000)
            return (number / 1000).ToString("0.##") + "K";
        else
            return number.ToString("0");
    }
}
