using DG.Tweening;
using UnityEngine;

public class AutoClicker : MonoBehaviour
{
    [SerializeField] private Tree _targetTree;
    [SerializeField] private float _interval = 3f;
    [SerializeField] private float _attackDistance = 0.5f;
    [SerializeField] private float _attackSpeed = 0.1f;
    [SerializeField] private int _requiredSquirrelCount = 1;

    private float _timer;
    private bool _isAttacking;
    private Vector3 _originalPos;
    private void Start()
    {
        _originalPos = transform.position;
    }

    private void Update()
    {
        if (_isAttacking || _interval <= 0f || !CanAutoClick())
        {
            return;
        }

        _timer += Time.deltaTime;
        if (_timer >= _interval)
        {
            _timer = 0f;
            ExecuteAutoClick();
        }
    }

    private void ExecuteAutoClick()
    {
        if (_targetTree == null)
        {
            return;
        }

        Vector3 targetPosition = _targetTree.transform.position;
        Vector3 direction = (targetPosition - _originalPos).normalized;
        Vector3 attackPos = _originalPos + (direction * _attackDistance);

        _isAttacking = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(ESfx.AutoClickerAttack);
        }

        Sequence attackSequence = DOTween.Sequence();
        attackSequence.Append(transform.DOMove(attackPos, _attackSpeed).SetEase(Ease.OutQuad))
            .AppendCallback(ApplyAutoHit)
            .Append(transform.DOMove(_originalPos, _attackSpeed).SetEase(Ease.InQuad))
            .OnComplete(ResetAttackState);
    }

    private bool CanAutoClick()
    {
        return GameplayManager.Instance != null &&
               GameplayManager.Instance.CanUseAutoClicker(_requiredSquirrelCount);
    }

    private void ApplyAutoHit()
    {
        if (_targetTree == null)
        {
            return;
        }

        Vector2 hitPosition = _targetTree.transform.position;
        _targetTree.OnClick(hitPosition, EClickType.Auto);
    }

    private void ResetAttackState()
    {
        _isAttacking = false;
        transform.position = _originalPos;
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
