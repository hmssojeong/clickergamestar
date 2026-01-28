using UnityEngine;

public class CurrencySaveData : MonoBehaviour
{
    // 재화 배열
    public double[] Currencies;

    // 재화 기본값
    public static CurrencySaveData Default => new CurrencySaveData()
    {
        Currencies = new double[(int)ECurrencyType.Apple]
    };
}
