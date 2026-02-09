#if !UNITY_WEBGL || UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseInitializer Instance {get; private set;}

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Fire-and-Forget 패턴
        // - 비동기 작업을 시작만하고, 결과를 기다리지는 않겠다.
        InitFirebase().Forget();
        // 아래쪽에서 실행할 코드가 없기 때문에 await를 안해도된다.
    }

    private async UniTask InitFirebase()
    {
        DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();

        try
        {
            if (status == DependencyStatus.Available)
            {
                Debug.Log("Firebase 초기화 성공!");
            }
        }
        catch (FirebaseException e)
        {
            Debug.LogError("Firebase 초기화 실패: " + e.Message);
        }
        catch (Exception e)
        {
            Debug.LogError("실패: " + e.Message);
        }
    }
}
#endif
