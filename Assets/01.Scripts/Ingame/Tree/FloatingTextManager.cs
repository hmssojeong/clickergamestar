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
        ShowText(worldPosition, $"+{damage}", isCritical);
    }

    // 점수 텍스트 표시
    public void ShowScore(Vector3 worldPosition, double score)
    {
        ShowText(worldPosition, $"+{score} 🍎", false);
    }

    // 일반 텍스트 표시
    public void ShowText(Vector3 worldPosition, string text, bool isCritical = false)
    {
        if (_floatingTextPrefab == null)
        {
            Debug.LogWarning("FloatingText Prefab이 설정되지 않았습니다!");
            return;
        }

        // 플로팅 텍스트 생성
        GameObject obj = Instantiate(_floatingTextPrefab, _canvasTransform);
        FloatingText floatingText = obj.GetComponent<FloatingText>();

        if (floatingText != null)
        {
            floatingText.Initialize(text, worldPosition, isCritical);
            floatingText.AddRandomOffset(); // 랜덤 오프셋 추가
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
