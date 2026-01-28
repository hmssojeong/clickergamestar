using UnityEngine;

public interface ICurrencyRepository
{
    // 저장소가 가져야할 약속!
    public void Save(CurrencySaveData saveData);
    public CurrencySaveData Load();
}
