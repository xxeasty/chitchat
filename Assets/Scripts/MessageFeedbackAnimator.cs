using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class MessageFeedbackAnimator : MonoBehaviour
{
    public TMP_Text messageText;
    public GameObject flyingTextPrefab;  // 날아오는 텍스트 프리팹
    public string _userId, _botId, _messageId;

    private string originalText;

    private void Start()
    {
    }

    public void Init(string userId, string botId, string messageId)
    {
        _userId = userId;
        _botId = botId;
        _messageId = messageId;

        originalText = messageText.text;
        StartCoroutine(CallFeedbackAPI(originalText));
    }

    IEnumerator CallFeedbackAPI(string userInput)
    {
        string json = JsonConvert.SerializeObject(new FeedbackPayload { userId = _userId, botId = _botId, input = userInput, messageId = _messageId });

        UnityWebRequest req = new UnityWebRequest("https://feedback-onuploaded-oupagmtrea-uc.a.run.app", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string rawJson = req.downloadHandler.text;
            Debug.Log("응답 JSON: " + rawJson);
            try
            {
                JObject jsonObj = JObject.Parse(rawJson);
                string innerJson = jsonObj["result"]?.ToString();

                JObject parsedResult = JObject.Parse(innerJson);

                string suggestion = parsedResult["result"]?.ToString();
                var editsToken = parsedResult["edits"];
                if (editsToken.Type == JTokenType.String && editsToken.ToString() == "empty")
                {
                    Debug.Log("수정사항 없음");
                }
                else if (editsToken.Type == JTokenType.Object)
                {
                    var edits = editsToken.ToObject<Dictionary<string, FeedbackEdit>>();
                    StartCoroutine(AnimateEdits(originalText, suggestion, edits));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JSON 파싱 실패: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("GPT 피드백 요청 실패: " + req.responseCode + " - " + req.error);
        }
    }

    IEnumerator AnimateEdits(string original, string corrected, Dictionary<string, FeedbackEdit> edits)
    {
        string displayText = original;

        foreach (var pair in edits)
        {
            string from = pair.Value.from;
            string to = pair.Value.to;

            int index = displayText.IndexOf(from);
            if (index < 0) continue;

            string spaces = new string(' ', to.Length);
            displayText = displayText.Remove(index, from.Length).Insert(index, spaces);
            messageText.text = displayText;

            GameObject flyingText = Instantiate(flyingTextPrefab, messageText.transform.parent);
            TMP_Text flyingTextComp = flyingText.GetComponent<TMP_Text>();
            flyingTextComp.text = to;

            Vector3 startPos = messageText.transform.position + new Vector3(0, 50, 0);
            flyingText.transform.position = startPos;

            float duration = 0.5f;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;

                // 이 시점에서 매번 위치 재계산
                messageText.ForceMeshUpdate();
                Vector3 dynamicTarget;
                if (index < messageText.textInfo.characterCount)
                {
                    var charInfo = messageText.textInfo.characterInfo[index];
                    dynamicTarget = messageText.transform.TransformPoint((charInfo.bottomLeft + charInfo.topRight) / 2f);
                }
                else
                {
                    dynamicTarget = messageText.transform.position;
                }

                flyingText.transform.position = Vector3.Lerp(startPos, dynamicTarget, t);
                yield return null;
            }

            flyingText.transform.position = messageText.transform.position;
            Destroy(flyingText);

            displayText = displayText.Remove(index, to.Length).Insert(index, to);
            messageText.text = displayText;

            yield return new WaitForSeconds(0.1f);
        }
    }


    class FeedbackPayload
    {
        public string userId;
        public string botId;
        public string input;
        public string messageId;
    }

    class FeedbackEdit
    {
        public string from;
        public string to;
        public string feedback;
    }
}
