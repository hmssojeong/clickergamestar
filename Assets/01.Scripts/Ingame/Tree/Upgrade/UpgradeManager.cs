using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업그레이드 시스템 관리
/// 스크롤 뷰에 아이템들을 동적으로 생성하고 관리합니다
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("UI References")]
    [SerializeField] private Transform _contentTransform; // ScrollView의 Content
    [SerializeField] private GameObject _upgradeItemPrefab; // UpgradeItem Prefab

    [Header("Upgrade Data")]
    [SerializeField] private List<UpgradeData> _upgradeList = new List<UpgradeData>();

    private Dictionary<string, UpgradeItem> _upgradeItems = new Dictionary<string, UpgradeItem>();

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeUpgrades();

        // GameManager 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAppleChanged.AddListener(OnApplesChanged);
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAppleChanged.RemoveListener(OnApplesChanged);
        }
    }

    /// <summary>
    /// 업그레이드 목록 초기화
    /// </summary>
    private void InitializeUpgrades()
    {
        if (_contentTransform == null)
        {
            Debug.LogError("Content Transform이 연결되지 않았습니다!");
            return;
        }

        if (_upgradeItemPrefab == null)
        {
            Debug.LogError("Upgrade Item Prefab이 연결되지 않았습니다!");
            return;
        }

        foreach (var data in _upgradeList)
        {
            CreateUpgradeItem(data);
        }

        Debug.Log($"{_upgradeList.Count}개의 업그레이드 아이템 생성 완료");
    }

    /// <summary>
    /// 업그레이드 아이템 생성
    /// </summary>
    private void CreateUpgradeItem(UpgradeData data)
    {
        GameObject itemObj = Instantiate(_upgradeItemPrefab, _contentTransform);
        UpgradeItem item = itemObj.GetComponent<UpgradeItem>();

        if (item == null)
        {
            Debug.LogError("UpgradeItem 스크립트를 찾을 수 없습니다!");
            return;
        }

        long currentDps = CalculateDps(data);
        int currentCost = CalculateCost(data);

        item.Initialize(
            data.id,
            data.name,
            data.icon,
            data.currentLevel,
            currentDps,
            currentCost,
            data.isHired
        );

        _upgradeItems.Add(data.id, item);
    }

    /// <summary>
    /// 업그레이드 구매
    /// </summary>
    public void PurchaseUpgrade(string itemId)
    {
        UpgradeData data = _upgradeList.Find(x => x.id == itemId);
        if (data == null)
        {
            Debug.LogError($"아이템 ID를 찾을 수 없습니다: {itemId}");
            return;
        }

        // 비용 확인
        int cost = CalculateCost(data);
        if (GameManager.Instance.Apples < cost)
        {
            Debug.Log("사과가 부족합니다!");
            return;
        }

        // 비용 차감
        GameManager.Instance.SpendApples(cost);

        // 레벨업
        if (!data.isHired)
        {
            // 첫 구매 (고용)
            data.isHired = true;
            data.currentLevel = 1;
            Debug.Log($"{data.name} 고용 완료!");
        }
        else
        {
            // 레벨업
            data.currentLevel++;
            Debug.Log($"{data.name} 레벨 {data.currentLevel} 달성!");
        }

        // DPS 증가 (GameManager의 AutoDamage에 추가)
        long dpsIncrease = CalculateDpsIncrease(data);
        GameManager.Instance.AutoDamage += (int)Mathf.Min(dpsIncrease, int.MaxValue);

        // UI 업데이트
        RefreshItem(itemId);

        // 다른 모든 아이템의 버튼 상태도 업데이트 (사과가 줄어들었으므로)
        RefreshAllItems();
    }

    /// <summary>
    /// 특정 아이템 UI 새로고침
    /// </summary>
    private void RefreshItem(string itemId)
    {
        UpgradeData data = _upgradeList.Find(x => x.id == itemId);
        if (data == null) return;

        if (_upgradeItems.ContainsKey(itemId))
        {
            int cost = CalculateCost(data);
            long dps = CalculateDps(data);
            _upgradeItems[itemId].Refresh(data.currentLevel, dps, cost, data.isHired);
        }
    }

    /// <summary>
    /// 모든 아이템 UI 새로고침
    /// </summary>
    public void RefreshAllItems()
    {
        foreach (var data in _upgradeList)
        {
            if (_upgradeItems.ContainsKey(data.id))
            {
                int cost = CalculateCost(data);
                long dps = CalculateDps(data);
                _upgradeItems[data.id].Refresh(data.currentLevel, dps, cost, data.isHired);
            }
        }
    }

    /// <summary>
    /// 비용 계산
    /// </summary>
    private int CalculateCost(UpgradeData data)
    {
        if (data.currentLevel == 0 && !data.isHired)
        {
            // 첫 구매 비용
            return data.baseCost;
        }

        // 레벨업 비용 (1.15배씩 증가)
        return Mathf.RoundToInt(data.baseCost * Mathf.Pow(1.15f, data.currentLevel));
    }

    /// <summary>
    /// DPS 계산
    /// </summary>
    private long CalculateDps(UpgradeData data)
    {
        if (data.currentLevel == 0)
        {
            return data.baseDps;
        }

        // 레벨당 1.5배 증가
        return (long)(data.baseDps * Mathf.Pow(1.5f, data.currentLevel - 1));
    }

    /// <summary>
    /// 이번 레벨업으로 증가하는 DPS 계산
    /// </summary>
    private long CalculateDpsIncrease(UpgradeData data)
    {
        if (data.currentLevel == 1)
        {
            // 첫 고용
            return data.baseDps;
        }

        // 이전 레벨과 현재 레벨의 차이
        long currentDps = (long)(data.baseDps * Mathf.Pow(1.5f, data.currentLevel - 1));
        long previousDps = (long)(data.baseDps * Mathf.Pow(1.5f, data.currentLevel - 2));

        return currentDps - previousDps;
    }

    /// <summary>
    /// 사과 개수가 변경되었을 때 호출
    /// </summary>
    private void OnApplesChanged(double newAmount)
    {
        // 모든 버튼의 활성화 상태 업데이트
        RefreshAllItems();
    }
}

/// <summary>
/// 업그레이드 데이터 클래스
/// </summary>
[System.Serializable]
public class UpgradeData
{
    [Header("Basic Info")]
    public string id = "unique_id"; // 고유 ID
    public string name = "Item Name"; // 아이템 이름
    public Sprite icon; // 아이콘 이미지

    [Header("Stats")]
    public int baseCost = 100; // 기본 비용
    public long baseDps = 10; // 기본 DPS

    [Header("Current State")]
    public int currentLevel = 0; // 현재 레벨
    public bool isHired = false; // 고용 여부
}