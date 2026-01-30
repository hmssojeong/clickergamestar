using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScene : MonoBehaviour
{
    // 로그인씬 (로그인/회원가입) -> 게임씬

    private enum SceneMode
    {
        Login,
        Register
    }

    private SceneMode _mode = SceneMode.Login;

    // 비밀번호 확인 오브젝트
    [SerializeField] private GameObject _passwordCofirmObject;
    [SerializeField] private Button _gotoRegisterButton;
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _gotoLoginButton;
    [SerializeField] private Button _registerButton;

    [SerializeField] private TextMeshProUGUI _messageTextUI;

    [SerializeField] private TMP_InputField _idInputField;
    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private TMP_InputField _passwordConfirmInputField;

    // 정규표현식 패턴
    private const string EMAIL_PATTERN = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    // 비밀번호: 영어/숫자/특수문자만, 7-20자, 특수문자 1개 이상, 대소문자 각 1개 이상
    private const string PASSWORD_PATTERN = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?])(?=.*\d)?[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]{7,20}$";

    private const string LAST_LOGIN_ID_KEY = "LastLoginID";

    private void Start()
    {
        AddButtonEvents();
        LoadLastLoginID();
        Refresh();
        Debug.Log("fdsf");
    }

    private void LoadLastLoginID()
    {
        if (PlayerPrefs.HasKey(LAST_LOGIN_ID_KEY))
        {
            string lastLoginID = PlayerPrefs.GetString(LAST_LOGIN_ID_KEY);
            _idInputField.text = lastLoginID;
        }
    }


    private void SaveLastLoginID(string id)
    {
        PlayerPrefs.SetString(LAST_LOGIN_ID_KEY, id);
        PlayerPrefs.Save(); // 즉시 저장
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            // 1. 비밀번호를 바이트 배열로 변환
            byte[] bytes = Encoding.UTF8.GetBytes(password);

            // 2. SHA256 해시 계산 (256비트 = 32바이트)
            byte[] hashBytes = sha256.ComputeHash(bytes);

            // 3. 바이트 배열을 16진수 문자열로 변환 (64자리)
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }

    private void AddButtonEvents()
    {
        _gotoRegisterButton.onClick.AddListener(GotoRegister);
        _loginButton.onClick.AddListener(Login);
        _gotoLoginButton.onClick.AddListener(GotoLogin);
        _registerButton.onClick.AddListener(Register);
    }

    private void Refresh()
    {
        // 2차 비밀번호 오브젝트는 회원가입 모드일때만 노출
        _passwordCofirmObject.SetActive(_mode == SceneMode.Register);
        _gotoRegisterButton.gameObject.SetActive(_mode == SceneMode.Login);
        _loginButton.gameObject.SetActive(_mode == SceneMode.Login);
        _gotoLoginButton.gameObject.SetActive(_mode == SceneMode.Register);
        _registerButton.gameObject.SetActive(_mode == SceneMode.Register);
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        { 
            return false;
        }

        return Regex.IsMatch(email, EMAIL_PATTERN);
    }

    private bool IsValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        // 길이 체크
        if (password.Length < 7 || password.Length > 20)
            return false;

        // 정규표현식으로 패턴 검사
        return Regex.IsMatch(password, PASSWORD_PATTERN);
    }

    private string GetPasswordValidationMessage(string password)
    {
        if (string.IsNullOrEmpty(password))
            return "비밀번호를 입력해주세요.";

        if (password.Length < 7)
            return "비밀번호는 최소 7자 이상이어야 합니다.";

        if (password.Length > 20)
            return "비밀번호는 최대 20자 이하여야 합니다.";

        if (!Regex.IsMatch(password, @"[a-z]"))
            return "비밀번호에 영어 소문자가 최소 1개 포함되어야 합니다.";

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return "비밀번호에 영어 대문자가 최소 1개 포함되어야 합니다.";

        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
            return "비밀번호에 특수문자가 최소 1개 포함되어야 합니다.";

        if (!Regex.IsMatch(password, @"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+$"))
            return "비밀번호는 영어, 숫자, 특수문자만 사용 가능합니다.";

        return string.Empty;
    }
    private void Login()
    {
        // 로그인
        // 1. 아이디 입력을 확인한다.
        string id = _idInputField.text;
        if (string.IsNullOrEmpty(id))
        {
            _messageTextUI.text = "아이디를 입력해주세요.";
            return;
        }

        // 2. 비밀번호 입력을 확인한다.
        string password = _passwordInputField.text;
        if (string.IsNullOrEmpty(password))
        {
            _messageTextUI.text = "패스워드를 입력해주세요.";
            return;
        }

        // 3. 실제 저장된 아이디-비밀번호 계정이 있는지 확인한다.
        // 3-1. 아이디가 있는지 확인한다.
        if (!PlayerPrefs.HasKey(id))
        {
            _messageTextUI.text = "아이디를 확인해주세요.";
            return;
        }

        // 3-2. 저장된 해시된 비밀번호를 가져온다
        string savedHashedPassword = PlayerPrefs.GetString(id);

        // 3-3. 입력된 비밀번호를 해싱한다
        string inputHashedPassword = HashPassword(password);

        // 3-4. 해시값을 비교한다 (원본 비밀번호는 알 수 없음)
        if (savedHashedPassword != inputHashedPassword)
        {
            _messageTextUI.text = "비밀번호를 확인해주세요.";
            return;
        }
        SaveLastLoginID(id);

        // 4. 있다면 씬 이동
        // 동기() -> 유저가 대기하게 한다.
        SceneManager.LoadScene("LoadingScene");

    }

    private void Register()
    {
        // 로그인
        // 1. 아이디 입력을 확인한다.
        string id = _idInputField.text;
        if (string.IsNullOrEmpty(id))
        {
            _messageTextUI.text = "아이디를 입력해주세요.";
            return;
        }

        if(!IsValidEmail(id))
        {
            _messageTextUI.text = "올바른 이메일 형식으로 입력해주세요. (예: example@email.com)";
            return;
        }

        // 2. 비밀번호 입력을 확인한다.
        string password = _passwordInputField.text;
        if (string.IsNullOrEmpty(password))
        {
            _messageTextUI.text = "패스워드를 입력해주세요.";
            return;
        }
        
        if(!IsValidPassword(password))
        {
            string validationMessage = GetPasswordValidationMessage(password);
            _messageTextUI.text = validationMessage;
            return;
        }

        // 2. 2ck 비밀번호 입력을 확인한다.
        string password2 = _passwordConfirmInputField.text;
        if (string.IsNullOrEmpty(password2) || password != password2)
        {
            _messageTextUI.text = "비밀번호 확인이 일치하지 않습니다.";
            return;
        }

        // 4. 실제 저장된 아이디-비밀번호 계정이 있는지 확인한다.
        // 4-1. 아이디가 있는지 확인한다.
        if (PlayerPrefs.HasKey(id))
        {
            _messageTextUI.text = "중복된 아이디입니다.";
            return;
        }

        // 5. 비밀번호를 해싱하여 저장 (원본 비밀번호는 저장하지 않음)
        string hashedPassword = HashPassword(password);
        PlayerPrefs.SetString(id, hashedPassword);
        PlayerPrefs.Save();


        PlayerPrefs.SetString(id, password);
        _messageTextUI.text = "회원가입이 완료되었습니다!";

        GotoLogin();
    }

    private void GotoLogin()
    {
        _mode = SceneMode.Login;
        _messageTextUI.text = "";
        LoadLastLoginID();
        Refresh();
    }

    private void GotoRegister()
    {
        _mode = SceneMode.Register;
        _messageTextUI.text = "";
        _idInputField.text = "";
        _passwordInputField.text = "";
        _passwordConfirmInputField.text = "";
        Refresh();
    }
}