using UnityEngine;

// 데이터의 영속성(저장/불러오기)에 대한 책임은 '레포지토리'가 가지고 있다.
//                                   ㄴ 비즈니스 로직과 분리한다.

// 비즈니스 로직은 매니저에게!!!
// 저장 로직은 레포지토리에게!!!
// - 1) 코드가 깔끔해지고 유지보수가 쉬어진다.
// - 2) 0000 해진다.
// - 3) 0000 해진다.
public class LocalCurrencyRepository : ICurrencyRepository
{
    public void Save(CurrencySaveData saveData)
    {
        // 어떻게든 Save한다.
        for (int i = 0; i < (int)ECurrencyType.Apple; i++)
        {
            var type = (ECurrencyType)i;
            PlayerPrefs.SetString(type.ToString(), saveData.Currencies[i].ToString("G17"));
        }
    }

    public CurrencySaveData Load()
    {
        CurrencySaveData data = CurrencySaveData.Default;

        for (int i = 0; i < (int)ECurrencyType.Apple; i++)
        {
            if (PlayerPrefs.HasKey(i.ToString()))
            {
                data.Currencies[i] = double.Parse(PlayerPrefs.GetString(i.ToString(), "0"));
            }
        }

        return data;
    }
}