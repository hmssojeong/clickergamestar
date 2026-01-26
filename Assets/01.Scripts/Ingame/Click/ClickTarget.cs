using UnityEngine;

public class ClickTarget : MonoBehaviour, Clickable
{
    [SerializeField] private string _name;


    public bool OnClick(ClickInfo clickInfo)
    {
        // S: 한 클래스는 하나의 역할/책임만 가지자
        Debug.Log($"{_name}: 다음부터는 늦지 않겠습니다.");

        return true;
    }

    private void EffectFeedback()
    {

    }

    private void AnimationFeedback()
    {

    }
}