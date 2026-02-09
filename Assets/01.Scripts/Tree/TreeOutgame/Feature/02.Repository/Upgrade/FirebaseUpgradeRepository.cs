#if !UNITY_WEBGL || UNITY_EDITOR
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Auth;
using Firebase.Firestore;
#endif

public class FirebaseUpgradeRepository : IUpgradeRepository
{
    private readonly string UPGRADE_COLLECTION_NAME = "Upgrade";
    private readonly string PLAYERPREFS_KEY = "UpgradeSaveData";
    
#if !UNITY_WEBGL || UNITY_EDITOR
    private readonly FirebaseAuth _auth;
    private readonly FirebaseFirestore _db;
#endif
    private string _userId;

    public FirebaseUpgradeRepository()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _auth = FirebaseAuth.DefaultInstance;
        _db = FirebaseFirestore.DefaultInstance;

        if(_auth.CurrentUser != null)
        {
            _userId = _auth.CurrentUser.Email;
        }

        // AccountManager를 통한 초기 ID 세팅 (CurrencyRepository 방식 적용)
        if (AccountManager.Instance != null && AccountManager.Instance.IsLogin)
        {
            _userId = AccountManager.Instance.Email;
        }
        else if (_auth.CurrentUser != null)
        {
            _userId = _auth.CurrentUser.Email;
        }

        if (!string.IsNullOrEmpty(_userId))
        {
            Debug.Log($"FirebaseUpgradeRepository 초기화 완료 - UserID: {_userId}");
        }
#else
        Debug.Log("FirebaseUpgradeRepository WebGL 모드로 초기화 (PlayerPrefs 사용)");
#endif
    }

    public async UniTaskVoid Save(UpgradeSaveData saveData)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        try
        {
            string email = _auth.CurrentUser.Email;

            await _db.Collection(email).Document(UPGRADE_COLLECTION_NAME).SetAsync(saveData);
            Debug.Log($"업그레이드 저장 성공 (Firebase) - UserID: {email}");
        }
        catch (Exception e)
        {
            Debug.LogError($"업그레이드 저장 실패 (Firebase): {e.Message}");
        }
#else
        try
        {
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(PLAYERPREFS_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("업그레이드 저장 성공 (PlayerPrefs)");
        }
        catch (Exception e)
        {
            Debug.LogError($"업그레이드 저장 실패 (PlayerPrefs): {e.Message}");
        }
        
        await UniTask.CompletedTask;
#endif
    }

    public async UniTask<UpgradeSaveData> Load()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        try
        {
            var user = _auth.CurrentUser;
            if (user == null)
            {
                Debug.LogWarning("로드 실패: 로그인된 사용자가 없습니다.");
                return UpgradeSaveData.Default;
            }

            string email = user.Email;
            DocumentSnapshot snapshot = await _db.Collection(email).Document(UPGRADE_COLLECTION_NAME).GetSnapshotAsync();

            if (snapshot.Exists)
            {
                UpgradeSaveData data = snapshot.ConvertTo<UpgradeSaveData>();
                if (data != null)
                {
                    Debug.Log($"업그레이드 로드 성공 (Firebase) - UserID: {email}");
                    return data;
                }
            }

            Debug.LogWarning($"저장된 데이터가 없어 기본값을 반환합니다 (Firebase) - UserID: {email}");
            return UpgradeSaveData.Default;
        }
        catch (Exception e)
        {
            Debug.LogError($"업그레이드 로드 에러 (Firebase): {e.Message}");
            return UpgradeSaveData.Default;
        }
#else
        try
        {
            if (PlayerPrefs.HasKey(PLAYERPREFS_KEY))
            {
                string json = PlayerPrefs.GetString(PLAYERPREFS_KEY);
                UpgradeSaveData data = JsonUtility.FromJson<UpgradeSaveData>(json);
                
                if (data != null)
                {
                    Debug.Log("업그레이드 로드 성공 (PlayerPrefs)");
                    return data;
                }
            }

            Debug.LogWarning("저장된 데이터가 없어 기본값을 반환합니다 (PlayerPrefs)");
            return UpgradeSaveData.Default;
        }
        catch (Exception e)
        {
            Debug.LogError($"업그레이드 로드 에러 (PlayerPrefs): {e.Message}");
            return UpgradeSaveData.Default;
        }
#endif
    }
}
#endif