using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _cursorSprite;
    [SerializeField] private float _cursorSize = 0.08f;

    private Camera _mainCamera;

    private void Start()
    {
        Cursor.visible = false;
        _mainCamera = Camera.main;

        if (_cursorSprite != null)
        {
            _cursorSprite.transform.localScale = Vector3.one * _cursorSize;
        }
    }

    private void Update()
    {
        if (_cursorSprite != null && _mainCamera != null)
        {
            Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            _cursorSprite.transform.position = mousePos;
        }
    }
}

    //void Update()
    //{
    //    // 마우스의 월드 좌표를 계산하여 오브젝트 위치를 갱신합니다.
    //    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //    transform.position = mousePos;

    //    // 클릭 시 피드백 (약간 눌리는 느낌)
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        transform.localScale = Vector3.one * (_cursorScale * 0.8f);
    //    }

    //    if (Input.GetMouseButtonUp(0))
    //    {
    //        transform.localScale = Vector3.one * _cursorScale;
    //    }
    //}