using DG.Tweening;
using UnityEngine;

public class GoldApple : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _rotationSpeed = 200f;

    [Header("Animation")]
    [SerializeField] private float _spawnScaleDuration = 0.3f;
    [SerializeField] private Ease _spawnEase = Ease.OutBack;

    private Rigidbody2D _rb;
    private bool _isCollected = false;

    private Vector3 _targetScale;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _targetScale = transform.localScale;
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // 스폰 애니메이션
        transform.localScale = Vector3.zero;
        transform.DOScale(_targetScale, _spawnScaleDuration).SetEase(_spawnEase);

        // 랜덤 회전 추가
        if (_rb != null)
        {
            _rb.angularVelocity = Random.Range(-_rotationSpeed, _rotationSpeed);
        }
    }

    private void Update()
    {
        // 화면 밖으로 나가면 삭제
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 바스켓이나 수집 영역에 닿으면 수집
        if (other.CompareTag("Collector") && !_isCollected)
        {
            CollectApple();
        }
    }

    // 사과를 수집합니다
    private void CollectApple()
    {
        if (_isCollected) return;

        _isCollected = true;

        // 수집 애니메이션
        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);

        // 약간의 보너스 효과
        GameManager.Instance.AddApples(10);

        // 파티클이나 사운드 재생
        // PlayCollectEffect();

        Destroy(gameObject, 0.3f);
    }

    // 사과를 클릭했을 때 (보너스 점수)
    private void OnMouseDown()
    {
        if (!_isCollected)
        {
            // 클릭 시 보너스 점수
            GameManager.Instance.AddApples(15);

            // 수집 효과
            CollectApple();
        }
    }
}