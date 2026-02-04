using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

// Firebase Database가 설치되면 자동으로 활성화됩니다
#if FIREBASE_DATABASE_AVAILABLE
using Firebase.Database;
#endif

/// <summary>
/// Firebase Realtime Database를 사용한 재화 데이터 저장소
/// 
/// 📦 Firebase Database 설치 방법:
/// 1. Firebase Unity SDK 다운로드: https://firebase.google.com/download/unity
/// 2. FirebaseDatabase.unitypackage를 Unity에 Import
/// 3. Unity 재시작
/// 4. 이 스크립트가 자동으로 Firebase를 사용하도록 전환됩니다
/// 
/// ⚠️ Firebase Database가 설치되지 않은 경우:
/// - 임시로 경고만 출력하는 버전이 사용됩니다
/// - 설치 후 자동으로 Firebase 버전으로 전환됩니다
/// </summary>
public class FirebaseCurrencyRepository : ICurrencyRepository
{
#if FIREBASE_DATABASE_AVAILABLE
    // ✅ Firebase Database가 설치된 경우
    private DatabaseReference _databaseRef;
    private readonly string _userId;

    public FirebaseCurrencyRepository(string userId)
    {
        _userId = userId;
        _databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        
        Debug.Log($"[FirebaseCurrencyRepository] ✅ Firebase Database 초기화 - UserID: {userId}");
    }

    public async UniTaskVoid Save(CurrencySaveData saveData)
    {
        try
        {
            saveData.LastSaveTime = DateTime.Now.ToString("o");
            string json = JsonConvert.SerializeObject(saveData);
            
            await _databaseRef
                .Child("users")
                .Child(_userId)
                .Child("currency")
                .SetRawJsonValueAsync(json)
                .AsUniTask();
            
            Debug.Log($"[FirebaseCurrencyRepository] Firebase에 재화 저장 완료 - UserID: {_userId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseCurrencyRepository] 저장 실패: {e.Message}");
            throw;
        }
    }

    public async UniTask<CurrencySaveData> Load()
    {
        try
        {
            var dataSnapshot = await _databaseRef
                .Child("users")
                .Child(_userId)
                .Child("currency")
                .GetValueAsync()
                .AsUniTask();

            if (!dataSnapshot.Exists)
            {
                Debug.LogWarning($"[FirebaseCurrencyRepository] Firebase에 저장된 데이터 없음 - 기본값 반환");
                return CurrencySaveData.Default;
            }

            string json = dataSnapshot.GetRawJsonValue();
            var data = JsonConvert.DeserializeObject<CurrencySaveData>(json);
            
            Debug.Log($"[FirebaseCurrencyRepository] Firebase에서 재화 로드 완료 - UserID: {_userId}");
            return data ?? CurrencySaveData.Default;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseCurrencyRepository] 로드 실패: {e.Message}");
            return CurrencySaveData.Default;
        }
    }
#else
    // ⚠️ Firebase Database가 설치되지 않은 경우 - 임시 구현
    public FirebaseCurrencyRepository(string userId)
    {
        Debug.LogWarning("[FirebaseCurrencyRepository] ⚠️ Firebase Database가 설치되지 않았습니다. 임시 버전을 사용합니다.");
        Debug.LogWarning("[FirebaseCurrencyRepository] 📦 설치 방법: https://firebase.google.com/download/unity");
    }

    public async UniTaskVoid Save(CurrencySaveData saveData)
    {
        await UniTask.Yield();
        Debug.LogWarning("[FirebaseCurrencyRepository] ⚠️ Firebase Database가 설치되지 않아 저장할 수 없습니다.");
    }

    public async UniTask<CurrencySaveData> Load()
    {
        await UniTask.Yield();
        Debug.LogWarning("[FirebaseCurrencyRepository] ⚠️ Firebase Database가 설치되지 않아 로드할 수 없습니다.");
        return CurrencySaveData.Default;
    }
#endif
}