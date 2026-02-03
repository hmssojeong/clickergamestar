using UnityEngine;
using UnityEngine.Events;
using System;

// 게임의 핵심 플레이 로직을 관리하는 매니저
// - 재화는 CurrencyManager에게 위임
// - 업그레이드는 UpgradeManager에게 위임
// - 게임 플레이 시스템(크리티컬, 피버 등)에만 집중
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Stats (Managed by UpgradeManager)")]
    public double ManualDamage = 1d;    // UpgradeManager가 설정
    public double AutoDamage = 1d;       // UpgradeManager가 설정

    [Header("Game Progress")]
    public double TotalApplesCollected = 0d; // 총 수집한 사과 (통계용)

    [Header("Critical System")]
    public double criticalChance = 0.1d;        // 크리티컬 확률 (UpgradeManager가 설정 가능)
    public double criticalMultiplier = 2.0d;    // 크리티컬 배수 (UpgradeManager가 설정 가능)

    [Header("Squirrel Auto Harvest")]
    public int squirrelCount = 0;               // 다람쥐 수 (UpgradeManager가 설정)
    public double squirrelApplePerSecond = 50d; // 다람쥐당 초당 사과

    [Header("Fever System")]
    public int clickCount = 0;                  // 현재 클릭 횟수
    public int feverThreshold = 75;             // 피버 발동 클릭 횟수 (UpgradeManager가 설정 가능)
    public double feverMultiplier = 2.5d;       // 피버 배수 (UpgradeManager가 설정 가능)
    public float feverDuration = 10f;           // 피버 지속 시간
    public bool isFeverActive = false;          // 피버 활성화 여부
    private float feverTimer = 0f;              // 피버 남은 시간

    [Header("Events")]
    public UnityEvent OnDataChanged;             // 데이터 변경 이벤트
    public UnityEvent OnTreeRespawnEvent;        // 나무 리스폰 이벤트
    public UnityEvent OnFeverStartEvent;         // 피버 시작 이벤트
    public UnityEvent OnFeverEndEvent;           // 피버 종료 이벤트

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 게임 데이터 로드
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.LoadGame();
        }

        // 다람쥐 자동 수확 시작 (1초마다 실행)
        InvokeRepeating(nameof(AutoHarvestBySquirrels), 1f, 1f);
    }

    private void Update()
    {
        // 피버 타임 처리
        if (isFeverActive)
        {
            feverTimer -= Time.deltaTime;
            if (feverTimer <= 0)
            {
                EndFever();
            }
        }
    }

    /// 나무 클릭 시 호출
    public void OnTreeClick()
    {
        // 기본 데미지 계산 (UpgradeManager에서 설정된 값 사용)
        double damage = ManualDamage;

        // 피버 타임 적용
        if (isFeverActive)
        {
            damage *= feverMultiplier;
        }

        // 크리티컬 판정
        bool isCritical = UnityEngine.Random.value < criticalChance;
        if (isCritical)
        {
            damage *= criticalMultiplier;
            Debug.Log("CRITICAL!");
        }

        // CurrencyManager를 통해 사과 추가
        AddApples(damage);

        // 클릭 카운트 증가
        clickCount++;

        // 피버 발동 체크
        if (clickCount >= feverThreshold && !isFeverActive)
        {
            StartFever();
        }
    }

    // 사과를 추가합니다 (CurrencyManager를 통해)
    public void AddApples(double amount)
    {
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("CurrencyManager가 없습니다!");
            return;
        }

        CurrencyManager.Instance.Add(ECurrencyType.Apple, amount);
        TotalApplesCollected += amount;

        Debug.Log($"사과 +{amount}! 총 수집: {TotalApplesCollected}개");
    }

    // 나무가 리스폰될 때 호출
    public void OnTreeRespawn()
    {
        // 보너스 사과 지급
        double bonusApples = ManualDamage * 10;
        AddApples(bonusApples);

        OnTreeRespawnEvent?.Invoke();

        Debug.Log($"나무 리스폰! 보너스 사과 +{bonusApples}");
    }

    // 피버 시작
    void StartFever()
    {
        isFeverActive = true;
        feverTimer = feverDuration;
        clickCount = 0;

        OnFeverStartEvent?.Invoke();
        Debug.Log($" FEVER TIME! (x{feverMultiplier}) ");
    }

    // 피버 종료
    void EndFever()
    {
        isFeverActive = false;
        clickCount = 0;

        OnFeverEndEvent?.Invoke();
        Debug.Log("피버 타임 종료!");
    }

    // 다람쥐 자동 수확
    void AutoHarvestBySquirrels()
    {
        if (squirrelCount > 0)
        {
            double totalAutoApples = squirrelCount * squirrelApplePerSecond;
            AddApples(totalAutoApples);
        }
    }

    // 피버 남은 시간 조회
    public float GetFeverRemainingTime()
    {
        return isFeverActive ? feverTimer : 0f;
    }

    // 현재 클릭 데미지 계산 (피버 포함)
    public double GetCurrentClickDamage()
    {
        double damage = ManualDamage;
        if (isFeverActive)
        {
            damage *= feverMultiplier;
        }
        return damage;
    }

    // 데이터 변경 알림 (UpgradeManager에서 호출)
    public void NotifyDataChanged()
    {
        OnDataChanged?.Invoke();
    }

    // 게임 리셋
    public void ResetGame()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.ResetAllData();
        }

        // 변수 리셋
        TotalApplesCollected = 0;
        ManualDamage = 1;
        AutoDamage = 1;
        criticalChance = 0.1d;
        criticalMultiplier = 2.0d;
        squirrelCount = 0;
        clickCount = 0;
        feverThreshold = 75;
        feverMultiplier = 2.5d;
        feverDuration = 10f;
        isFeverActive = false;

        // CurrencyManager 리셋
        if (CurrencyManager.Instance != null)
        {
            for (int i = 0; i < (int)ECurrencyType.Count; i++)
            {
                CurrencyManager.Instance.Set((ECurrencyType)i, new Currency(0));
            }
        }

        // UpgradeManager 리셋
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.InitializeUpgrades(null);
        }

        OnDataChanged?.Invoke();
    }

    private void OnApplicationQuit()
    {
        // 게임 종료 시 자동 저장
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // 모바일: 앱이 백그라운드로 갈 때 저장
        if (pauseStatus && SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame();
        }
    }
}