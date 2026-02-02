using UnityEngine;

public class Student
{
    public string Name { get; private set; }
    public int Age { get; private set; }
    public bool IsAttendance { get; private set; }

    public Student(string name, int age)
    {
        if(string.IsNullOrEmpty(name))
        {
            throw new System.ArgumentNullException("이름은 비어있을 수 없습니다.");
        }

        if(age < 19)
        {
            throw new System.ArgumentOutOfRangeException("성인만 참여할 수 있습니다.");
        }

        Name = name;
        Age = age;
    }

    public void CheckAttendance(bool isAttendance)
    {
        IsAttendance = isAttendance;
    }
}
