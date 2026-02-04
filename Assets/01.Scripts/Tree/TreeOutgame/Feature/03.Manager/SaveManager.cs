using UnityEngine;
using System;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;

// - 각 Manager가 자신의 데이터를 저장/로드 담당
// - SaveManager는 저장 타이밍만 관리
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool _autoSaveEnabled = true;
    [SerializeField] private float _autoSaveInterval = 60f; // 60초마다 자동 저장

    private const string GameStateKey = "GameState_v2";

    private void Awake()
    {
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
        if (_autoSaveEnabled)
        {
            InvokeRepeating(nameof(AutoSave), _autoSaveInterval, _autoSaveInterval);
        }
    }

    private void AutoSave()
    {
        SaveGame().Forget();
    }

    public async UniTaskVoid SaveGame()
    {
        try
        {
            Debug.Log("[SaveManager] 저장 시작...");

            // 1. 재화 저장 (CurrencyManager가 Repository를 통해 저장)
            if (CurrencyManager.Instance != null)
            {
                await CurrencyManager.Instance.SaveToRepository();
            }

            // 2. 업그레이드 저장 (UpgradeManager가 Repository를 통해 저장)
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.SaveUpgrades().Forget();
            }

            // 3. 게임 상태 저장 (PlayerPrefs)
            SaveGameplayState();

            Debug.Log("[SaveManager] 저장 완료! ✅");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] 저장 실패: {e.Message}");
        }
    }

    public async UniTask LoadGame()
    {
        try
        {
            Debug.Log("[SaveManager] 로드 시작...");

            // 1. 재화 로드
            if (CurrencyManager.Instance != null)
            {
                await CurrencyManager.Instance.LoadFromRepository();
            }

            // 2. 업그레이드 로드
            if (UpgradeManager.Instance != null)
            {
                await UpgradeManager.Instance.LoadUpgrades();
            }

            // 3. 게임 상태 로드
            LoadGameplayState();

            Debug.Log("[SaveManager] 로드 완료! ✅");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] 로드 실패: {e.Message}");
        }
    }

    private void SaveGameplayState()
    {
        if (GameplayManager.Instance == null) return;

        var state = new GameplayStateData
        {
            TotalApplesCollected = GameplayManager.Instance.TotalApplesCollected,
            ManualDamage = GameplayManager.Instance.ManualDamage,
            AutoDamage = GameplayManager.Instance.AutoDamage,
            CriticalChance = GameplayManager.Instance.GetCriticalChance(),
            CriticalMultiplier = GameplayManager.Instance.GetCriticalMultiplier(),
            SquirrelCount = GameplayManager.Instance.GetSquirrelCount()
        };

        string json = JsonConvert.SerializeObject(state);
        PlayerPrefs.SetString(GameStateKey, json);
        PlayerPrefs.Save();

        Debug.Log("[SaveManager] 게임 상태 저장 완료");
    }

    private void LoadGameplayState()
    {
        if (GameplayManager.Instance == null) return;

        if (!PlayerPrefs.HasKey(GameStateKey))
        {
            Debug.LogWarning("[SaveManager] 저장된 게임 상태가 없습니다.");
            return;
        }

        string json = PlayerPrefs.GetString(GameStateKey);
        var state = JsonConvert.DeserializeObject<GameplayStateData>(json);

        // GameplayManager에 상태 적용
        GameplayManager.Instance.SetManualDamage(state.ManualDamage);
        GameplayManager.Instance.SetAutoDamage(state.AutoDamage);
        GameplayManager.Instance.SetCriticalChance(state.CriticalChance);
        GameplayManager.Instance.SetCriticalMultiplier(state.CriticalMultiplier);
        GameplayManager.Instance.SetSquirrelCount(state.SquirrelCount);

        Debug.Log("[SaveManager] 게임 상태 로드 완료");
    }

    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[SaveManager] 모든 데이터 초기화 완료");
    }

    [System.Serializable]
    private class GameplayStateData
    {
        public double TotalApplesCollected;
        public double ManualDamage;
        public double AutoDamage;
        public double CriticalChance;
        public double CriticalMultiplier;
        public int SquirrelCount;
    }
}