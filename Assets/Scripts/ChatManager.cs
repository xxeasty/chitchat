using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using SimpleJSON;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour
{

    FirebaseFirestore db;

    public GameObject message_ai, message_user;
    public GameObject chatPanel, chatListPanel;
    public TMP_InputField textInput;
    public Transform content;
    public ScrollRect scrollRect;
    public TMP_Text inputMessage;

    public string personName;

    private string sys_prompt;

    private string apiKey = "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";
    private string endpoint = "https://api.openai.com/v1/chat/completions";

    void Awake()
    {

        db = FirebaseFirestore.DefaultInstance;

    }

    private void OnEnable()
    {

        sys_prompt = PromptBuilder.BuildSystemPrompt(
            name: personName,
            warmth: 8,
            energy: 5,
            emotionality: 9,
            humor: 6,
            intelligence: 7,
            perceptiveness: 6,
            flirtiness: 2
            );

        Debug.Log(sys_prompt);
        LoadDB(personName);
    }

    IEnumerator SendOpenAIRequest(string prompt)
    {

        string jsonBody = "{\"model\": \"gpt-3.5-turbo\"," +
            " \"messages\": [{\"role\": \"user\", \"content\": \"" + prompt + "\"}]}";
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
            GameObject bubble = Instantiate(message_ai, content);
            bubble.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = parsedText;
            SendDB(parsedText, false);
            StartCoroutine(Refresh());
        }
    }

    IEnumerator Refresh()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
    
    string ParseResponse(string json)
    {
        var data = JSON.Parse(json);
        return data["choices"][0]["message"]["content"];
    }

    public void SendMessage()
    {
        GameObject bubble = Instantiate(message_user, content);
        bubble.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = textInput.text;
        StartCoroutine(Refresh());
        StartCoroutine(SendOpenAIRequest(sys_prompt + " 이건 사용자의 말이니까 대답해: " + textInput.text));
        SendDB(textInput.text, true);
        inputMessage.text = "";
    }

    public void ExitButton()
    {
        chatPanel.SetActive(false);
        chatListPanel.SetActive(true);
        foreach (Transform child in content)
            GameObject.Destroy(child.gameObject);
    }

    private void LoadDB(string name)
    {
        db.Collection("users")
        .Document(PlayerPrefs.GetString("uid", ""))
        .Collection("chats")
        .Document(name)
        .Collection("messages")
        .OrderBy("timestamp")
        .Limit(10)
        .GetSnapshotAsync()
        .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    foreach (var doc in task.Result.Documents)
                    {
                        var data = doc.ToDictionary();
                        GameObject bubble;
                        if (data["userId"].ToString() == name)
                            bubble = Instantiate(message_ai, content);
                        else
                            bubble = Instantiate(message_user, content);
                        bubble.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = data["content"].ToString();
                    }
                }
            });
    }

    private void SendDB(string chatContent, bool isUser)
    {
        DocumentReference messageRef = db.Collection("users")
                                 .Document(PlayerPrefs.GetString("uid", ""))
                                 .Collection("chats")
                                 .Document(personName)
                                 .Collection("messages")
                                 .Document();

        Dictionary<string, object> messageData = null;
        if (isUser) {
            messageData = new Dictionary<string, object>
        {
            { "userId", PlayerPrefs.GetString("uid", "") },
            { "content", chatContent },
            { "timestamp", FieldValue.ServerTimestamp }
        };
        } else
        {
            messageData = new Dictionary<string, object>
        {
            { "userId", personName },
            { "content", chatContent },
            { "timestamp", FieldValue.ServerTimestamp }
            };
        }

        messageRef.SetAsync(messageData).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("Message sent.");
            }
            else
            {
                Debug.LogError("Error sending message: " + task.Exception);
            }
        });
    }


}