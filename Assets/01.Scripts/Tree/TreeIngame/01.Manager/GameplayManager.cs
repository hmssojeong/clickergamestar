using DG.Tweening.Core.Easing;
using System;
using UnityEngine;
using UnityEngine.Events;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    [Header("Domain Systems")]
    private FeverSystem _feverSystem;
    private CriticalSystem _criticalSystem;
    private AutoHarvestSystem _autoHarvestSystem;

    [Header("Upgrade Stats")]
    public double ManualDamage { get; private set; } = 1.0;
    public double AutoDamage { get; private set; } = 1.0;

    [Header("Game Progress")]
    public double TotalApplesCollected { get; private set; } = 0.0;

    [Header("Events")]
    public UnityEvent OnFeverStarted;
    public UnityEvent OnFeverEnded;
    public UnityEvent OnTreeRespawned;
    public UnityEvent OnDataChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystems();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSystems()
    {
        // 도메인 시스템 초기화
        _feverSystem = new FeverSystem(75, 10f, 2.5);
        _criticalSystem = new CriticalSystem(0.1, 2.0);
        _autoHarvestSystem = new AutoHarvestSystem(50.0);

        // 이벤트 구독
        _feverSystem.OnFeverStarted += () => OnFeverStarted?.Invoke();
        _feverSystem.OnFeverEnded += () => OnFeverEnded?.Invoke();

        Debug.Log("[GameplayManager] Systems initialized");
    }

    private void Start()
    {
        // 자동 수확 시작 (1초마다)
        InvokeRepeating(nameof(ProcessAutoHarvest), 1f, 1f);
    }

    private void Update()
    {
        // 피버 시스템 업데이트
        _feverSystem?.Update(Time.deltaTime);
    }

    public void ProcessTreeClick(ClickInfo clickInfo)
    {
        // 1. 피버 클릭 카운트 증가 (수동 클릭만)
        if (clickInfo.Type == EClickType.Manual)
        {
            _feverSystem.AddClick();
        }

        // 2. 최종 데미지 계산 (Domain 로직 사용)
        var (finalDamage, isCritical) = DamageCalculator.CalculateFinalDamage(
            clickInfo.Type,
            ManualDamage,
            AutoDamage,
            _criticalSystem.CriticalChance,
            _criticalSystem.CriticalMultiplier,
            _feverSystem.IsFeverActive,
            _feverSystem.FeverMultiplier
        );

        AddApples(finalDamage);

        OnDataChanged?.Invoke();
    }

    public void AddApples(double amount)
    {
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("[GameplayManager] CurrencyManager가 없습니다!");
            return;
        }

        CurrencyManager.Instance.Add(ECurrencyType.Apple, amount);
        TotalApplesCollected += amount;
    }

    private void ProcessAutoHarvest()
    {
        double apples = _autoHarvestSystem.CalculateAutoApples();
        if (apples > 0)
        {
            AddApples(apples);
        }
    }

    public void OnTreeRespawn()
    {
        double bonusApples = ManualDamage * 10;
        AddApples(bonusApples);

        OnTreeRespawned?.Invoke();
        Debug.Log($"나무 리스폰! 보너스 +{bonusApples} 사과");
    }

    public void SetManualDamage(double damage)
    {
        ManualDamage = damage;
        OnDataChanged?.Invoke();
    }

    public void SetAutoDamage(double damage)
    {
        AutoDamage = damage;
        OnDataChanged?.Invoke();
    }

    public void SetSquirrelCount(int count)
    {
        _autoHarvestSystem.SetSquirrelCount(count);
        OnDataChanged?.Invoke();
    }

    public void SetCriticalChance(double chance)
    {
        _criticalSystem.SetCriticalChance(chance);
        OnDataChanged?.Invoke();
    }

    public void SetCriticalMultiplier(double multiplier)
    {
        _criticalSystem.SetCriticalMultiplier(multiplier);
        OnDataChanged?.Invoke();
    }

    public void SetFeverMultiplier(double multiplier)
    {
        OnDataChanged?.Invoke();
    }

    public bool IsFeverActive() => _feverSystem.IsFeverActive;
    public float GetFeverTimeRemaining() => _feverSystem.FeverTimeRemaining;
    public int GetFeverClickCount() => _feverSystem.ClickCount;
    public double GetCriticalChance() => _criticalSystem.CriticalChance;
    public double GetCriticalMultiplier() => _criticalSystem.CriticalMultiplier;
    public int GetSquirrelCount() => _autoHarvestSystem.SquirrelCount;

    public void ResetGame()
    {
        TotalApplesCollected = 0;
        ManualDamage = 1.0;
        AutoDamage = 1.0;

        _feverSystem.Reset();
        _criticalSystem = new CriticalSystem(0.1, 2.0);
        _autoHarvestSystem = new AutoHarvestSystem(50.0);

        OnDataChanged?.Invoke();
        Debug.Log("[GameplayManager] 게임 리셋 완료");
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
}