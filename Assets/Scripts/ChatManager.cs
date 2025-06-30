using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using SimpleJSON;
using Newtonsoft.Json;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using Firebase.Auth;

public class ChatManager : MonoBehaviour
{

    FirebaseFirestore db;
    FirebaseAuth auth;

    public GameObject message_ai, message_user;
    public GameObject inChatPanel, inChatWorld, homePanel, bottomBar, backGround;
    public Transform user_pivot, ai_pivot;
    public TMP_InputField textInput;
    public Text nickname_text;
    public Transform bubbleList;

    public string botId, nickname;

    private List<RectTransform> allBubbles = new List<RectTransform>();
    GameObject bubble;

    void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    private void OnEnable()
    {
        DocumentReference botDoc = db.Collection("users")
                                     .Document(auth.CurrentUser.UserId)
                                     .Collection("bots")
                                     .Document(botId);

        Dictionary<string, object> updateData = new Dictionary<string, object>
    {
        { "unreadCount", 0 }
    };

        botDoc.UpdateAsync(updateData);
        nickname_text.text = nickname;
        LoadDB(botId);
    }
    
    public void SendMessage()
    {

        string userText = textInput.text;
        BubbleSet(userText, true);
        SendDB(userText, true, bubble);

        StartCoroutine(GetBotResponse(userText));
        textInput.text = "";

    }

    IEnumerator GetBotResponse(string userMessage)
    {
        string url = "https://chatwithbot-oupagmtrea-uc.a.run.app";

        Dictionary<string, string> postData = new Dictionary<string, string>
    {
        { "botId", botId },
        { "userId", auth.CurrentUser.UserId },
        { "message", userMessage }
    };
        Debug.Log(botId + auth.CurrentUser.UserId + userMessage);
        string jsonBody = JsonConvert.SerializeObject(postData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("chatWithBot API Error: " + request.error);
            yield break;
        }

        string resultJson = request.downloadHandler.text;
        string parsedText = JSON.Parse(resultJson)["result"];
        Debug.Log("GPT ÀÀ´ä: " + parsedText);

        BubbleSet(parsedText, false);
        SendDB(parsedText, false, bubble);
    }

    public void ExitButton()
    {
        inChatPanel.SetActive(false);
        inChatWorld.SetActive(false);
        backGround.SetActive(true);
        homePanel.SetActive(true);
        bottomBar.SetActive(true);
        foreach (Transform child in bubbleList)
            GameObject.Destroy(child.gameObject);
    }

    private void LoadDB(string name)
    {

        db.Collection("users")
        .Document(auth.CurrentUser.UserId)
        .Collection("chats")
        .Document(name)
        .Collection("messages")
        .OrderByDescending("timestamp")
        .Limit(10)
        .GetSnapshotAsync()
        .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    var sorted = new List<DocumentSnapshot>(task.Result.Documents);
                    sorted.Reverse();

                    foreach (var doc in sorted)
                    {
                        var data = doc.ToDictionary();
                        bool isUser = data["sender"].ToString() != "bot";
                        BubbleSet(data["content"].ToString(), isUser);
                    }

                }
            });
    }

    private void BubbleSet(string msgContent, bool isUser) {

        if (isUser)
            bubble = Instantiate(message_user, bubbleList);
        else
            bubble = Instantiate(message_ai, bubbleList);

        var textComponent = bubble.GetComponentInChildren<TMP_Text>();
        textComponent.text = msgContent;

        var rect = bubble.transform.GetChild(0).GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        bubble.transform.position = isUser ? user_pivot.position : ai_pivot.position;

        float height = rect.rect.height * bubble.transform.lossyScale.y;
        float spacing = 0.03f;

        foreach (var existing in allBubbles)
        {
            existing.position += new Vector3(0, height + spacing, 0);
        }

        allBubbles.Add(rect);
    }

    private void SendDB(string chatContent, bool isUser, GameObject bubble)
    {
        DocumentReference messageRef = db.Collection("users")
                                 .Document(auth.CurrentUser.UserId)
                                 .Collection("chats")
                                 .Document(botId)
                                 .Collection("messages")
                                 .Document();

        //if (isUser)
        //    bubble.transform.GetComponent<MessageFeedbackAnimator>().Init(auth.CurrentUser.UserId, botId, messageRef.Id);

        Dictionary<string, object> messageData = null;
        if (isUser)
        {
            messageData = new Dictionary<string, object>
        {
            { "sender", "user" },
            { "content", chatContent },
            { "timestamp", FieldValue.ServerTimestamp }
        };
        } else
        {
            messageData = new Dictionary<string, object>
        {
            { "sender", "bot" },
            { "content", chatContent },
            { "timestamp", FieldValue.ServerTimestamp }
            };
        }

        messageRef.SetAsync(messageData).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("Message uploaded.");
            }
            else
            {
                Debug.LogError("Error sending message: " + task.Exception);
            }
        });

        if (isUser)
        {
            DocumentReference statusRef = db.Collection("users")
                         .Document(auth.CurrentUser.UserId);

            Dictionary<string, object> statusData = new Dictionary<string, object>
        {
            { "lastActive", FieldValue.ServerTimestamp },
        };

            statusRef.SetAsync(statusData).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("lastactive update.");
                }
                else
                {
                    Debug.LogError("Error status update: " + task.Exception);
                }
            });
        }
    }
}

/*
 IEnumerator SendOpenAIRequest(string prompt)
 {

     List<Dictionary<string, string>> messages = new List<Dictionary<string, string>>();
     BuildPrompt(messages, prompt);

     var requestData = new Dictionary<string, object> {
     { "model", "gpt-4o" },
     { "temperature", 0.85 },
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

     if (request.result != UnityWebRequest.Result.Success)
     {
         Debug.Log("API Error: " + request.error);
     }
     else
     {
         string result = request.downloadHandler.text;
         string parsedText = ParseResponse(result);
         GameObject bubble = Instantiate(message_ai, content);
         bubble.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = parsedText;
         SendDB(parsedText, false);
         StartCoroutine(Refresh());
     }
 }

 private void BuildPrompt(List<Dictionary<string, string>> messages, string prompt)
 {
     if (recent_chat_botId != botId)
     {
         messages.Add(new Dictionary<string, string> {
         { "role", "system" },
         { "content", sys_prompt }
     });
     }

     int total = content.childCount;
     int maxCount = Mathf.Min(7, total);

     for (int i = total - maxCount; i < total; i++)
     {
         Transform child = content.GetChild(i);
         string role = "";

         if (child.tag == "user") role = "user";
         else if (child.tag == "bot") role = "assistant";
         else continue;

         TMP_Text text = child.GetComponentInChildren<TMP_Text>();
         if (text == null) continue;

         messages.Add(new Dictionary<string, string> {
             { "role", role },
             { "content", text.text }
         });
     }
     //messages.ForEach(m => Debug.Log(m["content"]));
     recent_chat_botId = botId;
 }
 */