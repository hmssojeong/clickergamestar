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
                ErrorMessage = e.Message
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
                ErrorMessage = e.Message
            };
        }
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