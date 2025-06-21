using UnityEngine;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEditor;

//[InitializeOnLoad]
//public static class ForceEnvVarBeforeEditorPlay
//{
//    static ForceEnvVarBeforeEditorPlay()
//    {
//        System.Environment.SetEnvironmentVariable("USE_AUTH_EMULATOR", "false");
//        Debug.Log("[EditorInit] 환경변수 강제 설정됨: USE_AUTH_EMULATOR = false");
//    }
//}

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput, passwordInput;
    public TMP_Text outputText;

    FirebaseAuth auth;

    public static string currentUserUID;


    void Start()
    {

        auth = FirebaseAuth.DefaultInstance;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {

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

        if (auth.CurrentUser != null)
            SceneManager.LoadScene("MainScene");

    }

    public void OnLoginClick()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            outputText.text = "이메일/비밀번호를 입력하세요.";
            return;
        }   

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Log("로그인 실패, 회원가입 시도");
                auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(registerTask => {
                    if (registerTask.IsCanceled || registerTask.IsFaulted)
                    {
                        outputText.text = "회원가입 실패: " + registerTask.Exception?.Message;
                    }
                    else
                    {
                        FirebaseUser newUser = registerTask.Result.User;
                        outputText.text = "회원가입 완료. UID: " + newUser.UserId;
                        SceneManager.LoadScene("MainScene");
                    }
                });
            }
            else
            {
                // 로그인 성공
                FirebaseUser user = task.Result.User;
                outputText.text = "로그인 성공. UID: " + user.UserId;
                SceneManager.LoadScene("MainScene");
            }
        });
    }
}