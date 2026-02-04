using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IAccountRepository
{
    bool IsEmailAvailable(string email);
    UniTask<AccountResult> Register(string email, string password);
    UniTask<AccountResult> Login(string email, string password);
    void Logout();
}