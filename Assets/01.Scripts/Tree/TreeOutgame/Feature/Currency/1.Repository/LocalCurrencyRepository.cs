using Newtonsoft.Json;
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
    private const string SaveKey = "GameUserData";

    public void Save(CurrencySaveData saveData)
    {
        string json = JsonConvert.SerializeObject(saveData);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public CurrencySaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return CurrencySaveData.Default;

        string json = PlayerPrefs.GetString(SaveKey);
        return JsonConvert.DeserializeObject<CurrencySaveData>(json);
    }
}