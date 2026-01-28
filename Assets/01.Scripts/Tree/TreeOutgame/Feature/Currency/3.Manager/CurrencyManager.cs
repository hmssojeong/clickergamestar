using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;

    public static event Action OnDataChanged;

    // 재화 데이터들 (배열로 관리)
    private double[] _currencies = new double[(int)ECurrencyType.Apple];

    // 저장소
    private LocalCurrencyRepository _repository;

    private void Awake()
    {
        instance = this;
    }

    // 0. 재화 조회
    public double Get(ECurrencyType currencyType)
    {
        return _currencies[(int)currencyType];
    }

    // 저장
    private void Save()
    {
        // 저장하는 방식
        // 1. PlayerPrefs + double -> string
        // 2. PlayerPrefs + double -> json
        // 3. CSV / Json으로 저장해주세요.
        // 4. 서버에 저장합시다 // DB에 저장합시다.
        // 5. 플랫폼에 따라 다르게 저장: 유니티에서는 3번, 빌드하고 나면 4번으로 저장되게 해주세요.
        // 6. Save 호출하면 Save가 더이상 호출되지 않은지 0.6초가 지나면 세이브되게...
        for(int i=0; i<(int)ECurrencyType.Apple; i++)
        { 
            var type = (ECurrencyType)i;
            PlayerPrefs.SetString("Apple", _currencies[(int)ECurrencyType.Apple].ToString("G17"));
        }
    }

    // 로드
    private void Load()
    {
        for (int i = 0; i < (int)ECurrencyType.Apple; i++)
        {
            if(PlayerPrefs.HasKey(i.ToString()))
            { 
                _currencies[i] = double.Parse(PlayerPrefs.GetString(i.ToString(), "0"));
            }
        }
    }
}
