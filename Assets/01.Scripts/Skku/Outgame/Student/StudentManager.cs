using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class StudentManager : MonoBehaviour
{
    // -----------------스펙 데이터
    public static StudentManager Instance { get; private set; }

    private List<Student> _students = new();

    [SerializeField] private StudentSpecTable _specTable;

    public event Action OnDataChanged;

    private IStudentRepository _repository;

    private void Awake()
    {
        Instance = this;

        _repository = new FirebaseStudentRepository();

        foreach (StudentSpecData data in _specTable.SpecDatas)
        {
            // 불러오기
            StudentSaveData saveData = _repository.Load(data.Name);

            bool isAttendance = false;
            if(saveData != null)
            {
                isAttendance = saveData.Attendance;
            }
            Student student = new Student(data, isAttendance);

            _students.Add(student);
        }
    }

    public List<IReadonlyStudent> GetAll()
    { 
        return _students.ToList<IReadonlyStudent>();
    }

    public bool TryAttendance(string studentName)
    {
        Student student = _students.Find(student => student.Name == studentName);
        if(student.IsAttendance)
        {
            return false;
        }

        student.CheckAttendance(true);

/*        CurrencyManager.Instance.Add(ECurrencyType.Apple, 100);*/

        OnDataChanged?.Invoke();

        StudentSaveData saveData = new StudentSaveData();
        saveData.Attendance = true;

        _repository.Save(studentName, saveData);

        return true;
    }

}
