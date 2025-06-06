using Firebase;
using Firebase.Auth;
using UnityEngine;

public class FirebaseInit : MonoBehaviour
{
    public static FirebaseAuth auth;

    void Start()
    {

        System.Environment.SetEnvironmentVariable("FIREBASE_AUTH_EMULATOR_HOST", null);
        System.Environment.SetEnvironmentVariable("USE_AUTH_EMULATOR", null);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase 초기화 완료");
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + status);
            }
        });
    }
}