using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "StudentSpecTable", menuName = "Scriptable Objects/StudentSpecTable")]
public class StudentSpecTable : ScriptableObject
{
    [SerializeField] public List<StudentSpecData> SpecDatas;
}
