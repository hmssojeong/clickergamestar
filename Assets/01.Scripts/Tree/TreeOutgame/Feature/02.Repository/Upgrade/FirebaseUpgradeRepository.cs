#if !UNITY_WEBGL || UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using UnityEngine;

public class FirebaseUpgradeRepository : IUpgradeRepository
{
    private readonly string UPGRADE_COLLECTION_NAME = "Upgrade";
    private readonly FirebaseAuth _auth;
    private readonly FirebaseFirestore _db;
    private string _userId;

    public FirebaseUpgradeRepository()
    {
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
            Debug.Log($"초기화 완료 - UserID: {_userId}");
        }
    }

    public async UniTaskVoid Save(UpgradeSaveData saveData)
    {
        try
        {
            string email = _auth.CurrentUser.Email;

            await _db.Collection(email).Document(UPGRADE_COLLECTION_NAME).SetAsync(saveData);
            Debug.Log($"재화 저장 성공 - UserID: {email}");
        }
        catch (Exception e)
        {
            Debug.LogError($"재화 저장 실패: {e.Message}");
        }
    }

    public async UniTask<UpgradeSaveData> Load()
    {
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
                    Debug.Log($"업그레이드 로드 성공 - UserID: {email}");
                    return data;
                }
            }

            Debug.LogWarning($"저장된 데이터가 없어 기본값을 반환합니다 - UserID: {email}");
            return UpgradeSaveData.Default;
        }
        catch (Exception e)
        {
            Debug.LogError($"업그레이드 로드 에러: {e.Message}");
            return UpgradeSaveData.Default;
        }
    }
}
#endif