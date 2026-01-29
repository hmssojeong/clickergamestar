using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private RectTransform _cursorRectTransform; // Image의 RectTransform
    [SerializeField] private float _cursorSize = 80f;
    [SerializeField] private float _cursorScale = 0.8f;
    [SerializeField] private float _edgePadding = 10f;

    private Vector3 _originalScale;

    private void Start()
    {
        Cursor.visible = false;

        if (_cursorRectTransform != null)
        {
            _originalScale = Vector3.one * _cursorSize;
            _cursorRectTransform.localScale = _originalScale;
        }

        // Raycast Target 비활성화
        Image img = _cursorRectTransform.GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = false;
        }
    }

    private void Update()
    {
        if (_cursorRectTransform != null)
        {
            // 마우스 위치를 화면 범위 내로 제한
            Vector3 mousePos = Input.mousePosition;

            mousePos.x = Mathf.Clamp(mousePos.x, _edgePadding, Screen.width - _edgePadding);
            mousePos.y = Mathf.Clamp(mousePos.y, _edgePadding, Screen.height - _edgePadding);

            _cursorRectTransform.position = mousePos;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _cursorRectTransform.localScale = _originalScale * _cursorScale;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _cursorRectTransform.localScale = _originalScale;
        }
    }
}