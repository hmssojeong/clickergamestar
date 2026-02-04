using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using System;
using UnityEngine;

public class FirebaseCurrencyRepository : ICurrencyRepository
{
    private readonly string CURRENCY_COLLECTION_NAME = "Currency";
    private readonly string _userId; 
    private readonly FirebaseFirestore _db;

    public FirebaseCurrencyRepository()
    {
        _db = FirebaseFirestore.DefaultInstance;
    }

    public async UniTaskVoid Save(CurrencySaveData saveData)
    {
        if (string.IsNullOrEmpty(_userId))
        {
            return;
        }

        try
        {
            await _db.Collection(CURRENCY_COLLECTION_NAME).Document(_userId).SetAsync(saveData);
        }
        catch (Exception e)
        {
            Debug.LogError($"재화 저장 실패: {e.Message}");
        }
    }

    public async UniTask<CurrencySaveData> Load()
    {
        if (string.IsNullOrEmpty(_userId))
        {
            return CurrencySaveData.Default;
        }

        try
        {
            DocumentSnapshot snapshot = await _db.Collection(CURRENCY_COLLECTION_NAME).Document(_userId).GetSnapshotAsync();

            if (snapshot.Exists)
            {
                CurrencySaveData data = snapshot.ConvertTo<CurrencySaveData>();
                if (data != null) return data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"재화 로드 실패: {e.Message}");
        }

        return CurrencySaveData.Default;
    }
}