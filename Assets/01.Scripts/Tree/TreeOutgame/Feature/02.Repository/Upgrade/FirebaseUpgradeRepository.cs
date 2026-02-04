using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using Newtonsoft.Json;
using System;
using UnityEditor.Overlays;
using UnityEngine;

public class FirebaseUpgradeRepository : IUpgradeRepository
{
    private string CURRENCY_COLLECTION_NAME = "Upgrade";

    private FirebaseAuth _auth = FirebaseAuth.DefaultInstance;
    private FirebaseFirestore _db = FirebaseFirestore.DefaultInstance;

    public async UniTaskVoid Save(UpgradeSaveData saveData)
    {
        try
        {
            string email = _auth.CurrentUser.Email;

            await _db.Collection(CURRENCY_COLLECTION_NAME).Document(email).SetAsync(saveData);
        }
        catch (Exception e)
        {
            Debug.LogError("Currency 저장 실패: " + e.Message);
        }
    }

    public async UniTask<UpgradeSaveData> Load()
    {
        try
        {
            string email = _auth.CurrentUser.Email;

            DocumentSnapshot snapshot = await _db.Collection(CURRENCY_COLLECTION_NAME).Document(email).GetSnapshotAsync();

            UpgradeSaveData data = snapshot.ConvertTo<UpgradeSaveData>();
            if (data != null)
            {
                return data;
            }

            return UpgradeSaveData.Default;
        }
        catch (Exception e)
        {
            Debug.LogError("Currency 로드 실패: " + e.Message);
        }

        return UpgradeSaveData.Default;
    }
}