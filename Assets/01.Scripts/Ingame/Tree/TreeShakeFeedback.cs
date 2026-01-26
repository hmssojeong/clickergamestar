using UnityEngine;
using DG.Tweening;

/// <summary>
/// 나무가 클릭될 때 스케일이 변하는 효과
/// </summary>
public class TreeShakeFeedback : MonoBehaviour, IFeedback
{
    [SerializeField] private Transform _treeTransform;
    [SerializeField] private float _scaleDuration = 0.15f;
    [SerializeField] private float _scaleAmount = 0.95f; // 95%로 줄어듦
    [SerializeField] private Ease _scaleEase = Ease.OutQuad;

    private Vector3 _originalScale;

    private void Awake()
    {
        if (_treeTransform == null)
            _treeTransform = transform;

        _originalScale = _treeTransform.localScale;
    }

    public void Play(ClickInfo clickInfo)
    {
        // 수동 클릭만 스케일 효과
        if (clickInfo.Type == EClickType.Manual)
        {
            ScaleTree();
        }
    }

    /// <summary>
    /// 나무를 작아졌다 커지게 합니다
    /// </summary>
    private void ScaleTree()
    {
        // 기존 트윈 중단
        _treeTransform.DOKill();

        // 스케일 시퀀스: 작아졌다 → 원래 크기로
        Sequence scaleSequence = DOTween.Sequence();
        scaleSequence.Append(_treeTransform.DOScale(_originalScale * _scaleAmount, _scaleDuration / 2).SetEase(_scaleEase));
        scaleSequence.Append(_treeTransform.DOScale(_originalScale, _scaleDuration / 2).SetEase(_scaleEase));
    }
}