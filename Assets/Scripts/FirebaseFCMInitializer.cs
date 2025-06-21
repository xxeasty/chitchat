using UnityEngine;
using Firebase;
using Firebase.Messaging;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class FirebaseFCMInitializer : MonoBehaviour
{
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                Debug.Log("Firebase 초기화 완료");
                InitializeFCM();
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + status);
            }
        });
    }

    private void InitializeFCM()
    {
        // 메시징 콜백 연결
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;

        // 토큰 직접 요청
        FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(tokenTask =>
        {
            if (!tokenTask.IsCompletedSuccessfully)
            {
                Debug.LogError("FCM 토큰 요청 실패: " + tokenTask.Exception);
                return;
            }

            string token = tokenTask.Result;
            Debug.Log("FCM 토큰: " + token);

            TryUploadTokenToFirestore(token);
        });
    }

    private void TryUploadTokenToFirestore(string token)
    {
        var auth = FirebaseAuth.DefaultInstance;
        var firestore = FirebaseFirestore.DefaultInstance;

        var user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("로그인된 유저 없음. FCM 토큰 저장 생략");
            return;
        }

        DocumentReference userDoc = firestore.Collection("users").Document(auth.CurrentUser.UserId);
        userDoc.SetAsync(new { fcmToken = token }, SetOptions.MergeAll).ContinueWithOnMainThread(uploadTask =>
        {
            if (uploadTask.IsCompletedSuccessfully)
            {
                Debug.Log("FCM 토큰 Firestore 저장 완료");
            }
            else
            {
                Debug.LogError("Firestore 저장 실패: " + uploadTask.Exception);
            }
        });
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("FCM 토큰 수신 (이벤트): " + token.Token);
        // 이벤트에서도 Firestore 저장하려면 아래 주석 해제
        // TryUploadTokenToFirestore(token.Token);
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("FCM 메시지 수신됨");
        if (e.Message.Notification != null)
        {
            Debug.Log($"제목: {e.Message.Notification.Title}");
            Debug.Log($"본문: {e.Message.Notification.Body}");
        }

        if (e.Message.Data != null)
        {
            foreach (var kvp in e.Message.Data)
            {
                Debug.Log($"데이터: {kvp.Key} = {kvp.Value}");
            }
        }
    }
}