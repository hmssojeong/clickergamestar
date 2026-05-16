using System;
using UnityEngine;

public class FeverSystem
{
    private int _feverThreshold;
    private float _feverDuration;
    private double _feverMultiplier;

    private int _clickCount;
    private float _feverTimer;
    private bool _isFeverActive;

    public int ClickCount => _clickCount;
    public bool IsFeverActive => _isFeverActive;
    public float FeverTimeRemaining => _isFeverActive ? _feverTimer : 0f;
    public int FeverThreshold => _feverThreshold;
    public float FeverDuration => _feverDuration;
    public double FeverMultiplier => _feverMultiplier;

    public event Action OnFeverStarted;
    public event Action OnFeverEnded;
    public event Action<int, int> OnClickCountChanged;
    public event Action<float> OnFeverTimeChanged;

    public FeverSystem(int feverThreshold = 75, float feverDuration = 10f, double feverMultiplier = 3d)
    {
        _feverThreshold = Mathf.Max(1, feverThreshold);
        _feverDuration = Mathf.Max(0.1f, feverDuration);
        _feverMultiplier = Math.Max(1d, feverMultiplier);

        _clickCount = 0;
        _feverTimer = 0f;
        _isFeverActive = false;
    }

    public void AddClick()
    {
        if (_isFeverActive)
        {
            return;
        }

        _clickCount++;
        OnClickCountChanged?.Invoke(_clickCount, _feverThreshold);

        if (_clickCount >= _feverThreshold)
        {
            StartFever();
        }
    }

    public void Update(float deltaTime)
    {
        if (!_isFeverActive)
        {
            return;
        }

        _feverTimer -= deltaTime;
        OnFeverTimeChanged?.Invoke(Mathf.Max(_feverTimer, 0f));

        if (_feverTimer <= 0f)
        {
            EndFever();
        }
    }

    public void Reset()
    {
        _clickCount = 0;
        _feverTimer = 0f;
        _isFeverActive = false;

        OnClickCountChanged?.Invoke(_clickCount, _feverThreshold);
        OnFeverTimeChanged?.Invoke(_feverTimer);
    }

    public void SetThreshold(int threshold)
    {
        _feverThreshold = Mathf.Max(1, threshold);
        OnClickCountChanged?.Invoke(_clickCount, _feverThreshold);
    }

    public void SetDuration(float duration)
    {
        _feverDuration = Mathf.Max(0.1f, duration);
    }

    public void SetMultiplier(double multiplier)
    {
        _feverMultiplier = Math.Max(1d, multiplier);
    }

    private void StartFever()
    {
        _isFeverActive = true;
        _feverTimer = _feverDuration;
        _clickCount = 0;

        OnFeverStarted?.Invoke();
        OnFeverTimeChanged?.Invoke(_feverTimer);
    }

    private void EndFever()
    {
        _isFeverActive = false;
        _feverTimer = 0f;
        _clickCount = 0;

        OnFeverEnded?.Invoke();
        OnClickCountChanged?.Invoke(_clickCount, _feverThreshold);
        OnFeverTimeChanged?.Invoke(_feverTimer);
    }
}
