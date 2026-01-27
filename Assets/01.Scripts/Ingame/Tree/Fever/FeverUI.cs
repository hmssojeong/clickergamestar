using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class FeverUI : MonoBehaviour
{
    [Header("Fever Gauge UI")]
    [SerializeField] private Slider _feverGaugeSlider; // 피버 게이지 슬라이더
    [SerializeField] private Image _gaugeFillImage; // 게이지 채우기 이미지
    [SerializeField] private TextMeshProUGUI _gaugeText; // "50 / 75" 형식
    [SerializeField] private Gradient _gaugeColorGradient; // 게이지 색상 그라데이션

    [Header("Fever Timer UI")]
    [SerializeField] private GameObject _feverTimerPanel; // 피버 타이머 패널
    [SerializeField] private Slider _feverTimerSlider; // 피버 남은 시간 슬라이더
    [SerializeField] private TextMeshProUGUI _feverTimerText; // "12.5s" 형식

    [Header("Fever Effect UI")]
    [SerializeField] private GameObject _feverEffectPanel; // "FEVER TIME!" 텍스트 패널
    [SerializeField] private TextMeshProUGUI _feverEffectText;
    [SerializeField] private Image _screenOverlay; // 화면 전체 오버레이
    [SerializeField] private Color _feverOverlayColor = new Color(1f, 0.5f, 0f, 0.2f); // 주황색

    [Header("Animation Settings")]
    [SerializeField] private float _gaugePulseScale = 1.1f;
    [SerializeField] private float _gaugePulseDuration = 0.3f;

    [Header("Sky Background Settings")]
    [SerializeField] private SpriteRenderer _skyUp;  
    [SerializeField] private SpriteRenderer _skyBottom; 
    [SerializeField] private Color _feverSkyColor = new Color32(0x54, 0x59, 0x7B, 0xFF);
    [SerializeField] private float _colorTransitionDuration = 0.5f; // 색 변화 시간

    private void Start()
    {
        if (FeverManager.Instance != null)
        {
            // 이벤트 구독
            FeverManager.Instance.OnClickCountChanged.AddListener(UpdateFeverGauge);
            FeverManager.Instance.OnFeverStart.AddListener(OnFeverStart);
            FeverManager.Instance.OnFeverEnd.AddListener(OnFeverEnd);
            FeverManager.Instance.OnFeverTimeChanged.AddListener(UpdateFeverTimer);
        }

        // 초기 상태 설정
        if (_feverTimerPanel != null)
            _feverTimerPanel.SetActive(false);

        if (_feverEffectPanel != null)
            _feverEffectPanel.SetActive(false);

        if (_screenOverlay != null)
        {
            _screenOverlay.color = new Color(1, 1, 1, 0); // 투명
        }
    }

    // 피버 게이지 업데이트
    private void UpdateFeverGauge(int currentClicks, int maxClicks)
    {
        if (_feverGaugeSlider != null)
        {
            float progress = (float)currentClicks / maxClicks;
            _feverGaugeSlider.value = progress;

            // 게이지 색상 변경
            if (_gaugeFillImage != null && _gaugeColorGradient != null)
            {
                _gaugeFillImage.color = _gaugeColorGradient.Evaluate(progress);
            }

            // 게이지 텍스트 업데이트
            if (_gaugeText != null)
            {
                _gaugeText.text = $"{currentClicks} / {maxClicks}";
            }

            // 게이지 꽉 차면 펄스 애니메이션
            if (progress >= 1f)
            {
                AnimateGaugeFull();
            }
        }
    }

    // 게이지 꽉 찼을 때 애니메이션
    private void AnimateGaugeFull()
    {
        if (_feverGaugeSlider != null)
        {
            _feverGaugeSlider.transform.DOKill();
            _feverGaugeSlider.transform.DOPunchScale(
                Vector3.one * (_gaugePulseScale - 1f),
                _gaugePulseDuration,
                5,
                0.5f
            );
        }
    }

    // 피버 타이머 업데이트
    private void UpdateFeverTimer(float remainingTime)
    {
        if (_feverTimerSlider != null && FeverManager.Instance != null)
        {
            float progress = FeverManager.Instance.GetFeverTimeProgress();
            _feverTimerSlider.value = progress;
        }

        if (_feverTimerText != null)
        {
            _feverTimerText.text = $"{remainingTime:F1}s";
        }
    }

    // 피버 시작 시 호출
    private void OnFeverStart()
    {
        if (_gaugeText != null)
        {
            _gaugeText.text = "FEVER";
            // 텍스트가 강조되도록 살짝 흔들리거나 색상을 바꾸는 효과를 추가할 수도 있습니다.
            _gaugeText.transform.DOShakePosition(0.5f, 5f);
        }

        // 타이머 패널 표시
        if (_feverTimerPanel != null)
        {
            _feverTimerPanel.SetActive(true);
            _feverTimerPanel.transform.localScale = Vector3.zero;
            _feverTimerPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        // "FEVER TIME!" 텍스트 표시
        if (_feverEffectPanel != null)
        {
            _feverEffectPanel.SetActive(true);
            _feverEffectText.DOFade(0f, 2f).From(1f);
            _feverEffectPanel.transform.DOScale(1.5f, 2f).From(0.5f).SetEase(Ease.OutQuad);

            // 2초 후 페이드 아웃
            DOVirtual.DelayedCall(2f, () =>
            {
                if (_feverEffectPanel != null)
                    _feverEffectPanel.SetActive(false);
            });
        }

        // 화면 오버레이 페이드 인
        if (_screenOverlay != null)
        {
            _screenOverlay.DOColor(_feverOverlayColor, 0.5f);
        }

        if (_skyUp != null)
        {
            _skyUp.DOColor(_feverSkyColor, _colorTransitionDuration);
        }
        if (_skyBottom != null)
        {
            _skyBottom.DOColor(_feverSkyColor, _colorTransitionDuration);
        }
    }

    // 피버 종료 시 호출
    private void OnFeverEnd()
    {
        if (_gaugeText != null && FeverManager.Instance != null)
        {
            // 피버가 끝나면 클릭 수가 0으로 리셋되므로 "0 / 75" 형태로 표시
            _gaugeText.text = $"0 / {FeverManager.Instance.GetClicksNeeded()}";
        }

        // 타이머 패널 숨기기
        if (_feverTimerPanel != null)
        {
            _feverTimerPanel.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => _feverTimerPanel.SetActive(false));
        }

        // 화면 오버레이 페이드 아웃
        if (_screenOverlay != null)
        {
            _screenOverlay.DOColor(new Color(1, 1, 1, 0), 0.5f);
        }

        if (_skyUp != null)
        {
            _skyUp.DOColor(Color.white, _colorTransitionDuration);
        }
        if (_skyBottom != null)
        {
            _skyBottom.DOColor(Color.white, _colorTransitionDuration);
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (FeverManager.Instance != null)
        {
            FeverManager.Instance.OnClickCountChanged.RemoveListener(UpdateFeverGauge);
            FeverManager.Instance.OnFeverStart.RemoveListener(OnFeverStart);
            FeverManager.Instance.OnFeverEnd.RemoveListener(OnFeverEnd);
            FeverManager.Instance.OnFeverTimeChanged.RemoveListener(UpdateFeverTimer);
        }
    }
}