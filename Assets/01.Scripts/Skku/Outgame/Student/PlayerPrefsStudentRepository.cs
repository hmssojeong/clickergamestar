using UnityEngine;

public class PlayerPrefsStudentRepository : IStudentRepository
{
    public void Save(string name, StudentSaveData saveData)
    {
        PlayerPrefs.SetString(name, JsonUtility.ToJson(saveData));
    }

    public StudentSaveData Load(string name)
    {
        if (!PlayerPrefs.HasKey(name))
        {
            return null;
        }

        return JsonUtility.FromJson<StudentSaveData>(PlayerPrefs.GetString(name));
    }
}
