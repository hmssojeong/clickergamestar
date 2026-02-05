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

    public FirebaseUpgradeRepository()
    {
        _auth = FirebaseAuth.DefaultInstance;
        _db = FirebaseFirestore.DefaultInstance;
    }

    public async UniTaskVoid Save(UpgradeSaveData saveData)
    {
        try
        {
            string email = _auth.CurrentUser?.Email;
            
            if (string.IsNullOrEmpty(email))
            {
                Debug.LogError("저장 실패: 로그인된 사용자가 없습니다.");
                return;
            }

            await _db.Collection(UPGRADE_COLLECTION_NAME).Document(email).SetAsync(saveData);
            Debug.Log($"업그레이드 저장 성공 - UserID: {email}");
        }
        catch (Exception e)
        {
            Debug.LogError($"업그레이드 저장 실패: {e.Message}");
        }
    }

    public async UniTask<UpgradeSaveData> Load()
    {
        try
        {
            string email = _auth.CurrentUser?.Email;
            
            if (string.IsNullOrEmpty(email))
            {
                Debug.LogError("로드 실패: 로그인된 사용자가 없습니다.");
                return UpgradeSaveData.Default;
            }

            DocumentSnapshot snapshot = await _db.Collection(UPGRADE_COLLECTION_NAME).Document(email).GetSnapshotAsync();

            if (snapshot.Exists)
            {
                UpgradeSaveData data = snapshot.ConvertTo<UpgradeSaveData>();
                if (data != null)
                {
                    Debug.Log($" 업그레이드 로드 성공 - UserID: {email}");
                    return data;
                }
            }
            else
            {
                Debug.LogWarning($"저장된 데이터가 없습니다. 기본값 사용 - UserID: {email}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"업그레이드 로드 실패: {e.Message}");
        }

        return UpgradeSaveData.Default;
    }
}