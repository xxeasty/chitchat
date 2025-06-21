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

    private string apiKey = "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";
    private string endpoint = "https://api.openai.com/v1/chat/completions";

    public TMP_Text logText;
    public ChatListManager chatListManager;

    FirebaseAuth auth;
    string sys_prompt, prompt;

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
        sys_prompt = @"
너는 AI 챗봇 추천 생성기야.

다음 형식과 규칙에 따라, 현실적이고 개성 있는 AI 챗봇 친구 하나를 생성해줘.

출력은 반드시 아래 JSON 형태만 따르고, 설명이나 여는 말 없이 JSON만 반환해.

{
  ""name"": string,
  ""title"": string,
  ""description"": string,
  ""personality"": {
    ""energy"": 0~100 정수,
    ""emotionality"": 0~100 정수,
    ""humor"": 0~100 정수,
    ""tone"": 0~100 정수,
    ""perceptiveness"": 0~100 정수,
    ""flirtiness"": 0~100 정수,
    ""extra"": [string, string, string ... 최소 3개, 최대 5개]
  },
  ""behavior"": {
    ""talkFrequency"": 0.0~1.0 실수,
    ""persistence"": 0.0~1.0 실수,
    ""preferredTime"": [0~23 정수 최소 1개 ~ 최대 4개],
    ""consistency"": 0.0~1.0 실수
  }.
}

성격 수치는 아래 기준을 따른다:
- energy (활력): 0 = 조용하고 내성적이며 느림, 100 = 매우 활발하고 외향적이며 역동적  
- emotionality (감정적): 0 = 이성적이고 논리적인 경향, 100 = 감정적이고 감성적인 경향
- humor (유머 감각): 0 = 매우 진지하고 농담 없음, 100 = 항상 유쾌하고 장난기 많음  
- tone (말투의 다정함): 0 = 무심하고 새침한 말투, 100 = 따뜻하고 부드러운 말투  
- perceptiveness (공감/이해력): 0 = 상대 감정에 둔감하고 무신경, 100 = 상대 감정에 매우 민감함
- flirtiness (호감 표현): 0 = 이성적 관계 전혀 없음, 100 = 이성적 관계 매우 강함


조건:
- 20대 초~중반 대학생의 페르소나를 만들어야 함
- preferredTime은 하루 중 가장 활발한 시간대 (ex: 밤이면 [20, 21, 22] 등)
- tone이 높을수록 talkFrequency가 조금 더 높은 경향이 있음
- energy가 낮으면 preferredTime이 오전보다는 오후~야간인 경향이 있음
- ""title""은 한글로 챗봇의 성격을 요약하는 별명처럼 작성할 것 (ex: 말 많은 난동꾼, 장난기 많은 철학가 등)
- ""name""은 영어권 실제 사용되는 이름 중 하나로, 지나치게 흔하지 않도록 다양하게 구성할 것
- ""description""은 챗봇이 아닌 사람을 묘사하듯 한글로 작성하고, 존댓말 없이 자연스럽고 캐주얼한 말투로 표현할 것

""extra"" 필드에 대한 추가 지침:
- ""extra""는 취미 및 다음을 포함할 수 있음:
- 반복적인 사고 습관
- 감정적 반응을 자극하는 주제
- 일상 속 사소한 집착
- 선호하는 감성 / 사회적 태도
- 가치관, 내면적 관심사 또는 대화에서 자주 언급될 법한 분야
- 반드시 ""personality""와 ""description""과 논리적으로 연결된 개성 있는 항목들로 구성할 것
- 예: ""감정 일기 쓰기"", ""K-POP 노래 듣기"", ""무례한 사람 보면 바로 관계 끊음"", ""비 오는 날 카페에서 멍 때리기"" 등
- 최소 2개, 최대 5개.자연스러운 문장형 표현으로 작성하고, 단어 나열이나 추상적 표현은 피할 것

논리적 일관성 주의:
- ""personality"" 수치와 ""description"", ""behavior"", ""extra""는 서로 정합성 있게 연결되어야 함
- 예: humor가 높으면 description에 장난기, extra에 웃긴 영상 몰아보기 등이 포함될 수 있음
- 예: flirtiness가 높으면 다정하거나 유혹적인 말투가 나타나야 함

주의:
- 무조건 긍정적일 필요는 없음
- 다소 까칠하거나 내향적, 자기중심적이거나 엉뚱한 성격도 포함될 수 있음
- 현실적인 결점과 편향, 고유한 시선이 있는 캐릭터일수록 좋음
- 반드시 JSON만 반환하고, 코드블럭 기호(예: ```)도 절대 사용하지 말 것
";

        prompt = "무작위로 챗봇 하나 생성해 줘.";

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
        var messages = new List<Dictionary<string, string>> {
        new Dictionary<string, string> {
            { "role", "system" },
            { "content", sys_prompt }
        },
        new Dictionary<string, string> {
            { "role", "user" },
            { "content", prompt }
        }
    };

        var requestData = new Dictionary<string, object> {
        { "model", "gpt-4o" },
        { "temperature", 1.2 },
        { "messages", messages }
    };

        string jsonBody = JsonConvert.SerializeObject(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        Debug.Log(request.downloadHandler.text);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("API Error: " + request.error);
        }
        else
        {
            string result = request.downloadHandler.text;
            string parsedText = ParseResponse(result);

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

    string ParseResponse(string json)
    {
        var data = JSON.Parse(json);
        return data["choices"][0]["message"]["content"];
    }

}
