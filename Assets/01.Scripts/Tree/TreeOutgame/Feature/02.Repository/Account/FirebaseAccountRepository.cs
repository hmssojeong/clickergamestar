using System;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public class FirebaseAccountRepository : IAccountRepository
{
    private FirebaseAuth _auth;

    public FirebaseAccountRepository()
    {
        _auth = FirebaseAuth.DefaultInstance;
    }

public async UniTask<AccountResult> Register(string email, string password)
    {
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
                ErrorMessage = GetKoreanErrorMessage(e.Message)
            };
        }
    }

public async UniTask<AccountResult> Login(string email, string password)
    {
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
                ErrorMessage = GetKoreanErrorMessage(e.Message)
            };
        }
    }

    
    private string GetKoreanErrorMessage(string firebaseError)
    {
        // Firebase 에러 메시지를 한글로 변환
        if (firebaseError.Contains("email") && firebaseError.Contains("not found"))
            return "아이디를 확인해주세요.";
        if (firebaseError.Contains("password") && firebaseError.Contains("wrong"))
            return "아이디를 확인해주세요.";
        if (firebaseError.Contains("user-not-found"))
            return "아이디를 확인해주세요.";
        if (firebaseError.Contains("wrong-password"))
            return "아이디를 확인해주세요.";
        if (firebaseError.Contains("invalid-credential"))
            return "아이디를 확인해주세요.";
        if (firebaseError.Contains("email-already-in-use"))
            return "이미 사용중인 이메일입니다.";
        if (firebaseError.Contains("weak-password"))
            return "비밀번호가 너무 약합니다.";
        if (firebaseError.Contains("invalid-email"))
            return "잘못된 이메일 형식입니다.";
        if (firebaseError.Contains("network"))
            return "네트워크 연결을 확인해주세요.";
        
        return "로그인에 실패했습니다.";
    }

    
public void Logout()
    {
        _auth.SignOut();
    }

    public bool IsEmailAvailable(string email)
    {
        throw new NotImplementedException();
    }
}