using DG.Tweening;
using UnityEngine;

public class ScaleTweeningFeedback : MonoBehaviour, IFeedback
{
    [SerializeField] private ClickTarget _owner;

    // 역할: 스케일 트위닝 피드백에 대한 로직을 담당
    public void Play()
    {
        _owner.transform.DOKill();
        _owner.transform.DOScale(1.2f, 0.3f).OnComplete(() =>
        {
            _owner.transform.localScale = Vector3.one;
        });
    }
}
