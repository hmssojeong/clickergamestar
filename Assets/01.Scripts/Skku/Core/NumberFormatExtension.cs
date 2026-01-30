using Unity.VisualScripting;
using UnityEngine;

public static class NumberFormatExtension
{
    // 확장 메서드
    // 이미 존재하는 클래스에 메서드를 추가하는 C#의 독특한 기능

    public static string ToFormattedString(this double value)
    {
        return CurrencyFormatter.Format(value);
    }
}
