using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using System;
using UnityEngine;

public class FirebaseCurrencyRepository : ICurrencyRepository
{
    private readonly string CURRENCY_COLLECTION_NAME = "Currency";
    private readonly FirebaseFirestore _db;
    private string _userId;

    public FirebaseCurrencyRepository()
    {
        _db = FirebaseFirestore.DefaultInstance;
        
        // AccountManager에서 현재 로그인된 사용자 이메일 가져오기
        if (AccountManager.Instance != null && AccountManager.Instance.IsLogin)
        {
            _userId = AccountManager.Instance.Email;
        }
        else
        {
            // 로그인 안된 경우 Firebase Auth에서 직접 가져오기
            var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser != null)
            {
                _userId = auth.CurrentUser.Email;
            }
            else
            {
                Debug.LogWarning("[FirebaseCurrencyRepository] 로그인된 사용자가 없습니다. 데이터를 저장/로드할 수 없습니다.");
            }
        }
        
        if (!string.IsNullOrEmpty(_userId))
        {
            Debug.Log($"[FirebaseCurrencyRepository] 초기화 완료 - UserID: {_userId}");
        }
    }

    public async UniTaskVoid Save(CurrencySaveData saveData)
    {
        if (string.IsNullOrEmpty(_userId))
        {
            Debug.LogError("[FirebaseCurrencyRepository] 저장 실패: 로그인된 사용자가 없습니다.");
            return;
        }

        try
        {
            await _db.Collection(CURRENCY_COLLECTION_NAME).Document(_userId).SetAsync(saveData);
            Debug.Log($"[FirebaseCurrencyRepository] 재화 저장 성공 - UserID: {_userId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseCurrencyRepository] 재화 저장 실패: {e.Message}");
        }
    }

    public async UniTask<CurrencySaveData> Load()
    {
        if (string.IsNullOrEmpty(_userId))
        {
            Debug.LogError("[FirebaseCurrencyRepository] 로드 실패: 로그인된 사용자가 없습니다.");
            return CurrencySaveData.Default;
        }

        try
        {
            DocumentSnapshot snapshot = await _db.Collection(CURRENCY_COLLECTION_NAME).Document(_userId).GetSnapshotAsync();

            if (snapshot.Exists)
            {
                CurrencySaveData data = snapshot.ConvertTo<CurrencySaveData>();
                if (data != null)
                {
                    Debug.Log($"[FirebaseCurrencyRepository] 재화 로드 성공 - UserID: {_userId}");
                    return data;
                }
            }
            else
            {
                Debug.LogWarning($"[FirebaseCurrencyRepository] 저장된 데이터가 없습니다. 기본값 사용 - UserID: {_userId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseCurrencyRepository] 재화 로드 실패: {e.Message}");
        }

        return CurrencySaveData.Default;
    }
}