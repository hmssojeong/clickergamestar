using UnityEngine;
using System;
using Newtonsoft.Json;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("Repository Settings")]
    [SerializeField] private bool _useLocalRepository = true;

    private ICurrencyRepository _repository;
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
        _repository = _useLocalRepository ? new LocalCurrencyRepository() : new FirebaseCurrencyRepository();
    }

    public void SaveGame()
    {
        try
        {
            SaveCurrencyAndUpgrades();

            SaveGameState();
        }
        catch (Exception e)
        {
            Debug.Log($"게임 저장 실패");
        }
    }

    public void LoadGame()
    {
        try
        {
            LoadCurrencyAndUpgrades();

            LoadGameState();
        }
        catch(Exception e)
        {
            Debug.Log($"게임 로드 실패");
        }
    }

    // 재화와 업그레이드 데이터를 Repository를 통해 저장
    private void SaveCurrencyAndUpgrades()
    {
        var saveData = new CurrencySaveData();

        // 재화 데이터 수집
        if(CurrencyManager.Instance != null)
        {
            for (int i = 0; i < (int)ECurrencyType.Count; i++)
            {
                ECurrencyType type = (ECurrencyType)i;
                saveData.Currencies[type] = CurrencyManager.Instance.Get(type).Value;
            }
        }

        // 업그레이드 레벨 수집
        if (UpgradeManager.Instance != null)
        {
            foreach(var upgrade in UpgradeManager.Instance.AllUpgrades)
            {
                saveData.UpgradeLevels[upgrade.Key] = upgrade.Value.Level;
            }
        }

        // Repository를 통해 저장
        _repository.Save(saveData);
    }

    private void LoadCurrencyAndUpgrades()
    {
        var saveData = _repository.Load();

        // 재화 로드
        if(CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.LoadFromData(saveData.Currencies);
        }

        // 업그레이드 로드
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.InitializeUpgrades(saveData.UpgradeLevels);
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
        PlayerPrefs.SetString(gameState.Name, json);
        PlayerPrefs.Save();
    }

    // 게임 상태를 PlayerPrefs에서 로드
    private void LoadGameState()
    {
        if (GameManager.Instance == null) return;

        if (!PlayerPrefs.HasKey(GameStateKey))
        {
            Debug.Log("저장된 게임 상태가 없습니다. 기본값으로 시작합니다.");
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
    }

    // 모든 저장 데이터를 삭제합니다.
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
    }
}
