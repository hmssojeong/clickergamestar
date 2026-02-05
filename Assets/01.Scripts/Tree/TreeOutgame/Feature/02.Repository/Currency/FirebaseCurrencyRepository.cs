using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using Firebase.Auth;
using System;
using UnityEngine;

public class FirebaseCurrencyRepository : ICurrencyRepository
{
    private readonly string CURRENCY_COLLECTION_NAME = "Currency";
    private readonly FirebaseFirestore _db;
    private string _userId;
    private readonly FirebaseAuth _auth;

    public FirebaseCurrencyRepository()
    {
        _db = FirebaseFirestore.DefaultInstance;
        _auth = FirebaseAuth.DefaultInstance;
        
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
                Debug.LogWarning("로그인된 사용자가 없습니다. 데이터를 저장/로드할 수 없습니다.");
            }
        }
        
        if (!string.IsNullOrEmpty(_userId))
        {
            Debug.Log($"초기화 완료 - UserID: {_userId}");
        }
    }

    public async UniTaskVoid Save(CurrencySaveData saveData)
    {
        try
        {
            string email = _auth.CurrentUser.Email;

            await _db.Collection(email).Document(CURRENCY_COLLECTION_NAME).SetAsync(saveData);
            Debug.Log($"재화 저장 성공 - UserID: {email}");
        }
        catch (Exception e)
        {
            Debug.LogError($"재화 저장 실패: {e.Message}");
        }
    }

    public async UniTask<CurrencySaveData> Load()
    {
        try
        {
            var user = _auth.CurrentUser;
            if (user == null)
            {
                return CurrencySaveData.Default;
            }

            string email = user.Email;
            DocumentSnapshot snapshot = await _db.Collection(email).Document(CURRENCY_COLLECTION_NAME).GetSnapshotAsync();

            CurrencySaveData data = snapshot.ConvertTo<CurrencySaveData>();
            if (data != null)
            {
                return data;
            }

            return CurrencySaveData.Default;
        }
        catch (Exception e)
        {
            Debug.LogError($"재화 로드 실패: {e.Message}");
        }

        return CurrencySaveData.Default;
    }
}