using UnityEngine;

public interface IStudentRepository
{
    public void Save(string name, StudentSaveData saveData);

    public StudentSaveData Load(string name);
}
