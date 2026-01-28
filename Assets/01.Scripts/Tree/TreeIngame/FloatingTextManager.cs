using UnityEngine;

// 플로팅 텍스트를 생성하고 관리하는 매니저
public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;

    [Header("Prefab")]
    [SerializeField] private GameObject _floatingTextPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform _canvasTransform;
    [SerializeField] private int _maxPoolSize = 30;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 데미지 텍스트 표시
    public void ShowDamage(Vector3 worldPosition, double damage, bool isCritical = false)
    {
        string formatted = CurrencyFormatter.Format(damage);
        ShowText(worldPosition, $"+{formatted}", isCritical);
    }

    // 점수 텍스트 표시
    public void ShowScore(Vector3 worldPosition, double score)
    {
        string formatted = CurrencyFormatter.Format(score);
        ShowText(worldPosition, $"+{formatted} 🍎", false);
    }

    // 일반 텍스트 표시
    public void ShowText(Vector3 worldPosition, string text, bool isCritical = false)
    {
        if (_floatingTextPrefab == null)
        {
            return;
        }

        GameObject obj = Instantiate(_floatingTextPrefab, _canvasTransform);
        FloatingText floatingText = obj.GetComponent<FloatingText>();

        if (floatingText != null)
        {
            // 1. 초기 위치 설정 전 랜덤 값을 먼저 적용하거나, 
            // 2. Initialize 내부에서 위치를 잡은 직후 바로 Offset을 줍니다.
            floatingText.Initialize(text, worldPosition, isCritical);
            floatingText.AddRandomOffset(); // 여기서 위치를 한 번 더 흩뿌려줍니다.
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
