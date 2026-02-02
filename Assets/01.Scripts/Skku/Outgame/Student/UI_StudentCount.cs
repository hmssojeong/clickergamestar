using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UI_StudentCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _studentCountTextUI;

    private void Start()
    {
        StudentManager.Instance.OnDataChanged += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        var students = StudentManager.Instance.GetAll();
        int attendanceCount = students.Count(s => s.IsAttendance == true);
        int totalCount = students.Count;

        _studentCountTextUI.text = $"{attendanceCount}/{totalCount}";
    }
}
