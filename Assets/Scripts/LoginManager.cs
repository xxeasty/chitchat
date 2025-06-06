using UnityEngine;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput, passwordInput;
    public TMP_Text outputText;

    private FirebaseAuth auth;

    public static string currentUserUID;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firestore 초기화 완료");
            }
            else
            {
                Debug.LogError($"Firebase 초기화 실패: {status}");
            }
        });

    }

    public void OnLoginClick()
    {
#if UNITY_EDITOR
        // 🔧 에디터 테스트용 가짜 UID 생성
        string fakeEmail = emailInput.text.Trim();

        currentUserUID = "debug_" + fakeEmail.GetHashCode();
        outputText.text = "에디터 로그인 (UID: " + currentUserUID + ")";
        PlayerPrefs.SetString("uid", currentUserUID);
        SceneManager.LoadScene("MainScene");
#else
        

        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            outputText.text = "이메일/비밀번호를 입력하세요.";
            return;
        }   

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Log("로그인 실패, 회원가입 시도");
                auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(registerTask => {
                    if (registerTask.IsCanceled || registerTask.IsFaulted)
                    {
                        outputText.text = "회원가입 실패: " + registerTask.Exception?.Message;
                    }
                    else
                    {
                        FirebaseUser newUser = registerTask.Result.User;
                        outputText.text = "회원가입 완료. UID: " + newUser.UserId;
                        SceneManager.LoadScene("Main");
                    }
                });
            }
            else
            {
                // 로그인 성공
                FirebaseUser user = task.Result.User;
                outputText.text = "로그인 성공. UID: " + user.UserId;
                SceneManager.LoadScene("Main");
            }
        });
                outputText.text = "에디터 전용 테스트입니다.";
#endif
    }
}