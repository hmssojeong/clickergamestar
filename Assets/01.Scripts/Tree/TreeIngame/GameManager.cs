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

    [Header("Events")]
    public UnityEvent<double> OnAppleChanged;       // 사과 점수 변경 이벤트
    public UnityEvent<double> OnManualDamageChanged; // 수동 데미지 변경 이벤트
    public UnityEvent<double> OnAutoDamageChanged;   // 자동 데미지 변경 이벤트
    public UnityEvent OnTreeRespawnEvent;         // 나무 리스폰 이벤트

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

    // 사과 점수를 추가합니다
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

    /// <summary>
    /// 자동 클릭 데미지를 업그레이드합니다
    /// </summary>
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

    /// <summary>
    /// 나무가 리스폰될 때 호출
    /// </summary>
    public void OnTreeRespawn()
    {
        // 보너스 사과 지급
        double bonusApples = ManualDamage * 10;
        AddApples(bonusApples);

        OnTreeRespawnEvent?.Invoke();

        Debug.Log($"나무 리스폰! 보너스 사과 +{bonusApples}");
    }

    /// <summary>
    /// 게임 데이터 저장 (PlayerPrefs 사용)
    /// </summary>
    public void SaveGame()
    {
        PlayerPrefs.SetString("Apples", Apples.ToString());
        PlayerPrefs.SetString("TotalApples", TotalApplesCollected.ToString());
        PlayerPrefs.SetString("ManualDamage", ManualDamage.ToString());
        PlayerPrefs.SetString("AutoDamage", AutoDamage.ToString());
        PlayerPrefs.SetString("ManualUpgradeCost", ManualUpgradeCost.ToString());
        PlayerPrefs.SetString("AutoUpgradeCost", AutoUpgradeCost.ToString());
        PlayerPrefs.SetString("AutoClickerCost", AutoClickerCost.ToString());
        PlayerPrefs.SetInt("HasAutoClicker", HasAutoClicker ? 1 : 0);
        PlayerPrefs.SetString("AutoClickerLevel", AutoClickerLevel.ToString());
        PlayerPrefs.Save();

        Debug.Log("게임 저장 완료!");
    }

    /// <summary>
    /// 게임 데이터 로드
    /// </summary>
    public void LoadGame()
    {
        Apples = double.Parse(PlayerPrefs.GetString("Apples", "0"));
        TotalApplesCollected = double.Parse(PlayerPrefs.GetString("TotalApples", "0"));
        ManualDamage = double.Parse(PlayerPrefs.GetString("ManualDamage", "1"));
        AutoDamage = double.Parse(PlayerPrefs.GetString("AutoDamage", "1"));
        ManualUpgradeCost = double.Parse(PlayerPrefs.GetString("ManualUpgradeCost", "10"));
        AutoUpgradeCost = double.Parse(PlayerPrefs.GetString("AutoUpgradeCost", "50"));
        AutoClickerCost = double.Parse(PlayerPrefs.GetString("AutoClickerCost", "100"));
        HasAutoClicker = PlayerPrefs.GetInt("HasAutoClicker", 0) == 1;
        AutoClickerLevel = int.Parse(PlayerPrefs.GetString("AutoClickerLevel", "0"));

        OnAppleChanged?.Invoke(Apples);
        OnManualDamageChanged?.Invoke(ManualDamage);
        OnAutoDamageChanged?.Invoke(AutoDamage);

        Debug.Log("게임 로드 완료!");
    }

    /// <summary>
    /// 게임 데이터 리셋
    /// </summary>
    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();

        Apples = 0;
        TotalApplesCollected = 0;
        ManualDamage = 1;
        AutoDamage = 1;
        ManualUpgradeCost = 10;
        AutoUpgradeCost = 50;
        AutoClickerCost = 100;
        HasAutoClicker = false;
        AutoClickerLevel = 0;

        OnAppleChanged?.Invoke(Apples);

        Debug.Log("게임 리셋!");
    }

    private void OnApplicationQuit()
    {
        // 게임 종료 시 자동 저장
        SaveGame();
    }
}