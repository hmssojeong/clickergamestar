using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.Pool;

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

    private IObjectPool<FloatingText> _managedPool;


    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
        
        if (_text == null)
            _text = GetComponent<TextMeshProUGUI>();
    }

    public void SetPool(IObjectPool<FloatingText> pool)
    {
        _managedPool = pool;
    }

    // 플로팅 텍스트 초기화 및 재생
    public void Initialize(string text, Vector3 worldPosition, bool isCritical = false)
    {
        _text.alpha = 1f;

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
        transform.DOKill();
        _rectTransform.DOKill();
        _text.DOKill();

        // 1. 기본 크기 설정 (크리티컬이면 1.5배, 아니면 1배)
        float targetScale = isCritical ? 1.8f : 1.0f;

        transform.localScale = Vector3.zero;

        // 애니메이션 시퀀스 생성 (완료 시점을 정확히 잡기 위함)
        Sequence seq = DOTween.Sequence();

        // 크기 조절
        seq.Join(transform.DOScale(Vector3.one * targetScale, 0.2f)
            .SetEase(isCritical ? Ease.OutBack : Ease.OutQuad));

        // 위로 떠오름
        seq.Join(_rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + _floatHeight * 100f, _floatDuration)
            .SetEase(_floatEase));

        // 페이드 아웃
        seq.Join(_text.DOFade(0f, _floatDuration).SetEase(Ease.InQuad));

        // 5. 핵심: 애니메이션이 끝나면 Destroy 대신 Release 호출
        seq.OnComplete(() =>
        {
            if (_managedPool != null)
                _managedPool.Release(this);
            else
                Destroy(gameObject);
        });
    }

    // 랜덤 방향으로 조금 움직이기
    public void AddRandomOffset()
    {
        float randomX = Random.Range(-150f, 150f);
        float randomY = Random.Range(-20f, 20f);
        _rectTransform.anchoredPosition += new Vector2(randomX, randomY);
    }
}
