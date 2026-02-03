using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;

public class FirebaseTutorial : MonoBehaviour
{
    private FirebaseApp _app = null;
    private FirebaseAuth _auth = null;
    private FirebaseFirestore _db = null;

    private TextMeshPro _progressText;
    private void Start()
    {
        // 과제.
        // 이 씬이 시작되면 아래 내용을 단계적으로 수행한다.
        // - 각 단계 마다 ProgressText의 내용이 바뀐다. (ex. 로그아웃 완료)
        // - 각 단계 마다 Debug.Log로 완료를 알린다. (ex. 로그아웃 완료)
        // 1. 파이베이스 초기화
        // 2. 로그아웃
        // 3. 재로그인
        // 4. 강아지 추가 (전제조건: 파이어스토어 규칙에 로그인한 사람만 글 쓰기 가능)
        // ㄴ 위 내용이 에러없이 잘 이어지게

        InitFirebase();
        _progressText.text = "파이어베이스 초기화 완료"; 
        Debug.Log("파이어베이스 초기화 완료");

        Logout();
        _progressText.text = "로그아웃 완료";
        Debug.Log("로그아웃 완료");

        Login("hongil@skku.re.kr", "12345678");
        _progressText.text = "로그인 완료";
        Debug.Log("로그인 완료");

        SaveDog();
        _progressText.text = "강아지 추가 완료";
        Debug.Log("강아지 추가 완료");

    }

    private void InitFirebase()
    {
        // 콜백 함수 : 특정 이벤트가 발생하고 나면 자동으로 호출되는 함수
        // 접속에 1MS ~~~ 

        // 유니티는 MonoBehaviour 실행에 있어서 싱글쓰레드
        // Task 타입이란 비동기에 대한 진행사항과 완료되었을 때 결과값을 가지고 있는 객체

        // <정리과제>
        // 1. 파이어베이스 로그인/로그아웃, CRUD를 공부하시고
        // 2. Task, async, await
        //    - 단점이 무엇인지 (메인쓰레드로 돌아오지 않을 확률이 크다.)
        // 3. Unity만의 Task : UniTask
        //    - 장점이 무엇인지

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                // 1. 파이어베이스 연결에 성공했다면..
                _app = FirebaseApp.DefaultInstance;       // 파이어베이스 앱   모듈 가져오기
                _auth = FirebaseAuth.DefaultInstance;      // 파이어베이스 인증 모듈 가져오기 
                _db = FirebaseFirestore.DefaultInstance; // 파이어베이스  DB 모듈 가져오기

                Debug.Log("Firebase 초기화 성공!");
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + task.Result);
            }
        });
    }


    private void Register(string email, string password)
    {
        _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("회원가입이 실패했습니다: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("회원가입에 성공했습니다.: {0} ({1})", result.User.DisplayName, result.User.UserId);
        });
    }

    private void Login(string email, string password)
    {
        _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("로그인 실패: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;


            // 로그인에 성공하면은 반환되는 결과값의 유저와 auth 모듈의 CurrentUser가 둘 다 로그인한 유저로 같다.
            FirebaseUser resultUser = task.Result.User;
            FirebaseUser user = _auth.CurrentUser;

            Debug.LogFormat("로그인 성공!: {0} ({1})", result.User.Email, result.User.UserId);
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

    private void SaveDog()
    {
        Dog dog = new Dog("소똥이", 4);

        _db.Collection("Dogs").AddAsync(dog).ContinueWithOnMainThread(task =>
        //_db.Collection("Dogs").Document("개").SetAsync(dog).ContinueWithOnMainThread(task =>
        {
            // Add vs Set
            // Add: 추가한다.
            // Set: 이미 아이디의 문서가 있다면 수정하고, 없다면 추가한다.

            if (task.IsCompletedSuccessfully)
            {
                string documentId = task.Result.Id;
                Debug.LogError("저장 성공! 문서 ID: " + documentId);
                Debug.LogError("저장 성공!");
            }
            else
            {
                Debug.LogError("저장 실패: " + task.Exception);
            }
        });
    }

    private void LoadMyDog()
    {
        _db.Collection("Dogs").Document("개").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                var snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Dog myDog = snapshot.ConvertTo<Dog>();
                    Debug.Log($"{myDog.Name}({myDog.Age})");
                }
                else
                {
                    Debug.LogError("데이터가 없습니다.");
                }
            }
            else
            {
                Debug.LogError("불러오기 실패: " + task.Exception);
            }
        });

    }

    private void LoadDogs()
    {
        _db.Collection("Dogs").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                var snapshots = task.Result;
                Debug.Log("강아지들-------------------------------------------");
                foreach (DocumentSnapshot snapshot in snapshots.Documents)
                {
                    Dog myDog = snapshot.ConvertTo<Dog>();
                    Debug.Log($"{myDog.Name}({myDog.Age})");
                }

                Debug.LogError("불러오기 성공!");
            }
            else
            {
                Debug.LogError("불러오기 실패: " + task.Exception);
            }
        });
    }

    private void DeleteDogs()
    {
        // 목표: 소똥이들 삭제
        _db.Collection("Dogs").WhereEqualTo("Name", "소똥이").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                var snapshots = task.Result;
                Debug.Log("강아지들-------------------------------------------");
                foreach (DocumentSnapshot snapshot in snapshots.Documents)
                {
                    Dog myDog = snapshot.ConvertTo<Dog>();
                    if (myDog.Name == "소똥이")
                    {
                        _db.Collection("Dogs").Document(myDog.Id).DeleteAsync().ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompletedSuccessfully)
                            {
                                Debug.Log("데이터가 삭제됐습니다.");
                            }
                        });
                    }
                }

                Debug.LogError("불러오기 성공!");
            }
            else
            {
                Debug.LogError("불러오기 실패: " + task.Exception);
            }
        });
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

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SaveDog();
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            LoadMyDog();
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            LoadDogs();
        }
    }

}