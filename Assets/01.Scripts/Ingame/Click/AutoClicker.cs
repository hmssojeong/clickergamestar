using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class AutoClicker : MonoBehaviour
{
    // 역할: 정해진 시간 간격마다 Clickable한 친구를 때린다.
    [SerializeField] private float _interval;       // 시간 간격
    [SerializeField] private float _attackDistance = 0.5f;
    [SerializeField] private float _attackSpeed = 0.1f;

    private float _timer;
    private Vector3 _originalPos;

    private void Start()
    {
        _originalPos = transform.position;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _interval)
        {
            _timer = 0f;
            ExecuteAutoClick();
        }
    }

    private void ExecuteAutoClick()
    {
        // 1. 나무(타겟) 찾기
        GameObject target = GameObject.FindGameObjectWithTag("Clickable");

        if (target != null)
        {
            // 나무 방향으로의 방향 계산
            Vector3 direction = (target.transform.position - _originalPos).normalized;
            Vector3 attackPos = _originalPos + (direction * _attackDistance);

            // 2. 공격 애니메이션 시퀀스
            Sequence attackSeq = DOTween.Sequence();

            attackSeq.Append(transform.DOMove(attackPos, _attackSpeed).SetEase(Ease.OutQuad)) // 나무로 돌진
                     .Append(transform.DOMove(_originalPos, _attackSpeed).SetEase(Ease.InQuad)); // 제자리로 복귀

            // 3. 실제 데미지 입히기
            Clickable clickableScript = target.GetComponent<Clickable>();
            if (clickableScript != null)
            {
                ClickInfo clickInfo = new ClickInfo
                {
                    Type = EClickType.Auto,
                    Damage = GameManager.Instance.AutoDamage,
                    Position = target.transform.position
                };
                clickableScript.OnClick(clickInfo);
            }
        }
    }
}