using UnityEngine;
using TMPro;
using Firebase.Auth;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using SimpleJSON;

public class InputPauseListener : MonoBehaviour
{
    TMP_InputField inputField;
    FirebaseAuth auth;

    public GameObject feedback_obj;
    public ChatManager chatManager;
    public Transform chatPanel;
    public float pauseThreshold = 3.0f;

    string userId, botId;
    private float lastInputTime;
    private string lastText;
    private bool hasPaused = false;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        userId = auth.CurrentUser.UserId;
        botId = chatManager.botId;

        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(OnInputChanged);

        lastInputTime = Time.time;
        lastText = inputField.text;
        hasPaused = false;
    }

    void Update()
    {
        if (hasPaused || inputField.text == "") return;

        if (Time.time - lastInputTime >= pauseThreshold)
        {
            hasPaused = true;
            OnInputPause();
        }
    }

    void OnInputChanged(string _)
    {
        if (HasMeaningfulNewContent(lastText, inputField.text))
        {
            lastInputTime = Time.time;
            lastText = inputField.text;
            hasPaused = false;
        }
    }

    bool HasMeaningfulNewContent(string oldText, string newText)
    {
        if (newText.Length <= oldText.Length) return false;

        string added = newText.Substring(oldText.Length);

        foreach (char c in added)
        {
            if (char.IsLetterOrDigit(c))
                return true;
        }

        return false;
    }

    void OnInputPause()
    {
        StartCoroutine(SendPauseFeedbackRequest(userId, botId, inputField.text));
    }

    IEnumerator SendPauseFeedbackRequest(string userId, string botId, string partialInput)
    {
        string url = "https://feedback-onpause-oupagmtrea-uc.a.run.app";

        // JSON payload 구성
        var payload = new
        {
            userId = userId,
            botId = botId,
            partialInput = partialInput
        };

        string json = JsonConvert.SerializeObject(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("GPT 피드백 요청 실패: " + request.error);
        }
        else
        {
            string result = request.downloadHandler.text;
            string parsedText = JSON.Parse(result)["result"];
            GameObject feedback = Instantiate(feedback_obj, chatPanel);
            feedback.transform.GetComponentInChildren<TMP_Text>().text =
                "잘 모르겠어? 이렇게 해보는 건 어때?\n"
                + lastText + $" <color=red>{parsedText}</color>";
        }
    }
}
