using UnityEngine;

/// <summary>
/// 클릭 가능한 나무 오브젝트
/// 클릭하면 사과를 떨어뜨리고 점수를 증가시킵니다
/// </summary>
public class Tree : MonoBehaviour, Clickable
{
    [Header("Tree Settings")]
    [SerializeField] private string _treeName = "Apple Tree";
    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;

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
    }

    /// <summary>
    /// 클릭 시 호출되는 메서드
    /// </summary>
    public bool OnClick(ClickInfo clickInfo)
    {
        // 1. 데미지 적용
        _currentHealth -= clickInfo.Damage;

        // 2. 사과 점수 추가
        int appleScore = clickInfo.Damage; // 데미지만큼 사과 추가
        GameManager.Instance.AddApples(appleScore);

        // 3. 사과 떨어뜨리기 (수동 클릭만)
        if (clickInfo.Type == EClickType.Manual)
        {
            DropApple(clickInfo.Position);
        }

        // 4. 피드백 실행
        PlayFeedbacks(clickInfo);

        // 5. 파티클 효과
        PlayParticleEffects(clickInfo);

        // 6. 나무가 죽었는지 체크
        if (_currentHealth <= 0)
        {
            RespawnTree();
        }

        return true;
    }

    /// <summary>
    /// 사과를 떨어뜨립니다
    /// </summary>
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

    /// <summary>
    /// 클릭 위치에 가장 가까운 스폰 포인트 반환
    /// </summary>
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

    /// <summary>
    /// 모든 피드백을 실행합니다
    /// </summary>
    private void PlayFeedbacks(ClickInfo clickInfo)
    {
        var feedbacks = GetComponentsInChildren<IFeedback>();
        foreach (var feedback in feedbacks)
        {
            feedback.Play(clickInfo);
        }
    }

    /// <summary>
    /// 파티클 효과를 재생합니다
    /// </summary>
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

    /// <summary>
    /// 나무를 리스폰합니다
    /// </summary>
    private void RespawnTree()
    {
        _currentHealth = _maxHealth;
        
        // 나무 리스폰 이벤트 (보너스 점수, 특별 효과 등)
        GameManager.Instance.OnTreeRespawn();
        
        Debug.Log($"{_treeName} 리스폰!");
    }

    /// <summary>
    /// 현재 체력 퍼센트 반환 (UI 표시용)
    /// </summary>
    public float GetHealthPercent()
    {
        return (float)_currentHealth / _maxHealth;
    }
}
