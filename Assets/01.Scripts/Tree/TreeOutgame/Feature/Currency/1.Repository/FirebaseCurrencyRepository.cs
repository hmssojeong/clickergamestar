using UnityEngine;

public class FirebaseCurrencyRepository : ICurrencyRepository
{
    public void Save(CurrencySaveData saveData)
    {
        // 파이어베이스 데이터를 서버에 저장하는 플랫폼

        // Todo: 다음주에 파이어베이스를 배우면 채울것이다.
    }

    public CurrencySaveData Load()
    {
        // Todo: 다음주에 파이어베이스를 배우면 채울것이다.


        return CurrencySaveData.Default;
    }
}
