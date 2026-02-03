using UnityEngine;

public class Student : IReadonlyStudent
{
    public string Name { get; private set; }
    public int Age { get; private set; }
    public bool IsAttendance { get; private set; }

    public Student(StudentSpecData data, bool attendance)
    {
        if(string.IsNullOrEmpty(data.Name))
        {
            throw new System.ArgumentNullException("이름은 비어있을 수 없습니다.");
        }

        if(data.Age < 19)
        {
            throw new System.ArgumentOutOfRangeException("성인만 참여할 수 있습니다.");
        }

        Name = data.Name;
        Age = data.Age;
        IsAttendance = attendance;
    }

    public void CheckAttendance(bool isAttendance)
    {
        IsAttendance = isAttendance;
    }
}
