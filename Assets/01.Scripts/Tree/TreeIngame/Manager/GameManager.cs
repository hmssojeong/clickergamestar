using UnityEngine;
using UnityEngine.Events;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Damage Settings")]
    public double ManualDamage = 1d;    // 수동 클릭 데미지
    public double AutoDamage = 1d;       // 자동 클릭 데미지

    [Header("Apple Score")]
    public double Apples = 0d;           // 현재 사과 점수
    public double TotalApplesCollected = 0d; // 총 수집한 사과

    [Header("Upgrade Costs")]
    public double ManualUpgradeCost = 10;
    public double AutoUpgradeCost = 50;
    public double AutoClickerCost = 100;

    [Header("Auto Clicker")]
    public bool HasAutoClicker = false;
    public int AutoClickerLevel = 0;

    [Header("Critical System")]
    public double criticalChance = 0.1d;        // 크리티컬 확률 (10%)
    public double criticalMultiplier = 2.0d;    // 크리티컬 배수 (2배)

    [Header("Squirrel Auto Harvest")]
    public int squirrelCount = 0;               // 다람쥐 수
    public double squirrelApplePerSecond = 50d; // 다람쥐당 초당 사과

    [Header("Fever System")]
    public int clickCount = 0;                  // 현재 클릭 횟수
    public int feverThreshold = 75;             // 피버 발동 클릭 횟수
    public double feverMultiplier = 2.5d;       // 피버 배수
    public float feverDuration = 10f;           // 피버 지속 시간
    public bool isFeverActive = false;          // 피버 활성화 여부
    private float feverTimer = 0f;              // 피버 남은 시간

    [Header("Events")]
    public UnityEvent<double> OnAppleChanged;       // 사과 점수 변경 이벤트
    public UnityEvent<double> OnManualDamageChanged; // 수동 데미지 변경 이벤트
    public UnityEvent<double> OnAutoDamageChanged;   // 자동 데미지 변경 이벤트
    public UnityEvent OnTreeRespawnEvent;         // 나무 리스폰 이벤트
    public UnityEvent OnFeverStartEvent;          // 피버 시작 이벤트
    public UnityEvent OnFeverEndEvent;            // 피버 종료 이벤트

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
        // 다람쥐 자동 수확 시작 (1초마다 실행)
        InvokeRepeating("AutoHarvestBySquirrels", 1f, 1f);
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

    public void AddApples(double amount)
    {
        Apples += amount;
        TotalApplesCollected += amount;

        OnAppleChanged?.Invoke(Apples);

        Debug.Log($"사과 +{amount}! 총: {Apples}개");
    }

    // 사과 점수를 사용합니다 (업그레이드 등)
    public bool SpendApples(double amount)
    {
        if (Apples >= amount)
        {
            Apples -= amount;
            OnAppleChanged?.Invoke(Apples);
            return true;
        }

        Debug.Log("사과가 부족합니다!");
        return false;
    }

    // 수동 클릭 데미지를 업그레이드합니다
    public bool UpgradeManualDamage()
    {
        if (SpendApples(ManualUpgradeCost))
        {
            ManualDamage += 1;
            ManualUpgradeCost = Math.Round(ManualUpgradeCost * 1.5f); // 비용 50% 증가

            OnManualDamageChanged?.Invoke(ManualDamage);

            Debug.Log($"수동 데미지 업그레이드! 현재: {ManualDamage}");
            return true;
        }

        return false;
    }

    // 자동 클릭 데미지를 업그레이드합니다
    public bool UpgradeAutoDamage()
    {
        if (SpendApples(AutoUpgradeCost))
        {
            AutoDamage += 1;
            AutoUpgradeCost = Math.Round(AutoUpgradeCost * 1.5f);

            OnAutoDamageChanged?.Invoke(AutoDamage);

            Debug.Log($"자동 데미지 업그레이드! 현재: {AutoDamage}");
            return true;
        }

        return false;
    }

    // 자동 클리커를 구매합니다
    public bool BuyAutoClicker()
    {
        if (SpendApples(AutoClickerCost))
        {
            HasAutoClicker = true;
            AutoClickerLevel++;
            AutoClickerCost = Math.Round(AutoClickerCost * 2f); // 비용 2배 증가

            // AutoClicker 오브젝트 활성화
            GameObject autoClicker = GameObject.Find("AutoClicker");
            if (autoClicker != null)
            {
                autoClicker.SetActive(true);
            }

            Debug.Log($"자동 클리커 구매! 레벨: {AutoClickerLevel}");
            return true;
        }

        return false;
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

    public void OnTreeClick()
    {
        // 기본 데미지 계산
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
            Debug.Log("⭐ CRITICAL! ⭐");
        }

        // 사과 추가
        AddApples(damage);

        // 클릭 카운트 증가
        clickCount++;

        // 피버 발동 체크
        if (clickCount >= feverThreshold && !isFeverActive)
        {
            StartFever();
        }
    }

    void StartFever()
    {
        isFeverActive = true;
        feverTimer = feverDuration;
        clickCount = 0;

        OnFeverStartEvent?.Invoke();
        Debug.Log($" FEVER TIME! (x{feverMultiplier}) ");
    }

    void EndFever()
    {
        isFeverActive = false;
        clickCount = 0;

        OnFeverEndEvent?.Invoke();
    }

    void AutoHarvestBySquirrels()
    {
        if (squirrelCount > 0)
        {
            double totalAutoApples = squirrelCount * squirrelApplePerSecond;
            AddApples(totalAutoApples);
        }
    }


    public float GetFeverRemainingTime()
    {
        return isFeverActive ? feverTimer : 0f;
    }

    public double GetCurrentClickDamage()
    {
        double damage = ManualDamage;
        if (isFeverActive)
        {
            damage *= feverMultiplier;
        }
        return damage;
    }

    public void SaveGame()
    {
        // 기존 데이터 저장
        PlayerPrefs.SetString("Apples", Apples.ToString());
        PlayerPrefs.SetString("TotalApples", TotalApplesCollected.ToString());
        PlayerPrefs.SetString("ManualDamage", ManualDamage.ToString());
        PlayerPrefs.SetString("AutoDamage", AutoDamage.ToString());
        PlayerPrefs.SetString("ManualUpgradeCost", ManualUpgradeCost.ToString());
        PlayerPrefs.SetString("AutoUpgradeCost", AutoUpgradeCost.ToString());
        PlayerPrefs.SetString("AutoClickerCost", AutoClickerCost.ToString());
        PlayerPrefs.SetInt("HasAutoClicker", HasAutoClicker ? 1 : 0);
        PlayerPrefs.SetString("AutoClickerLevel", AutoClickerLevel.ToString());

        // 새로운 업그레이드 데이터 저장
        PlayerPrefs.SetString("CriticalChance", criticalChance.ToString());
        PlayerPrefs.SetString("CriticalMultiplier", criticalMultiplier.ToString());
        PlayerPrefs.SetString("SquirrelCount", squirrelCount.ToString());
        PlayerPrefs.SetString("FeverThreshold", feverThreshold.ToString());
        PlayerPrefs.SetString("FeverMultiplier", feverMultiplier.ToString());
        PlayerPrefs.SetString("FeverDuration", feverDuration.ToString());

        PlayerPrefs.Save();

        Debug.Log("게임 저장 완료!");
    }

    public void LoadGame()
    {
        // 기존 데이터 로드
        Apples = double.Parse(PlayerPrefs.GetString("Apples", "0"));
        TotalApplesCollected = double.Parse(PlayerPrefs.GetString("TotalApples", "0"));
        ManualDamage = double.Parse(PlayerPrefs.GetString("ManualDamage", "1"));
        AutoDamage = double.Parse(PlayerPrefs.GetString("AutoDamage", "1"));
        ManualUpgradeCost = double.Parse(PlayerPrefs.GetString("ManualUpgradeCost", "10"));
        AutoUpgradeCost = double.Parse(PlayerPrefs.GetString("AutoUpgradeCost", "50"));
        AutoClickerCost = double.Parse(PlayerPrefs.GetString("AutoClickerCost", "100"));
        HasAutoClicker = PlayerPrefs.GetInt("HasAutoClicker", 0) == 1;
        AutoClickerLevel = int.Parse(PlayerPrefs.GetString("AutoClickerLevel", "0"));

        // 새로운 업그레이드 데이터 로드
        criticalChance = double.Parse(PlayerPrefs.GetString("CriticalChance", "0.1"));
        criticalMultiplier = double.Parse(PlayerPrefs.GetString("CriticalMultiplier", "2"));
        squirrelCount = int.Parse(PlayerPrefs.GetString("SquirrelCount", "0"));
        feverThreshold = int.Parse(PlayerPrefs.GetString("FeverThreshold", "75"));
        feverMultiplier = double.Parse(PlayerPrefs.GetString("FeverMultiplier", "2.5"));
        feverDuration = float.Parse(PlayerPrefs.GetString("FeverDuration", "10"));

        OnAppleChanged?.Invoke(Apples);
        OnManualDamageChanged?.Invoke(ManualDamage);
        OnAutoDamageChanged?.Invoke(AutoDamage);

        Debug.Log("게임 로드 완료!");
    }

    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();

        // 기존 변수 리셋
        Apples = 0;
        TotalApplesCollected = 0;
        ManualDamage = 1;
        AutoDamage = 1;
        ManualUpgradeCost = 10;
        AutoUpgradeCost = 50;
        AutoClickerCost = 100;
        HasAutoClicker = false;
        AutoClickerLevel = 0;

        // 새로운 변수 리셋
        criticalChance = 0.1d;
        criticalMultiplier = 2.0d;
        squirrelCount = 0;
        clickCount = 0;
        feverThreshold = 75;
        feverMultiplier = 2.5d;
        feverDuration = 10f;
        isFeverActive = false;

        OnAppleChanged?.Invoke(Apples);

        Debug.Log("게임 리셋!");
    }

    private void OnApplicationQuit()
    {
        // 게임 종료 시 자동 저장
        SaveGame();
    }
}
