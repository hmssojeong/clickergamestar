using UnityEngine;

public class ClickTarget : MonoBehaviour, Clickable
{
    [SerializeField] private string _name;


    public bool OnClick(ClickInfo clickInfo)
    {
        // S: 한 클래스는 하나의 역할/책임만 가지자
        Debug.Log($"{_name}: 나무클릭");

        var feedbacks = GetComponentsInChildren<IFeedback>();
        foreach (var feedback in feedbacks)
        {
            feedback.Play(clickInfo);
        }

        return true;
    }

    private void EffectFeedback()
    {

    }

    private void AnimationFeedback()
    {

    }
}