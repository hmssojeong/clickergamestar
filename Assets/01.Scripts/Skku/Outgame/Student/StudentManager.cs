using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;

public class StudentManager : MonoBehaviour
{
    // -----------------Ω∫∆Â µ•¿Ã≈Õ
    public static StudentManager Instance { get; private set; }

    private List<Student> _students = new();

    [SerializeField] private StudentSpecTable _specTable = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _students.Add(new Student("«‘º“¡§", 29));
        _students.Add(new Student("≥ÎπŒ±’", 24));
        _students.Add(new Student("∞Ì«ˆ¡æ", 28));

        // .... 100∏Ì
    }

}
