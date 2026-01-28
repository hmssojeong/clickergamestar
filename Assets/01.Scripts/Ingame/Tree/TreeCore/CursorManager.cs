using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Sprite _defaultGlove;
    [SerializeField] private Sprite _clickGlove;   
    [SerializeField] private float _cursorScale = 0.08f;

    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        // 기본 마우스 커서는 숨깁니다.
        Cursor.visible = false;

        transform.localScale = Vector3.one * _cursorScale;
    }

    void Update()
    {
        // 마우스의 월드 좌표를 계산하여 오브젝트 위치를 갱신합니다.
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePos;

        /*        // 클릭 피드백 (마우스 왼쪽 버튼을 누르고 있을 때)
                if (Input.GetMouseButtonDown(0))
                {
                    if (_clickGlove != null) _spriteRenderer.sprite = _clickGlove;
                    // 살짝 작아지는 효과 같은 것도 넣을 수 있습니다.
                    transform.localScale = Vector3.one * 0.9f;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _spriteRenderer.sprite = _defaultGlove;
                    transform.localScale = Vector3.one;
                }*/

        // 클릭 시 피드백 (약간 눌리는 느낌)
        if (Input.GetMouseButtonDown(0))
        {
            transform.localScale = Vector3.one * (_cursorScale * 0.8f);
        }

        if (Input.GetMouseButtonUp(0))
        {
            transform.localScale = Vector3.one * _cursorScale;
        }
    }
}