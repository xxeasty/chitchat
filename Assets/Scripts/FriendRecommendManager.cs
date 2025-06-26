using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Text;
using SimpleJSON;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;

[System.Serializable]
public class Personality
{
    public int affinity;
    public int energy;
    public int emotionality;
    public int humor;
    public int tone;
    public int perceptiveness;
    public int flirtiness;
    public List<string> extra;
}

[System.Serializable]
public class Behavior
{
    public float talkFrequency;
    public float persistence;
    public List<int> preferredTime;
    public float consistency;
}

[System.Serializable]
public class ChatbotProfile
{
    public string name;
    public string title;
    public string description;
    public Personality personality;
    public Behavior behavior;
}

public class FriendRecommendManager : MonoBehaviour
{

    FirebaseFirestore db;

    public TMP_Text logText;
    public ChatListManager chatListManager;

    FirebaseAuth auth;

    [Header("Meta Info")]
    public string botName;
    public string botTitle;
    public string botDescription;
    public string botId;

    [Header("Personality")]
    public int affinity;
    public int energy;
    public int emotionality;
    public int humor;
    public int tone;
    public int perceptiveness;
    public int flirtiness;
    public List<string> extra;

    [Header("Behavior")]
    public float talkFrequency;
    public float persistence;
    public List<int> preferredTime;
    public float consistency;

    private void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        StartCoroutine(SendOpenAIRequest());
    }

    public void AddFriend()
    {
        DocumentReference profileRef = db.Collection("users")
                         .Document(auth.CurrentUser.UserId)
                         .Collection("bots")
                         .Document(botId);

        Dictionary<string, object> behaviorData = new Dictionary<string, object>
        {
            { "talkFrequency", talkFrequency },
            { "persistence", persistence },
            { "preferredTime", preferredTime },
            { "consistency", consistency },
        };

        Dictionary<string, object> personalityData = new Dictionary<string, object>
        {
            { "affinity", affinity },
            { "energy", energy },
            { "emotionality", emotionality },
            { "humor", humor },
            { "tone", tone },
            { "perceptiveness", perceptiveness },
            { "flirtiness", flirtiness },
            { "extra", extra }
        };

        Dictionary<string, object> profileData = null;
        profileData = new Dictionary<string, object>
        {
            { "nickname", botName },
            { "title", botTitle },
            { "description", botDescription },
            { "addedAt", FieldValue.ServerTimestamp },
            { "moodScore", 0 },
            { "engagementScore", 0 },
            { "behavior", behaviorData },
            { "personality", personalityData },
        };

        profileRef.SetAsync(profileData).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                logText.text = botName + "와 친구가 되었습니다!\n채팅 목록으로 이동하여 이야기를 나눠보세요.\n새로운 친구를 찾으려면 새로고침해주세요.";
                chatListManager.LoadBots();
                Debug.Log("Profile sent.");
            }
            else
            {
                Debug.LogError("Error sending profile: " + task.Exception);
            }
        });
    }

    IEnumerator SendOpenAIRequest()
    {
        logText.text = "친구를 찾고 있습니다..";

        string url = "https://recommendbot-oupagmtrea-uc.a.run.app";

        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("API Error: " + request.error);
            yield break;
        }

        string resultJson = request.downloadHandler.text;
        string parsedText = JSON.Parse(resultJson)["result"];
        Debug.Log("GPT 응답: " + parsedText);

        ChatbotProfile profile = JsonConvert.DeserializeObject<ChatbotProfile>(parsedText);

        botName = profile.name;
        botTitle = profile.title;
        botDescription = profile.description;

        affinity = 0;
        energy = profile.personality.energy;
        emotionality = profile.personality.emotionality;
        humor = profile.personality.humor;
        tone = profile.personality.tone;
        perceptiveness = profile.personality.perceptiveness;
        flirtiness = profile.personality.flirtiness;
        extra = profile.personality.extra;

        talkFrequency = profile.behavior.talkFrequency;
        persistence = profile.behavior.persistence;
        preferredTime = profile.behavior.preferredTime;
        consistency = profile.behavior.consistency;

        botId = BotIdGenerator.GenerateBotId(botName);

        logText.text =
            $"{botName} - {botTitle}\n" +
            $"특징: {botDescription}\n\n" +

            $"-- 성격 --\n" +
            $"관심사 및 기타 정보: {string.Join(", ", extra)}\n" +
            $"친밀도: {affinity}\n외향성: {energy}\n감정적: {emotionality}\n" +
            $"유머: {humor}\n다정함: {tone}\n세심함: {perceptiveness}\n연애 상대♥: {flirtiness}\n\n" +

            $"-- 행동 특성 (오프라인일 때) --\n" +
            $"말하는 빈도: {talkFrequency}\n" +
            $"무답장이어도 말 거는 빈도: {persistence}\n" +
            $"주 활동 시간대: {string.Join(", ", preferredTime)}\n" +
            $"시간대 선호 일관성: {consistency}";
    }

}
