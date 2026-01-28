using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _cursorSprite;
    [SerializeField] private float _cursorSize = 0.08f;
    [SerializeField] private float _cursorScale = 0.8f;

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

        if (Input.GetMouseButtonDown(0))
        {
            _cursorSprite.transform.localScale *= _cursorScale;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _cursorSprite.transform.localScale = Vector3.one * _cursorSize;
        }
    }
}
