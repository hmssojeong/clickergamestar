using UnityEngine;
using Cysharp.Threading.Tasks;
public interface ICurrencyRepository
{
    // 저장소가 가져야할 약속!
    public UniTaskVoid Save(CurrencySaveData saveData);
    public UniTask<CurrencySaveData> Load();
}
