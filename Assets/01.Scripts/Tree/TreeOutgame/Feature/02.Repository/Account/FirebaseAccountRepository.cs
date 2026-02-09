#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase;
using Firebase.Auth;
#endif

public class FirebaseAccountRepository : IAccountRepository
{
#if !UNITY_WEBGL || UNITY_EDITOR
    private FirebaseAuth _auth;
#endif

    public FirebaseAccountRepository()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _auth = FirebaseAuth.DefaultInstance;
#else
        Debug.Log("FirebaseAccountRepository WebGL 모드로 초기화");
#endif
    }

public async UniTask<AccountResult> Register(string email, string password)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        try
        {
            AuthResult result = await _auth.CreateUserWithEmailAndPasswordAsync(email, password).AsUniTask();
            return new AccountResult()
            {
                Success = true,
                ErrorMessage = "아이디 생성완료했습니다."
            };
        }
        catch (Exception e)
        {
            return new AccountResult()
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
#else
        Debug.LogWarning("WebGL에서는 Firebase 회원가입을 지원하지 않습니다.");
        await UniTask.CompletedTask;
        return new AccountResult()
        {
            Success = false,
            ErrorMessage = "WebGL에서는 Firebase 회원가입을 지원하지 않습니다."
        };
#endif
    }

public async UniTask<AccountResult> Login(string email, string password)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        try
        {
            Firebase.Auth.AuthResult result = await _auth.SignInWithEmailAndPasswordAsync(email, password).AsUniTask();
            return new AccountResult()
            {
                Success = true,
            };
        }
        catch (Exception e)
        {
            return new AccountResult()
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
#else
        Debug.LogWarning("WebGL에서는 Firebase 로그인을 지원하지 않습니다.");
        await UniTask.CompletedTask;
        return new AccountResult()
        {
            Success = false,
            ErrorMessage = "WebGL에서는 Firebase 로그인을 지원하지 않습니다."
        };
#endif
    }

    
public void Logout()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _auth.SignOut();
#else
        Debug.LogWarning("WebGL에서는 Firebase 로그아웃을 지원하지 않습니다.");
#endif
    }

    public bool IsEmailAvailable(string email)
    {
        throw new NotImplementedException();
    }
}
#endif