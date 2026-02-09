#if !UNITY_WEBGL || UNITY_EDITOR
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Firestore;
using Firebase.Auth;
#endif

public class FirebaseCurrencyRepository : ICurrencyRepository
{
    private readonly string CURRENCY_COLLECTION_NAME = "Currency";
    private readonly string PLAYERPREFS_KEY = "CurrencySaveData";
    
#if !UNITY_WEBGL || UNITY_EDITOR
    private readonly FirebaseFirestore _db;
    private readonly FirebaseAuth _auth;
#endif
    private string _userId;

    public FirebaseCurrencyRepository()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
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
            Debug.Log($"FirebaseCurrencyRepository 초기화 완료 - UserID: {_userId}");
        }
#else
        Debug.Log("FirebaseCurrencyRepository WebGL 모드로 초기화 (PlayerPrefs 사용)");
#endif
    }

    public async UniTaskVoid Save(CurrencySaveData saveData)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        try
        {
            string email = _auth.CurrentUser.Email;

            await _db.Collection(email).Document(CURRENCY_COLLECTION_NAME).SetAsync(saveData);
            Debug.Log($"재화 저장 성공 (Firebase) - UserID: {email}");
        }
        catch (Exception e)
        {
            Debug.LogError($"재화 저장 실패 (Firebase): {e.Message}");
        }
#else
        try
        {
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(PLAYERPREFS_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("재화 저장 성공 (PlayerPrefs)");
        }
        catch (Exception e)
        {
            Debug.LogError($"재화 저장 실패 (PlayerPrefs): {e.Message}");
        }
        
        await UniTask.CompletedTask;
#endif
    }

    public async UniTask<CurrencySaveData> Load()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
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
                Debug.Log($"재화 로드 성공 (Firebase) - UserID: {email}");
                return data;
            }

            return CurrencySaveData.Default;
        }
        catch (Exception e)
        {
            Debug.LogError($"재화 로드 실패 (Firebase): {e.Message}");
        }

        return CurrencySaveData.Default;
#else
        try
        {
            if (PlayerPrefs.HasKey(PLAYERPREFS_KEY))
            {
                string json = PlayerPrefs.GetString(PLAYERPREFS_KEY);
                CurrencySaveData data = JsonUtility.FromJson<CurrencySaveData>(json);
                
                if (data != null)
                {
                    Debug.Log("재화 로드 성공 (PlayerPrefs)");
                    return data;
                }
            }

            Debug.LogWarning("저장된 데이터가 없어 기본값을 반환합니다 (PlayerPrefs)");
            return CurrencySaveData.Default;
        }
        catch (Exception e)
        {
            Debug.LogError($"재화 로드 에러 (PlayerPrefs): {e.Message}");
            return CurrencySaveData.Default;
        }
#endif
    }
}
#endif