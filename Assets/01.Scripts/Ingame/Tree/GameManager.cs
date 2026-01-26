using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 게임 전체를 관리하는 매니저
/// 점수, 업그레이드, 재화 등을 관리합니다
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Damage Settings")]
    public int ManualDamage = 1;    // 수동 클릭 데미지
    public int AutoDamage = 1;       // 자동 클릭 데미지

    [Header("Apple Score")]
    public int Apples = 0;           // 현재 사과 점수
    public int TotalApplesCollected = 0; // 총 수집한 사과

    [Header("Upgrade Costs")]
    public int ManualUpgradeCost = 10;
    public int AutoUpgradeCost = 50;
    public int AutoClickerCost = 100;

    [Header("Auto Clicker")]
    public bool HasAutoClicker = false;
    public int AutoClickerLevel = 0;

    [Header("Events")]
    public UnityEvent<int> OnAppleChanged;       // 사과 점수 변경 이벤트
    public UnityEvent<int> OnManualDamageChanged; // 수동 데미지 변경 이벤트
    public UnityEvent<int> OnAutoDamageChanged;   // 자동 데미지 변경 이벤트
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

    /// <summary>
    /// 사과 점수를 추가합니다
    /// </summary>
    public void AddApples(int amount)
    {
        Apples += amount;
        TotalApplesCollected += amount;
        
        OnAppleChanged?.Invoke(Apples);
        
        Debug.Log($"사과 +{amount}! 총: {Apples}개");
    }

    /// <summary>
    /// 사과 점수를 사용합니다 (업그레이드 등)
    /// </summary>
    public bool SpendApples(int amount)
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

    /// <summary>
    /// 수동 클릭 데미지를 업그레이드합니다
    /// </summary>
    public bool UpgradeManualDamage()
    {
        if (SpendApples(ManualUpgradeCost))
        {
            ManualDamage += 1;
            ManualUpgradeCost = Mathf.RoundToInt(ManualUpgradeCost * 1.5f); // 비용 50% 증가
            
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
            AutoUpgradeCost = Mathf.RoundToInt(AutoUpgradeCost * 1.5f);
            
            OnAutoDamageChanged?.Invoke(AutoDamage);
            
            Debug.Log($"자동 데미지 업그레이드! 현재: {AutoDamage}");
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// 자동 클리커를 구매합니다
    /// </summary>
    public bool BuyAutoClicker()
    {
        if (SpendApples(AutoClickerCost))
        {
            HasAutoClicker = true;
            AutoClickerLevel++;
            AutoClickerCost = Mathf.RoundToInt(AutoClickerCost * 2f); // 비용 2배 증가
            
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
        int bonusApples = ManualDamage * 10;
        AddApples(bonusApples);
        
        OnTreeRespawnEvent?.Invoke();
        
        Debug.Log($"나무 리스폰! 보너스 사과 +{bonusApples}");
    }

    /// <summary>
    /// 게임 데이터 저장 (PlayerPrefs 사용)
    /// </summary>
    public void SaveGame()
    {
        PlayerPrefs.SetInt("Apples", Apples);
        PlayerPrefs.SetInt("TotalApples", TotalApplesCollected);
        PlayerPrefs.SetInt("ManualDamage", ManualDamage);
        PlayerPrefs.SetInt("AutoDamage", AutoDamage);
        PlayerPrefs.SetInt("ManualUpgradeCost", ManualUpgradeCost);
        PlayerPrefs.SetInt("AutoUpgradeCost", AutoUpgradeCost);
        PlayerPrefs.SetInt("AutoClickerCost", AutoClickerCost);
        PlayerPrefs.SetInt("HasAutoClicker", HasAutoClicker ? 1 : 0);
        PlayerPrefs.SetInt("AutoClickerLevel", AutoClickerLevel);
        PlayerPrefs.Save();
        
        Debug.Log("게임 저장 완료!");
    }

    /// <summary>
    /// 게임 데이터 로드
    /// </summary>
    public void LoadGame()
    {
        Apples = PlayerPrefs.GetInt("Apples", 0);
        TotalApplesCollected = PlayerPrefs.GetInt("TotalApples", 0);
        ManualDamage = PlayerPrefs.GetInt("ManualDamage", 1);
        AutoDamage = PlayerPrefs.GetInt("AutoDamage", 1);
        ManualUpgradeCost = PlayerPrefs.GetInt("ManualUpgradeCost", 10);
        AutoUpgradeCost = PlayerPrefs.GetInt("AutoUpgradeCost", 50);
        AutoClickerCost = PlayerPrefs.GetInt("AutoClickerCost", 100);
        HasAutoClicker = PlayerPrefs.GetInt("HasAutoClicker", 0) == 1;
        AutoClickerLevel = PlayerPrefs.GetInt("AutoClickerLevel", 0);
        
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
