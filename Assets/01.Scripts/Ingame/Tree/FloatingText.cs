using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _floatDuration = 1f;
    [SerializeField] private float _floatHeight = 2f;
    [SerializeField] private Ease _floatEase = Ease.OutQuad;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _criticalColor = Color.red;

    private Canvas _canvas;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
        
        if (_text == null)
            _text = GetComponent<TextMeshProUGUI>();
    }

    // 플로팅 텍스트 초기화 및 재생
    public void Initialize(string text, Vector3 worldPosition, bool isCritical = false)
    {
        _text.color = isCritical ? _criticalColor : _normalColor;
        _text.text = text;

        // 월드 좌표를 스크린 좌표로 변환
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
            Camera.main,
            worldPosition
        );

        // 스크린 좌표를 캔버스 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            screenPosition,
            _canvas.worldCamera,
            out Vector2 localPoint
        );

        _rectTransform.localPosition = localPoint;

        // 애니메이션 재생
        PlayAnimation(isCritical);
    }

    // 떠오르는 애니메이션
    private void PlayAnimation(bool isCritical)
    {
        if (isCritical)
        {
            // 크리티컬이면 1.5배 더 크게 커졌다가 돌아옴
            transform.DOScale(Vector3.one * 1.5f, 0.1f).SetLoops(2, LoopType.Yoyo);
        }

        // 위로 떠오름
        _rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + _floatHeight * 100f, _floatDuration)
            .SetEase(_floatEase);

        // 페이드 아웃
        _text.DOFade(0f, _floatDuration)
            .SetEase(Ease.InQuad);

        // 스케일 애니메이션 (선택)
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.2f)
            .SetEase(Ease.OutBack);

        // 애니메이션 완료 후 삭제
        Destroy(gameObject, _floatDuration);
    }

    // 랜덤 방향으로 조금 움직이기
    public void AddRandomOffset()
    {
        float randomX = Random.Range(-50f, 50f);
        _rectTransform.anchoredPosition += new Vector2(randomX, 0);
    }
}
