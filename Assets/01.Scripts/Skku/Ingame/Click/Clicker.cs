using UnityEngine;

public class Clicker : MonoBehaviour
{
    [SerializeField] private int _damage = 10;

    private void Update()
    {
        // 1. 마우스 클릭을 감지한다.
        if (Input.GetMouseButtonDown(0))
        {
            // 2. 마우스 좌표를 구한다.
            // 마우스 좌표계는 스크린 좌표계
            Vector2 mousePos = Input.mousePosition;
            Click(mousePos);
        }
    }

    private void Click(Vector2 mousePos)
    {
        // 마우스의 스크린 좌표계를 월드 좌표계로 바꿔줄 필요가 있다.
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // 3. 마우스 좌표로 가상의 레이저를 쏴서 그 레이저가 클릭타겟과 충돌했는지 체크
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit == true)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(ESfx.Click);
            }

            Clickable clickable = hit.collider.GetComponent<Clickable>();
            // 누가 클릭했는지             (ManualClick, AutoClick)
            // 어느정도의 강도로 클릭했는지 (int)

            ClickInfo clickInfo = new ClickInfo
            {
                Type = EClickType.Manual,
                Damage = _damage,
                Position = hit.point,
            };

            clickable?.OnClick(clickInfo);
        }
    }
}