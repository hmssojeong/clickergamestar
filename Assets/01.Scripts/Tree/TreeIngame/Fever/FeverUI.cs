using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeverUI : MonoBehaviour
{
    [Header("Fever Gauge UI")]
    [SerializeField] private Slider _feverGaugeSlider;
    [SerializeField] private Image _gaugeFillImage;
    [SerializeField] private TextMeshProUGUI _gaugeText;
    [SerializeField] private Gradient _gaugeColorGradient;

    [Header("Fever Timer UI")]
    [SerializeField] private GameObject _feverTimerPanel;
    [SerializeField] private Slider _feverTimerSlider;
    [SerializeField] private TextMeshProUGUI _feverTimerText;

    [Header("Fever Effect UI")]
    [SerializeField] private GameObject _feverEffectPanel;
    [SerializeField] private TextMeshProUGUI _feverEffectText;
    [SerializeField] private Image _screenOverlay;
    [SerializeField] private Color _feverOverlayColor = new Color(1f, 0.5f, 0f, 0.2f);

    [Header("Animation Settings")]
    [SerializeField] private float _gaugePulseScale = 1.1f;
    [SerializeField] private float _gaugePulseDuration = 0.3f;

    [Header("Sky Background Settings")]
    [SerializeField] private SpriteRenderer _skyUp;
    [SerializeField] private SpriteRenderer _skyBottom;
    [SerializeField] private Color _feverSkyColor = new Color32(0x54, 0x59, 0x7B, 0xFF);
    [SerializeField] private float _colorTransitionDuration = 0.5f;

    [Header("Rain Particle Settings")]
    [SerializeField] private ParticleSystem _rainFallParticle;
    [SerializeField] private ParticleSystem _rainMistParticle;
    [SerializeField] private GameObject _fallingLeavesObject;
    [SerializeField] private float _fadeDuration = 0.5f;

    [Header("Sound Settings")]
    [SerializeField] private AudioSource _feverAudioSource;
    [SerializeField] private AudioClip _feverBGMClip;
    [SerializeField] private AudioClip _feverStartClip;
    [SerializeField] private float _soundFadeDuration = 0.5f;

    private GameplayManager _gameplayManager;

    private void Start()
    {
        InitializeVisualState();
        SubscribeToGameplayManager();
        SyncFromGameplayState();
    }

    private void OnDestroy()
    {
        UnsubscribeFromGameplayManager();
    }

    private void InitializeVisualState()
    {
        if (_rainFallParticle != null)
        {
            _rainFallParticle.Stop();
            SetParticleAlpha(_rainFallParticle, 0f);
        }

        if (_rainMistParticle != null)
        {
            _rainMistParticle.Stop();
            SetParticleAlpha(_rainMistParticle, 0f);
        }

        if (_feverTimerPanel != null)
        {
            _feverTimerPanel.SetActive(false);
        }

        if (_feverEffectPanel != null)
        {
            _feverEffectPanel.SetActive(false);
        }

        if (_screenOverlay != null)
        {
            _screenOverlay.color = new Color(1f, 1f, 1f, 0f);
        }

        if (_fallingLeavesObject != null)
        {
            _fallingLeavesObject.SetActive(false);
        }
    }

    private void SubscribeToGameplayManager()
    {
        _gameplayManager = GameplayManager.Instance;
        if (_gameplayManager == null)
        {
            return;
        }

        _gameplayManager.OnFeverClickCountChanged += UpdateFeverGauge;
        _gameplayManager.OnFeverStarted += OnFeverStart;
        _gameplayManager.OnFeverEnded += OnFeverEnd;
        _gameplayManager.OnFeverTimeChanged += UpdateFeverTimer;
    }

    private void UnsubscribeFromGameplayManager()
    {
        if (_gameplayManager == null)
        {
            return;
        }

        _gameplayManager.OnFeverClickCountChanged -= UpdateFeverGauge;
        _gameplayManager.OnFeverStarted -= OnFeverStart;
        _gameplayManager.OnFeverEnded -= OnFeverEnd;
        _gameplayManager.OnFeverTimeChanged -= UpdateFeverTimer;
    }

    private void SyncFromGameplayState()
    {
        if (_gameplayManager == null)
        {
            return;
        }

        UpdateFeverGauge(
            _gameplayManager.GetFeverClickCount(),
            _gameplayManager.GetFeverClickThreshold());

        if (_gameplayManager.IsFeverActive())
        {
            if (_feverTimerPanel != null)
            {
                _feverTimerPanel.SetActive(true);
                _feverTimerPanel.transform.localScale = Vector3.one;
            }

            UpdateFeverTimer(_gameplayManager.GetFeverTimeRemaining());
            if (_gaugeText != null)
            {
                _gaugeText.text = "FEVER";
            }
        }
        else
        {
            ResetGaugeDisplay();
        }
    }

    private void UpdateFeverGauge(int currentClicks, int maxClicks)
    {
        if (_feverGaugeSlider != null)
        {
            float progress = maxClicks <= 0 ? 0f : (float)currentClicks / maxClicks;
            _feverGaugeSlider.value = progress;

            if (_gaugeFillImage != null)
            {
                _gaugeFillImage.color = _gaugeColorGradient.Evaluate(progress);
            }

            if (_gaugeText != null && (_gameplayManager == null || !_gameplayManager.IsFeverActive()))
            {
                _gaugeText.text = $"수동 클릭: {currentClicks} / {maxClicks}";
            }

            if (progress >= 1f)
            {
                AnimateGaugeFull();
            }
        }
    }

    private void AnimateGaugeFull()
    {
        if (_feverGaugeSlider == null)
        {
            return;
        }

        _feverGaugeSlider.transform.DOKill();
        _feverGaugeSlider.transform.DOPunchScale(
            Vector3.one * (_gaugePulseScale - 1f),
            _gaugePulseDuration,
            5,
            0.5f);
    }

    private void UpdateFeverTimer(float remainingTime)
    {
        if (_feverTimerSlider != null && _gameplayManager != null)
        {
            _feverTimerSlider.value = _gameplayManager.GetFeverTimeProgress();
        }

        if (_feverTimerText != null)
        {
            _feverTimerText.text = $"{remainingTime:F1}s";
        }
    }

    private void OnFeverStart()
    {
        if (_feverAudioSource != null && _feverStartClip != null)
        {
            _feverAudioSource.PlayOneShot(_feverStartClip);
        }

        if (_feverAudioSource != null && _feverBGMClip != null)
        {
            _feverAudioSource.clip = _feverBGMClip;
            _feverAudioSource.loop = true;
            _feverAudioSource.volume = 0f;
            _feverAudioSource.Play();
            _feverAudioSource.DOFade(1f, _soundFadeDuration);
        }

        if (_feverGaugeSlider != null)
        {
            _feverGaugeSlider.value = 1f;
        }

        if (_gaugeText != null)
        {
            _gaugeText.text = "FEVER";
            _gaugeText.transform.DOShakePosition(0.5f, 5f);
        }

        if (_feverTimerPanel != null)
        {
            _feverTimerPanel.SetActive(true);
            _feverTimerPanel.transform.localScale = Vector3.zero;
            _feverTimerPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        if (_feverEffectPanel != null)
        {
            _feverEffectPanel.SetActive(true);

            if (_feverEffectText != null)
            {
                _feverEffectText.DOFade(0f, 2f).From(1f);
            }

            _feverEffectPanel.transform.DOScale(1.5f, 2f).From(0.5f).SetEase(Ease.OutQuad);
            DOVirtual.DelayedCall(2f, () =>
            {
                if (_feverEffectPanel != null)
                {
                    _feverEffectPanel.SetActive(false);
                }
            });
        }

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

        if (_rainFallParticle != null)
        {
            _rainFallParticle.Play();
            FadeParticle(_rainFallParticle, 1f);
        }

        if (_rainMistParticle != null)
        {
            _rainMistParticle.Play();
            FadeParticle(_rainMistParticle, 1f);
        }

        if (_fallingLeavesObject != null)
        {
            _fallingLeavesObject.SetActive(true);
        }
    }

    private void OnFeverEnd()
    {
        if (_feverAudioSource != null)
        {
            _feverAudioSource.DOFade(0f, _soundFadeDuration).OnComplete(() =>
            {
                _feverAudioSource.Stop();
                _feverAudioSource.clip = null;
            });
        }

        ResetGaugeDisplay();

        if (_feverTimerPanel != null)
        {
            _feverTimerPanel.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => _feverTimerPanel.SetActive(false));
        }

        if (_screenOverlay != null)
        {
            _screenOverlay.DOColor(new Color(1f, 1f, 1f, 0f), 0.5f);
        }

        if (_skyUp != null)
        {
            _skyUp.DOColor(Color.white, _colorTransitionDuration);
        }

        if (_skyBottom != null)
        {
            _skyBottom.DOColor(Color.white, _colorTransitionDuration);
        }

        FadeParticle(_rainFallParticle, 0f, true);
        FadeParticle(_rainMistParticle, 0f, true);

        if (_fallingLeavesObject != null)
        {
            _fallingLeavesObject.SetActive(false);
        }
    }

    private void ResetGaugeDisplay()
    {
        if (_gameplayManager == null)
        {
            return;
        }

        if (_feverGaugeSlider != null)
        {
            _feverGaugeSlider.value = 0f;
        }

        if (_gaugeFillImage != null)
        {
            _gaugeFillImage.color = _gaugeColorGradient.Evaluate(0f);
        }

        if (_gaugeText != null)
        {
            _gaugeText.text = $"수동 클릭: 0 / {_gameplayManager.GetFeverClickThreshold()}";
        }

        if (_feverTimerSlider != null)
        {
            _feverTimerSlider.value = 0f;
        }

        if (_feverTimerText != null)
        {
            _feverTimerText.text = "0.0s";
        }
    }

    private void FadeParticle(ParticleSystem particleSystem, float targetAlpha, bool stopOnComplete = false)
    {
        if (particleSystem == null)
        {
            return;
        }

        var main = particleSystem.main;
        Color color = main.startColor.color;

        DOTween.To(
            () => color.a,
            alpha =>
            {
                color.a = alpha;
                main.startColor = color;
            },
            targetAlpha,
            _fadeDuration).OnComplete(() =>
            {
                if (stopOnComplete && targetAlpha <= 0f)
                {
                    particleSystem.Stop();
                }
            });
    }

    private void SetParticleAlpha(ParticleSystem particleSystem, float alpha)
    {
        if (particleSystem == null)
        {
            return;
        }

        var main = particleSystem.main;
        Color color = main.startColor.color;
        color.a = alpha;
        main.startColor = color;
    }
}
