using System;
using UnityEngine;

public class FeverSystem
{
    // 피버 설정
    private readonly int _feverThreshold;
    private readonly float _feverDuration;
    private readonly double _feverMultiplier;

    private int _clickCount;
    private float _feverTimer;
    private bool _isFeverActive;

    public int ClickCount => _clickCount;
    public bool IsFeverActive => _isFeverActive;
    public float FeverTimeRemaining => _isFeverActive ? _feverTimer : 0f;
    public double FeverMultiplier => _feverMultiplier;

    public event Action OnFeverStarted;
    public event Action OnFeverEnded;

    public FeverSystem(int feverThreshold = 75, float feverDuration = 10f, double feverMultiplier = 2.5)
    {
        _feverThreshold = feverThreshold;
        _feverDuration = feverDuration;
        _feverMultiplier = feverMultiplier;

        _clickCount = 0;
        _feverTimer = 0f;
        _isFeverActive = false;
    }

    public void AddClick()
    {
        if (_isFeverActive) return; 

        _clickCount++;

        // 피버 발동 체크
        if (_clickCount >= _feverThreshold)
        {
            StartFever();
        }
    }

    private void StartFever()
    {
        _isFeverActive = true;
        _feverTimer = _feverDuration;
        _clickCount = 0;

        OnFeverStarted?.Invoke();
    }

    private void EndFever()
    {
        _isFeverActive = false;
        _clickCount = 0;

        OnFeverEnded?.Invoke();
    }

    public void Update(float deltaTime)
    {
        if (!_isFeverActive) return;

        _feverTimer -= deltaTime;

        if (_feverTimer <= 0)
        {
            EndFever();
        }
    }

    public void Reset()
    {
        _clickCount = 0;
        _feverTimer = 0f;
        _isFeverActive = false;
    }
}