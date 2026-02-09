using UnityEngine;
using System.Collections.Generic;

public class UI_StudentPanel : MonoBehaviour
{
    [SerializeField] List<UI_StudentItem> _studentUIItems;

    private void Start()
    {
        StudentManager.Instance.OnDataChanged += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        List<IReadonlyStudent> students = StudentManager.Instance.GetAll();

        for(int i = 0; i< students.Count; ++i)
        {
            _studentUIItems[i].Refresh(students[i]);
        }
    }
}
