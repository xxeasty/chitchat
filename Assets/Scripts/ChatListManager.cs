using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Linq;
using System.Text.RegularExpressions;
using Firebase;
using System;

public class ChatListManager : MonoBehaviour
{

    public GameObject inChatPanel, inChatWorld, backGround, homePanel, bottomBar;
    public ChatManager chatManager;

    public GameObject chatBtn;
    public Transform content;

    private BotLoader botLoader;
    FirebaseAuth auth;
    FirebaseFirestore db;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        botLoader = GetComponent<BotLoader>();
    }

    private void OnEnable()
    {
        LoadBots();
    }

    public async void LoadBots()
    {

        foreach (Transform child in content)
            Destroy(child.gameObject);

        string userId = auth.CurrentUser.UserId;

        await botLoader.LoadBotsAndCreateButtons(userId, async (botList) =>
        {
            foreach (var bot in botList)
            {
                GameObject btn = Instantiate(chatBtn, content.transform);
                btn.transform.GetChild(2).GetComponent<Text>().text = bot.Nickname;

                string message_content = "", timestamp_latest = "";
                QuerySnapshot snapshot = await db.Collection("users")
    .Document(auth.CurrentUser.UserId)
    .Collection("chats")
    .Document(bot.BotId)
    .Collection("messages")
    .OrderByDescending("timestamp")
    .Limit(1)
    .GetSnapshotAsync();

                if (snapshot.Count > 0)
                {
                    var doc = snapshot.Documents.ToList();
                    message_content = doc[0].GetValue<string>("content");
                    Firebase.Firestore.Timestamp ts = doc[0].GetValue<Firebase.Firestore.Timestamp>("timestamp");
                    DateTime dateTime = ts.ToDateTime();
                    string parsedTime = dateTime.ToLocalTime().ToString("tt h:mm", new System.Globalization.CultureInfo("ko-KR"));
                    timestamp_latest = parsedTime;
                }
                else
                    message_content = "대화를 시작해보세요!";

                btn.transform.GetChild(3).GetComponent<Text>().text = message_content;
                btn.transform.GetChild(4).GetComponent<Text>().text = timestamp_latest;

                btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
                {
                    chatManager.botId = bot.BotId;
                    chatManager.nickname = bot.Nickname;
                    ChatEnter();
                });
            }
        });
    }

    public void ChatEnter()
    {
        inChatPanel.SetActive(true);
        inChatWorld.SetActive(true);
        backGround.SetActive(false);
        homePanel.SetActive(false);
        bottomBar.SetActive(false);
    }

}
