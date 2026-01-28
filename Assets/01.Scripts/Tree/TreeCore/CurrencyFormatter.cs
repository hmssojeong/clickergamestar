using UnityEngine;

public static class CurrencyFormatter
{
    private static readonly string[] _suffixes =
    {
        "", "K", "M", "B", "T",
        "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj",
        "ak", "al", "am", "an", "ao", "ap", "aq", "ar", "as", "at",
        "au", "av", "aw", "ax", "ay", "az",
        "ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj",
        "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt",
        "bu", "bv", "bw", "bx", "by", "bz"
    };

    public static string Format(double value)
    {
        if (value < 1000) return value.ToString("N0");

        int suffixIndex = 0;
        double tempValue = value;

        while (tempValue >= 1000 && suffixIndex < _suffixes.Length - 1)
        {
            tempValue /= 1000;
            suffixIndex++;
        }

        // 家荐痢 贸府 肺流
        if (tempValue >= 100) return $"{tempValue:F0}{_suffixes[suffixIndex]}";
        if (tempValue >= 10) return $"{tempValue:F1}{_suffixes[suffixIndex]}";
        return $"{tempValue:F2}{_suffixes[suffixIndex]}";
    }
}