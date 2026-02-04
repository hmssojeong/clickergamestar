using UnityEngine;
using UnityEngine.Pool;

// 플로팅 텍스트를 생성하고 관리하는 매니저
public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [Header("Prefab")]
    [SerializeField] private GameObject _floatingTextPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform _canvasTransform;
    [SerializeField] private int _defaultCapacity = 10;
    [SerializeField] private int _maxPoolSize = 30;

    private IObjectPool<FloatingText> _pool;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        if (_floatingTextPrefab == null) return;

        _pool = new ObjectPool<FloatingText>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(_floatingTextPrefab, _canvasTransform);
                FloatingText floatingText = obj.GetComponent<FloatingText>();

                floatingText.SetPool(_pool);
                obj.SetActive(false);
                return floatingText;
            },
            actionOnGet: (ft) => ft.gameObject.SetActive(true),
            actionOnRelease: (ft) => ft.gameObject.SetActive(false),
            actionOnDestroy: (ft) => Destroy(ft.gameObject),
            collectionCheck: true,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxPoolSize
        );

        // 풀 예열 (초기 생성)
        var tempList = new System.Collections.Generic.List<FloatingText>(_defaultCapacity);
        for (int i = 0; i < _defaultCapacity; i++)
        {
            tempList.Add(_pool.Get());
        }
        foreach (var item in tempList)
        {
            _pool.Release(item);
        }
    }

    // 데미지 텍스트 표시
    public void ShowDamage(Vector3 worldPosition, double damage, bool isCritical = false)
    {
        string formatted = damage.ToFormattedString();
        ShowText(worldPosition, $"+{formatted}", isCritical);
    }

    // 점수 텍스트 표시
    public void ShowScore(Vector3 worldPosition, double score)
    {
        string formatted = score.ToFormattedString();
        ShowText(worldPosition, $"+{formatted}", false);
    }

    // 일반 텍스트 표시
    public void ShowText(Vector3 worldPosition, string text, bool isCritical = false)
    {
        if (_pool == null)
        {
            return;
        }

        // Instantiate 대신 풀에서 가져오기
        FloatingText floatingText = _pool.Get();

        if (floatingText != null)
        {
            floatingText.Initialize(text, worldPosition, isCritical);
            floatingText.AddRandomOffset();
        }
    }

    // 여러 텍스트를 연속으로 표시 (콤보 효과)
    public void ShowCombo(Vector3 worldPosition, int comboCount)
    {
        string comboText = comboCount switch
        {
            >= 10 => "MEGA COMBO!",
            >= 5 => "SUPER COMBO!",
            >= 3 => "COMBO!",
            _ => $"x{comboCount}"
        };

        ShowText(worldPosition, comboText, comboCount >= 5);
    }
}
