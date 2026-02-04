using UnityEngine;
using System;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;

/// <summary>
/// 통합 저장/로드 관리자
/// Repository 패턴을 사용하여 각 Manager의 데이터를 저장/로드
/// async/await를 사용한 비동기 처리
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("Repository Settings")]
    [SerializeField] private bool _useLocalRepository = true;

    private ICurrencyRepository _currencyRepository;
    private IUpgradeRepository _upgradeRepository;
    private string _currentUserId = "guest"; // 기본 사용자 ID
    
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

    /// <summary>
    /// Repository 초기화 (로그인 시 호출)
    /// </summary>
    public void InitializeRepository(string userId = "guest")
    {
        _currentUserId = userId;
        
        if (_useLocalRepository)
        {
            _currencyRepository = new LocalCurrencyRepository();
            _upgradeRepository = new LocalUpgradeRepository(userId);
        }
        else
        {
            _currencyRepository = new FirebaseCurrencyRepository(userId);
            _upgradeRepository = new FirebaseUpgradeRepository(userId);
        }
        
        Debug.Log($"[SaveLoadManager] 초기화 완료 - 모드: {(_useLocalRepository ? "Local" : "Firebase")}, UserID: {userId}");
    }

    /// <summary>
    /// 전체 게임 데이터 저장 (비동기)
    /// </summary>
    public async UniTaskVoid SaveGame()
    {
        try
        {
            Debug.Log("[SaveLoadManager] 저장 시작...");
            
            // 재화 + 업그레이드 저장
            await SaveCurrencyAndUpgrades();

            // 게임 상태 저장 (동기)
            SaveGameState();

            Debug.Log("[SaveLoadManager] 저장 완료! ✅");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] 저장 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 전체 게임 데이터 로드 (비동기)
    /// </summary>
    public async UniTask LoadGame()
    {
        try
        {
            Debug.Log("[SaveLoadManager] 로드 시작...");
            
            // 재화 + 업그레이드 로드
            await LoadCurrencyAndUpgrades();

            // 게임 상태 로드 (동기)
            LoadGameState();

            Debug.Log("[SaveLoadManager] 로드 완료! ✅");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] 로드 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 재화와 업그레이드 데이터를 Repository를 통해 저장 (비동기)
    /// </summary>
    private async UniTask SaveCurrencyAndUpgrades()
    {
        // === 재화 데이터 저장 ===
        var currencySaveData = new CurrencySaveData();
        
        if (CurrencyManager.Instance != null)
        {
            for (int i = 0; i < (int)ECurrencyType.Count; i++)
            {
                ECurrencyType type = (ECurrencyType)i;
                currencySaveData.Currencies[type] = CurrencyManager.Instance.Get(type).Value;
            }
        }

        _currencyRepository.Save(currencySaveData).Forget();
        Debug.Log("[SaveLoadManager] 재화 저장 완료");

        // === 업그레이드 데이터 저장 ===
        var upgradeSaveData = new UpgradeSaveData();
        
        if (UpgradeManager.Instance != null)
        {
            foreach (var upgrade in UpgradeManager.Instance.GetAll())
            {
                upgradeSaveData.UpgradeLevels[upgrade.Type] = upgrade.Level;
            }
        }

        _upgradeRepository.Save(upgradeSaveData).Forget();
        Debug.Log("[SaveLoadManager] 업그레이드 저장 완료");
        
        // 저장이 완료될 때까지 잠시 대기
        await UniTask.Delay(100);
    }

    /// <summary>
    /// 재화와 업그레이드 데이터를 Repository에서 로드 (비동기)
    /// </summary>
    private async UniTask LoadCurrencyAndUpgrades()
    {
        // === 재화 데이터 로드 ===
        var currencySaveData = await _currencyRepository.Load();
        
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.LoadFromData(currencySaveData.Currencies);
            Debug.Log("[SaveLoadManager] 재화 로드 완료");
        }

        // === 업그레이드 데이터 로드 ===
        var upgradeSaveData = await _upgradeRepository.Load();
        
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.InitializeUpgrades(upgradeSaveData.UpgradeLevels);
            Debug.Log("[SaveLoadManager] 업그레이드 로드 완료");
        }
    }

    /// <summary>
    /// 게임 상태를 PlayerPrefs에 저장 (동기)
    /// </summary>
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

    /// <summary>
    /// 게임 상태를 PlayerPrefs에서 로드 (동기)
    /// </summary>
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

    /// <summary>
    /// 모든 저장 데이터를 삭제합니다.
    /// </summary>
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[SaveLoadManager] 모든 데이터 초기화 완료");
    }

    /// <summary>
    /// Repository 모드 변경 (런타임)
    /// </summary>
    public void SwitchToLocal()
    {
        _useLocalRepository = true;
        InitializeRepository(_currentUserId);
    }

    /// <summary>
    /// Repository 모드 변경 (런타임)
    /// </summary>
    public void SwitchToFirebase()
    {
        _useLocalRepository = false;
        InitializeRepository(_currentUserId);
    }
}