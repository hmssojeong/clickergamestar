using DG.Tweening;
using TMPro;
using UnityEngine;

public class HUD_Currency : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private float _punchScale = 0.2f;
    [SerializeField] private float _punchDuration = 0.3f;

    private void Start()
    {
        Refresh();

        CurrencyManager.OnDataChanged += Refresh;
    }

    private void Refresh()
    {
        // 객체: 속성과 기능을 가지고 
        // 값객체 :                 + 값의 속성을 가지고 있다.
/*        Currency gold = CurrencyManager.Instance.Gold; // 3000*/
        // 최종 사용자 입장에서 double은 그냥 숫자일 뿐인지 '재화'인지 모른다..
        // 1. 규칙1. 재화는 0 미만일 수 없다. 그런데 음수가 가능해진다.
        // 2. 규칙2. 재화는 표현할때 무조건 ToFormattedString() 을 써야한다.

/*        _goldText.text = $"{gold}";*/

        _goldText.transform.DOKill();
        _goldText.transform.localScale = Vector3.one;
        _goldText.transform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, 1, 0.5f);
    }
}