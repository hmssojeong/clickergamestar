using UnityEngine;

public class Tree : MonoBehaviour, Clickable
{
    [Header("Tree Settings")]
    [SerializeField] private string _treeName = "Apple Tree";
    [SerializeField] private double _maxHealth = 100;
    private double _currentHealth;

    [Header("Apple Drop Settings")]
    [SerializeField] private GameObject _applePrefab;
    [SerializeField] private GameObject _goldApplePrefab;
    [SerializeField] private Transform[] _appleSpawnPoints;
    [SerializeField] private float _appleDropForce = 5f;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem _leafParticle;
    [SerializeField] private ParticleSystem _clickEffectParticle;

    private void Start()
    {
        _currentHealth = _maxHealth;
        UpdateHealthBar();
    }

    public bool OnClick(ClickInfo clickInfo)
    {
        // 0. 피버 클릭 카운트 증가 (수동 클릭만)
        if (clickInfo.Type == EClickType.Manual && FeverManager.Instance != null)
        {
            FeverManager.Instance.AddClick();
        }

        // ========== 업그레이드 시스템 통합 ========== 

        // 1. 크리티컬 확률 (GameManager에서 가져오기)
        float criticalChance = 0.2f; // 기본 20%
        if (GameManager.Instance != null)
        {
            criticalChance = (float)GameManager.Instance.criticalChance;
        }
        bool isCritical = Random.value < criticalChance;

        // 2. 크리티컬 배수 (GameManager에서 가져오기)
        double criticalMultiplier = 2.0;
        if (GameManager.Instance != null)
        {
            criticalMultiplier = GameManager.Instance.criticalMultiplier;
        }

        // 3. 데미지 적용 (피버 배율 적용)
        double finalDamage = clickInfo.Damage;
        if (FeverManager.Instance != null)
        {
            finalDamage *= FeverManager.Instance.GetDamageMultiplier();
        }

        // 4. 크리티컬이면 배수 적용
        if (isCritical)
        {
            finalDamage *= criticalMultiplier;
        }

        // ==========================================

        // Floating Text 표시
        if (FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ShowDamage(clickInfo.Position, finalDamage, isCritical);
        }

        _currentHealth -= finalDamage;

        // 사과 점수 추가
        double appleScore = finalDamage;
        GameManager.Instance.AddApples(appleScore);

        // 체력바 UI 업데이트
        UpdateHealthBar();

        // 사과 떨어뜨리기 (수동 클릭만)
        if (clickInfo.Type == EClickType.Manual)
        {
            DropApple(clickInfo.Position);

            // 피버 중이면 사과 2배 드롭
            if (FeverManager.Instance != null && FeverManager.Instance.IsFeverActive)
            {
                DropApple(clickInfo.Position);
            }
        }

        // 피드백 실행
        PlayFeedbacks(clickInfo);

        // 파티클 효과
        PlayParticleEffects(clickInfo);

        // 나무가 죽었는지 체크
        if (_currentHealth <= 0)
        {
            RespawnTree();
        }

        return true;
    }

    private void DropApple(Vector2 clickPosition)
    {
        if (_applePrefab == null) return;

        GameObject prefabToSpawn = _applePrefab;

        if (FeverManager.Instance != null && FeverManager.Instance.IsFeverActive)
        {
            prefabToSpawn = (_goldApplePrefab != null) ? _goldApplePrefab : _applePrefab;
        }

        if (prefabToSpawn == null) return;

        Transform spawnPoint = GetClosestSpawnPoint(clickPosition);

        if (spawnPoint != null)
        {
            GameObject apple = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);

            Rigidbody2D rb = apple.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dropDirection = Vector2.down + new Vector2(Random.Range(-0.5f, 0.5f), 0);
                rb.AddForce(dropDirection * _appleDropForce, ForceMode2D.Impulse);
            }

            Destroy(apple, 2f);
        }
    }

    private Transform GetClosestSpawnPoint(Vector2 clickPosition)
    {
        if (_appleSpawnPoints == null || _appleSpawnPoints.Length == 0)
            return transform;

        Transform closest = _appleSpawnPoints[0];
        float minDistance = Vector2.Distance(clickPosition, closest.position);

        foreach (Transform point in _appleSpawnPoints)
        {
            float distance = Vector2.Distance(clickPosition, point.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = point;
            }
        }

        return closest;
    }

    private void PlayFeedbacks(ClickInfo clickInfo)
    {
        var feedbacks = GetComponentsInChildren<IFeedback>();
        foreach (var feedback in feedbacks)
        {
            feedback.Play(clickInfo);
        }
    }

    private void PlayParticleEffects(ClickInfo clickInfo)
    {
        if (_clickEffectParticle != null && clickInfo.Type == EClickType.Manual)
        {
            _clickEffectParticle.transform.position = clickInfo.Position;
            _clickEffectParticle.Play();
        }

        if (_leafParticle != null && Random.value > 0.7f)
        {
            _leafParticle.Play();
        }
    }

    private void UpdateHealthBar()
    {
        double healthPercent = GetHealthPercent();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTreeHealth(healthPercent);
        }
    }

    private void RespawnTree()
    {
        _currentHealth = _maxHealth;
        UpdateHealthBar();
        GameManager.Instance.OnTreeRespawn();

        Debug.Log($"{_treeName} 리스폰!");
    }

    public double GetHealthPercent()
    {
        return (_currentHealth / _maxHealth) * 100f;
    }
}