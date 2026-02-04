using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

// Firebase Database가 설치되면 자동으로 활성화됩니다
#if FIREBASE_DATABASE_AVAILABLE
using Firebase.Database;
#endif

/// <summary>
/// Firebase Realtime Database를 사용한 업그레이드 데이터 저장소
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
public class FirebaseUpgradeRepository : IUpgradeRepository
{
#if FIREBASE_DATABASE_AVAILABLE
    // ✅ Firebase Database가 설치된 경우
    private DatabaseReference _databaseRef;
    private readonly string _userId;

    public FirebaseUpgradeRepository(string userId)
    {
        _userId = userId;
        _databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        
        Debug.Log($"[FirebaseUpgradeRepository] ✅ Firebase Database 초기화 - UserID: {userId}");
    }

    public async UniTaskVoid Save(UpgradeSaveData data)
    {
        try
        {
            data.LastSaveTime = DateTime.Now.ToString("o");
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            
            await _databaseRef
                .Child("users")
                .Child(_userId)
                .Child("upgrade")
                .SetRawJsonValueAsync(json)
                .AsUniTask();
            
            Debug.Log($"[FirebaseUpgradeRepository] Firebase에 업그레이드 저장 완료 - UserID: {_userId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseUpgradeRepository] 저장 실패: {e.Message}");
            throw;
        }
    }

    public async UniTask<UpgradeSaveData> Load()
    {
        try
        {
            var dataSnapshot = await _databaseRef
                .Child("users")
                .Child(_userId)
                .Child("upgrade")
                .GetValueAsync()
                .AsUniTask();

            if (!dataSnapshot.Exists)
            {
                Debug.LogWarning($"[FirebaseUpgradeRepository] Firebase에 저장된 데이터 없음 - 기본값 반환");
                return UpgradeSaveData.Default;
            }

            string json = dataSnapshot.GetRawJsonValue();
            var data = JsonConvert.DeserializeObject<UpgradeSaveData>(json);
            
            Debug.Log($"[FirebaseUpgradeRepository] Firebase에서 업그레이드 로드 완료 - UserID: {_userId}");
            return data ?? UpgradeSaveData.Default;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseUpgradeRepository] 로드 실패: {e.Message}");
            return UpgradeSaveData.Default;
        }
    }
#else
    // ⚠️ Firebase Database가 설치되지 않은 경우 - 임시 구현
    public FirebaseUpgradeRepository(string userId)
    {
        Debug.LogWarning("[FirebaseUpgradeRepository] ⚠️ Firebase Database가 설치되지 않았습니다. 임시 버전을 사용합니다.");
        Debug.LogWarning("[FirebaseUpgradeRepository] 📦 설치 방법: https://firebase.google.com/download/unity");
    }

    public async UniTaskVoid Save(UpgradeSaveData data)
    {
        await UniTask.Yield();
        Debug.LogWarning("[FirebaseUpgradeRepository] ⚠️ Firebase Database가 설치되지 않아 저장할 수 없습니다.");
    }

    public async UniTask<UpgradeSaveData> Load()
    {
        await UniTask.Yield();
        Debug.LogWarning("[FirebaseUpgradeRepository] ⚠️ Firebase Database가 설치되지 않아 로드할 수 없습니다.");
        return UpgradeSaveData.Default;
    }
#endif
}