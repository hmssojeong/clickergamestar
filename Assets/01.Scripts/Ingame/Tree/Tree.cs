using UnityEngine;

public class Tree : MonoBehaviour, Clickable
{
    [Header("Tree Settings")]
    [SerializeField] private string _treeName = "Apple Tree";
    [SerializeField] private double _maxHealth = 100;
    private double _currentHealth;

    [Header("Apple Drop Settings")]
    [SerializeField] private GameObject _applePrefab;
    [SerializeField] private Transform[] _appleSpawnPoints; // 사과가 떨어지는 위치들
    [SerializeField] private float _appleDropForce = 5f;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem _leafParticle; // 나뭇잎 파티클
    [SerializeField] private ParticleSystem _clickEffectParticle; // 클릭 이펙트

    private void Start()
    {
        _currentHealth = _maxHealth;

        // 초기 체력바 업데이트
        UpdateHealthBar();
    }

    public bool OnClick(ClickInfo clickInfo)
    {
        // 0. 피버 클릭 카운트 증가 (수동 클릭만) ⭐ 추가!
        if (clickInfo.Type == EClickType.Manual && FeverManager.Instance != null)
        {
            FeverManager.Instance.AddClick();
        }

        //크리티컬 판정 로직 추가
        float criticalChance = 0.2f; // 20%확률
        bool isCritical = Random.value < criticalChance;

        // 1. 데미지 적용 (피버 배율 적용)
        double finalDamage = clickInfo.Damage;
        if (FeverManager.Instance != null)
        {
            finalDamage *= FeverManager.Instance.GetDamageMultiplier();
        }

        //크리티컬이면 데미지 2배 적용
        if (isCritical)
        {
            finalDamage *= 2.0;
        }

        if (FloatingTextManager.Instance != null)
        {
            // 세 번째 인자로 isCritical을 전달합니다.
            FloatingTextManager.Instance.ShowDamage(clickInfo.Position, finalDamage, isCritical);
        }

        _currentHealth -= finalDamage;

        // 2. 사과 점수 추가 (배율 적용된 데미지)
        double appleScore = finalDamage;
        GameManager.Instance.AddApples(appleScore);

        // 3. 체력바 UI 업데이트
        UpdateHealthBar();

        // Floating Text (배율 적용된 데미지 표시) ⭐ 수정!
        if (clickInfo.Type == EClickType.Manual && FloatingTextManager.Instance != null)
        {
            Vector3 worldPos = clickInfo.Position;
            FloatingTextManager.Instance.ShowDamage(worldPos, finalDamage);
        }

        // 4. 사과 떨어뜨리기 (수동 클릭만)
        if (clickInfo.Type == EClickType.Manual)
        {
            DropApple(clickInfo.Position);

            // 피버 중이면 사과 2배 드롭 ⭐ 추가!
            if (FeverManager.Instance != null && FeverManager.Instance.IsFeverActive)
            {
                DropApple(clickInfo.Position);
            }
        }

        // 5. 피드백 실행
        PlayFeedbacks(clickInfo);

        // 6. 파티클 효과
        PlayParticleEffects(clickInfo);

        // 7. 나무가 죽었는지 체크
        if (_currentHealth <= 0)
        {
            RespawnTree();
        }

        return true;
    }

    private void DropApple(Vector2 clickPosition)
    {
        if (_applePrefab == null) return;

        // 클릭 위치에 가장 가까운 스폰 포인트 찾기
        Transform spawnPoint = GetClosestSpawnPoint(clickPosition);

        if (spawnPoint != null)
        {
            GameObject apple = Instantiate(_applePrefab, spawnPoint.position, Quaternion.identity);

            // 사과에 물리 적용 (Rigidbody2D가 있다면)
            Rigidbody2D rb = apple.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dropDirection = Vector2.down + new Vector2(Random.Range(-0.5f, 0.5f), 0);
                rb.AddForce(dropDirection * _appleDropForce, ForceMode2D.Impulse);
            }

            // 사과는 2초 후 자동 삭제
            Destroy(apple, 2f);
        }
    }

    // 클릭 위치에 가장 가까운 스폰 포인트 반환
    private Transform GetClosestSpawnPoint(Vector2 clickPosition)
    {
        if (_appleSpawnPoints == null || _appleSpawnPoints.Length == 0)
            return transform; // 스폰 포인트가 없으면 나무 위치 사용

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
        // 클릭 이펙트 파티클
        if (_clickEffectParticle != null && clickInfo.Type == EClickType.Manual)
        {
            _clickEffectParticle.transform.position = clickInfo.Position;
            _clickEffectParticle.Play();
        }

        // 나뭇잎 파티클 (랜덤하게)
        if (_leafParticle != null && Random.value > 0.7f)
        {
            _leafParticle.Play();
        }
    }

    // 체력바 UI를 업데이트
    private void UpdateHealthBar()
    {
        double healthPercent = GetHealthPercent();

        // UIManager에게 체력 업데이트 알림
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTreeHealth(healthPercent);
        }
    }

    // 나무를 리스폰합니다
    private void RespawnTree()
    {
        _currentHealth = _maxHealth;

        // 체력바 UI 업데이트
        UpdateHealthBar();

        // 나무 리스폰 이벤트 (보너스 점수, 특별 효과 등)
        GameManager.Instance.OnTreeRespawn();

        Debug.Log($"{_treeName} 리스폰!");
    }

    public double GetHealthPercent()
    {
        return (_currentHealth / _maxHealth) * 100f;
    }
}