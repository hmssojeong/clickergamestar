using UnityEngine;
using Firebase.Extensions;
using Firebase;
using Firebase.Auth;

public class FirebaseTutorial : MonoBehaviour
{
    private FirebaseApp _app = null;
    private FirebaseAuth _auth = null;

    private void Start()
    {
        // 콜백 함수 : 특정 이벤트가 발생하고 나면 자동으로 호출되는 함수
        // 접속에 1MS ~~~
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (task.Result == Firebase.DependencyStatus.Available)
            {
                // 1. 파이어베이스 연결에 성공했다면..
                _app = FirebaseApp.DefaultInstance;     // 파이어베이스 앱 모듈 가져오기
                _auth = FirebaseAuth.DefaultInstance;   // 파이어베이스 인증 모듈 가져오기

                Debug.Log("초기화 성공!");
            }
            else
            {
                Debug.Log("초기화 실패"+ task.Result);
            }
        });
    }

    public void Register(string email, string password)
    {
        _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("회원가입이 취소됐습니다.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("회원가입이 실패했습니다: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
        });
    }

    public void Login(string email, string password)
    {
        _auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("로그인 실패" + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;

            // 로그인에 성공하면 반환되는 결과값의 유저와 auth 모듈의 CurrentUser가 둘 다 로그인한 유저로 같다.
            FirebaseUser resultUser = task.Result.User;
            FirebaseUser user = _auth.CurrentUser;

            Debug.LogFormat("로그인 성공!: {0} ({1})", result.User.DisplayName, result.User.UserId);
        });
    }

    private void Logout()
    {
        _auth.SignOut();
        Debug.Log("로그아웃 성공!");
    }

    private void CheckLoginStatus()
    {
        FirebaseUser user = _auth.CurrentUser;
        if (user == null)
        {
            Debug.Log("로그인 안됨");
        }
        else
        {
            Debug.LogFormat("로그인 중: {0} ({1})", user.Email, user.UserId);
        }
    }

    private void Update()
    {
        if (_app == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Register("hongil@skku.re.kr", "12345678");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Login("hongil@skku.re.kr", "12345678");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Logout();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CheckLoginStatus();
        }
    }
}
 