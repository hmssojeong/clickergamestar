using UnityEngine;
using System;
using Newtonsoft.Json;

// 통합 저장/로드 관리자
// 각 Manager에게 저장/로드를 위임
public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("Repository Settings")]
    [SerializeField] private bool _useLocalRepository = true;

    private ICurrencyRepository _currencyRepository;
    private const string GameStateKey = "GameState";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeRepository();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeRepository()
    {
        _currencyRepository = _useLocalRepository 
            ? new LocalCurrencyRepository() 
            : new FirebaseCurrencyRepository();
        
        Debug.Log($"[SaveLoadManager] 초기화 완료 - 모드: {(_useLocalRepository ? "Local" : "Firebase")}");
    }

    // 전체 게임 데이터 저장
    public void SaveGame()
    {
        try
        {
            // 재화 + 업그레이드 저장
            SaveCurrencyAndUpgrades();

            // 게임 상태 저장
            SaveGameState();

            Debug.Log("[SaveLoadManager] 저장 완료! ✅");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] 저장 실패: {e.Message}");
        }
    }

    // 전체 게임 데이터 로드
    public void LoadGame()
    {
        try
        {
            // 재화 + 업그레이드 로드
            LoadCurrencyAndUpgrades();

            // 게임 상태 로드
            LoadGameState();

            Debug.Log("[SaveLoadManager] 로드 완료! ✅");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] 로드 실패: {e.Message}");
        }
    }

    // 재화와 업그레이드 데이터를 Repository를 통해 저장
    private void SaveCurrencyAndUpgrades()
    {
        var saveData = new CurrencySaveData();

        // 재화 데이터 수집
        if (CurrencyManager.Instance != null)
        {
            for (int i = 0; i < (int)ECurrencyType.Count; i++)
            {
                ECurrencyType type = (ECurrencyType)i;
                saveData.Currencies[type] = CurrencyManager.Instance.Get(type).Value;
            }
        }

        // 업그레이드 데이터 수집
        if (UpgradeManager.Instance != null)
        {
            foreach (var upgrade in UpgradeManager.Instance.GetAll())
            {
                saveData.UpgradeLevels[upgrade.Type] = upgrade.Level;
            }
        }

        // Repository를 통해 저장
        _currencyRepository.Save(saveData);
        
        Debug.Log("[SaveLoadManager] 재화 및 업그레이드 저장 완료");
    }

    private void LoadCurrencyAndUpgrades()
    {
        var saveData = _currencyRepository.Load();

        // 재화 로드
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.LoadFromData(saveData.Currencies);
            Debug.Log("[SaveLoadManager] 재화 로드 완료");
        }

        // 업그레이드 로드
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.InitializeUpgrades(saveData.UpgradeLevels);
            Debug.Log("[SaveLoadManager] 업그레이드 로드 완료");
        }
    }

    // 게임 상태를 PlayerPrefs에 저장
    private void SaveGameState()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        var gameState = new GameStateSaveData
        {
            TotalApplesCollected = GameManager.Instance.TotalApplesCollected,
            CriticalChance = GameManager.Instance.criticalChance,
            CriticalMultiplier = GameManager.Instance.criticalMultiplier,
            SquirrelCount = GameManager.Instance.squirrelCount,
            SquirrelApplePerSecond = GameManager.Instance.squirrelApplePerSecond,
            FeverThreshold = GameManager.Instance.feverThreshold,
            FeverMultiplier = GameManager.Instance.feverMultiplier,
            FeverDuration = GameManager.Instance.feverDuration
        };

        string json = JsonConvert.SerializeObject(gameState);
        PlayerPrefs.SetString(GameStateKey, json);
        PlayerPrefs.Save();
        
        Debug.Log("[SaveLoadManager] 게임 상태 저장 완료");
    }

    // 게임 상태를 PlayerPrefs에서 로드
    private void LoadGameState()
    {
        if (GameManager.Instance == null) return;

        if (!PlayerPrefs.HasKey(GameStateKey))
        {
            Debug.LogWarning("[SaveLoadManager] 저장된 게임 상태가 없습니다. 기본값으로 시작합니다.");
            return;
        }

        string json = PlayerPrefs.GetString(GameStateKey);
        var gameState = JsonConvert.DeserializeObject<GameStateSaveData>(json);

        GameManager.Instance.TotalApplesCollected = gameState.TotalApplesCollected;
        GameManager.Instance.criticalChance = gameState.CriticalChance;
        GameManager.Instance.criticalMultiplier = gameState.CriticalMultiplier;
        GameManager.Instance.squirrelCount = gameState.SquirrelCount;
        GameManager.Instance.squirrelApplePerSecond = gameState.SquirrelApplePerSecond;
        GameManager.Instance.feverThreshold = gameState.FeverThreshold;
        GameManager.Instance.feverMultiplier = gameState.FeverMultiplier;
        GameManager.Instance.feverDuration = gameState.FeverDuration;

        // UI 갱신
        GameManager.Instance.NotifyDataChanged();
        
        Debug.Log("[SaveLoadManager] 게임 상태 로드 완료");
    }

    // 모든 저장 데이터를 삭제합니다.
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[SaveLoadManager] 모든 데이터 초기화 완료");
    }
}
