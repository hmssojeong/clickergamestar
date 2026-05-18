using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    [Header("Fever Settings")]
    [SerializeField] private int _feverThreshold = 75;
    [SerializeField] private float _feverDuration = 10f;
    [SerializeField] private float _feverMultiplier = 3f;

    [Header("Domain Systems")]
    private FeverSystem _feverSystem;
    private CriticalSystem _criticalSystem;
    private int _squirrelCount;

    [Header("Upgrade Stats")]
    public double ManualDamage { get; private set; } = 1.0d;
    public double AutoDamage { get; private set; } = 1.0d;

    [Header("Game Progress")]
    public double TotalApplesCollected { get; private set; } = 0.0d;

    public event Action OnFeverStarted;
    public event Action OnFeverEnded;
    public event Action<int, int> OnFeverClickCountChanged;
    public event Action<float> OnFeverTimeChanged;
    public event Action OnTreeRespawned;
    public event Action OnDataChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystems();
            return;
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        _feverSystem?.Update(Time.deltaTime);
    }

    public double ProcessTreeClick(ClickInfo clickInfo)
    {
        if (clickInfo.Type == EClickType.Manual)
        {
            _feverSystem.AddClick();
        }

        var (finalDamage, isCritical) = DamageCalculator.CalculateFinalDamage(
            clickInfo.Type,
            ManualDamage,
            AutoDamage,
            _criticalSystem.CriticalChance,
            _criticalSystem.CriticalMultiplier,
            _feverSystem.IsFeverActive,
            _feverSystem.FeverMultiplier);

        AddApples(finalDamage);

        if (FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ShowDamage(clickInfo.Position, finalDamage, isCritical);
        }

        return finalDamage;
    }

    public void AddApples(double amount)
    {
        if (CurrencyManager.Instance == null)
        {
            return;
        }

        CurrencyManager.Instance.Add(ECurrencyType.Apple, amount);
        TotalApplesCollected += amount;
        OnDataChanged?.Invoke();
    }

    public void OnTreeRespawn()
    {
        OnTreeRespawned?.Invoke();
    }

    public void SetManualDamage(double damage)
    {
        ManualDamage = NormalizeLegacyWholeNumber(damage);
        OnDataChanged?.Invoke();
    }

    public void SetAutoDamage(double damage)
    {
        AutoDamage = NormalizeLegacyWholeNumber(damage);
        OnDataChanged?.Invoke();
    }

    public void SetSquirrelCount(int count)
    {
        _squirrelCount = Mathf.Max(0, count);
        OnDataChanged?.Invoke();
    }

    public bool CanUseAutoClicker(int requiredSquirrelCount)
    {
        if (requiredSquirrelCount <= 0)
        {
            return true;
        }

        return _squirrelCount >= requiredSquirrelCount;
    }

    public void SetCriticalChance(double chance)
    {
        _criticalSystem.SetCriticalChance(chance);
        OnDataChanged?.Invoke();
    }

    public void SetCriticalMultiplier(double multiplier)
    {
        _criticalSystem.SetCriticalMultiplier(NormalizeLegacyWholeNumber(multiplier));
        OnDataChanged?.Invoke();
    }

    public void SetFeverMultiplier(double multiplier)
    {
        _feverMultiplier = (float)Math.Max(1d, NormalizeLegacyWholeNumber(multiplier));
        _feverSystem.SetMultiplier(_feverMultiplier);
        OnDataChanged?.Invoke();
    }

    public void ConfigureFever(int threshold, float duration, double multiplier)
    {
        _feverThreshold = Mathf.Max(1, threshold);
        _feverDuration = Mathf.Max(0.1f, duration);
        _feverMultiplier = (float)Math.Max(1d, NormalizeLegacyWholeNumber(multiplier));

        _feverSystem.SetThreshold(_feverThreshold);
        _feverSystem.SetDuration(_feverDuration);
        _feverSystem.SetMultiplier(_feverMultiplier);
        OnDataChanged?.Invoke();
    }

    public bool IsFeverActive() => _feverSystem.IsFeverActive;
    public float GetFeverTimeRemaining() => _feverSystem.FeverTimeRemaining;
    public float GetFeverTimeProgress() => _feverSystem.FeverDuration <= 0f
        ? 0f
        : _feverSystem.FeverTimeRemaining / _feverSystem.FeverDuration;
    public int GetFeverClickCount() => _feverSystem.ClickCount;
    public int GetFeverClickThreshold() => _feverSystem.FeverThreshold;
    public double GetFeverMultiplier() => _feverSystem.FeverMultiplier;
    public double GetCriticalChance() => _criticalSystem.CriticalChance;
    public double GetCriticalMultiplier() => _criticalSystem.CriticalMultiplier;
    public int GetSquirrelCount() => _squirrelCount;

    public void ResetGame()
    {
        TotalApplesCollected = 0d;
        ManualDamage = 1.0d;
        AutoDamage = 1.0d;

        _feverSystem.Reset();
        _criticalSystem = new CriticalSystem(0.1d, 2.0d);
        _squirrelCount = 0;

        OnDataChanged?.Invoke();
    }

    private void InitializeSystems()
    {
        _feverSystem = new FeverSystem(_feverThreshold, _feverDuration, _feverMultiplier);
        _criticalSystem = new CriticalSystem(0.1d, 2.0d);
        _squirrelCount = 0;

        BindFeverEvents();
    }

    private void BindFeverEvents()
    {
        _feverSystem.OnFeverStarted += HandleFeverStarted;
        _feverSystem.OnFeverEnded += HandleFeverEnded;
        _feverSystem.OnClickCountChanged += HandleFeverClickCountChanged;
        _feverSystem.OnFeverTimeChanged += HandleFeverTimeChanged;
    }

    private void HandleFeverStarted()
    {
        OnFeverStarted?.Invoke();
    }

    private void HandleFeverEnded()
    {
        OnFeverEnded?.Invoke();
    }

    private void HandleFeverClickCountChanged(int currentClicks, int maxClicks)
    {
        OnFeverClickCountChanged?.Invoke(currentClicks, maxClicks);
    }

    private void HandleFeverTimeChanged(float remainingTime)
    {
        OnFeverTimeChanged?.Invoke(remainingTime);
    }

    private void OnApplicationQuit()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame().Forget();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame().Forget();
        }
    }

    private static double NormalizeLegacyWholeNumber(double value)
    {
        return Math.Round(value);
    }
}
