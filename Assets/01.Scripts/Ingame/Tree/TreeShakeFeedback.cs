using UnityEngine;
using DG.Tweening;

/// <summary>
/// 나무가 클릭될 때 흔들리는 효과
/// </summary>
public class TreeShakeFeedback : MonoBehaviour, IFeedback
{
    [SerializeField] private Transform _treeTransform;
    [SerializeField] private float _shakeDuration = 0.3f;
    [SerializeField] private float _shakeStrength = 0.5f;
    [SerializeField] private int _shakeVibrato = 10;
    [SerializeField] private float _shakeRandomness = 90f;

    private void Awake()
    {
        if (_treeTransform == null)
            _treeTransform = transform;
    }

    public void Play(ClickInfo clickInfo)
    {
        // 수동 클릭만 흔들림 효과
        if (clickInfo.Type == EClickType.Manual)
        {
            ShakeTree();
        }
    }

    /// <summary>
    /// 나무를 흔듭니다
    /// </summary>
    private void ShakeTree()
    {
        // 기존 트윈 중단
        _treeTransform.DOKill();

        // 흔들림 효과
        _treeTransform.DOShakePosition(
            _shakeDuration,
            _shakeStrength,
            _shakeVibrato,
            _shakeRandomness,
            false,
            true
        );

        // 약간의 회전도 추가
        _treeTransform.DOShakeRotation(
            _shakeDuration,
            new Vector3(0, 0, 5f),
            _shakeVibrato,
            _shakeRandomness
        );
    }
}
