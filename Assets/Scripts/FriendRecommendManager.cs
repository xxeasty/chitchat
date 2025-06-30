using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine.UI;
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
    public int age;
    public string title;
    public string description;
    public Personality personality;
    public Behavior behavior;
}

public class FriendRecommendManager : MonoBehaviour
{

    FirebaseFirestore db;

    public Text botName_text, botAge_text, botTitle_text, botDescription_text;
    public Slider energy_slider, emotionality_slider, humor_slider, tone_slider, perceptiveness_slider, flirtiness_slider;
    public ChatListManager chatListManager;

    FirebaseAuth auth;

    [Header("Meta Info")]
    public string botName;
    public int botAge;
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
            { "age", botAge },
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
                botTitle_text.text = botName + "과 친구 완료!";
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
        botName_text.text = "친구를 찾고 있습니다...";
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
        botAge = profile.age;
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

        botName_text.text = botName;
        botAge_text.text = botAge.ToString() + "세";
        energy_slider.value = energy;
        emotionality_slider.value = emotionality;
        humor_slider.value = humor;
        tone_slider.value = tone;
        perceptiveness_slider.value = perceptiveness;
        flirtiness_slider.value = flirtiness;

        botTitle_text.text = botTitle;
        botDescription_text.text = botDescription;
    }

}
