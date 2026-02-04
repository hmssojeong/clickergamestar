using UnityEngine;

public class Tree : MonoBehaviour
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

    private void OnMouseDown()
    {
        // 마우스 클릭 시 처리
        Vector2 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        OnClick(clickPosition, EClickType.Manual);
    }

    public void OnClick(Vector2 position, EClickType clickType)
    {
        // ClickInfo 생성
        var clickInfo = new ClickInfo(position, clickType);

        // GameplayManager에게 클릭 처리 위임
        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.ProcessTreeClick(clickInfo);
        }

        // 나무 체력 감소
        double damage = clickType == EClickType.Manual
            ? GameplayManager.Instance.ManualDamage
            : GameplayManager.Instance.AutoDamage;

        _currentHealth -= damage;

        // UI 업데이트
        UpdateHealthBar();

        // 사과 떨어뜨리기
        if (clickType == EClickType.Manual)
        {
            DropApple(position);
        }

        // 피드백 실행
        PlayFeedbacks(clickInfo);

        // 파티클 효과
        PlayParticleEffects(clickInfo);

        // 나무 리스폰 체크
        if (_currentHealth <= 0)
        {
            RespawnTree();
        }
    }

    private void DropApple(Vector2 clickPosition)
    {
        if (_applePrefab == null) return;

        GameObject prefabToSpawn = _applePrefab;

        // 피버 중이면 골든 애플
        if (GameplayManager.Instance != null && GameplayManager.Instance.IsFeverActive())
        {
            prefabToSpawn = (_goldApplePrefab != null) ? _goldApplePrefab : _applePrefab;
        }

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

        // UI 업데이트는 GameplayUIController가 담당
        if (GameplayUIController.Instance != null)
        {
            GameplayUIController.Instance.UpdateTreeHealth(healthPercent);
        }
    }

    private void RespawnTree()
    {
        _currentHealth = _maxHealth;
        UpdateHealthBar();

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.OnTreeRespawn();
        }

        Debug.Log($"{_treeName} 리스폰!");
    }

    public double GetHealthPercent()
    {
        return (_currentHealth / _maxHealth) * 100f;
    }
}