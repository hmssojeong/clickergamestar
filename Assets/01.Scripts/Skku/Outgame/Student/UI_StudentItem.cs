using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StudentItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameTextUI;
    [SerializeField] private TextMeshProUGUI _ageTextUI;
    [SerializeField] private TextMeshProUGUI _attendanceTextUI;
    [SerializeField] private Button _attendanceButton;

    private IReadonlyStudent _student;

    private void Start()
    {
        _attendanceButton.onClick.AddListener(OnClickAttendance);
    }

    public void Refresh(IReadonlyStudent student)
    {
        _student = student;

        _nameTextUI.text = student.Name;
        _ageTextUI.text = student.Age.ToString();
        _attendanceTextUI.text = student.IsAttendance ? "Yes" : "No";
        _attendanceButton.interactable = !student.IsAttendance;
    }

    private void OnClickAttendance()
    {
        StudentManager.Instance.TryAttendance(_student.Name);
    }
}
