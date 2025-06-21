using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;

public class ChatListManager : MonoBehaviour
{

    public GameObject chatPanel, chatListPanel, bottomBar;
    public ChatManager chatManager;

    public GameObject chatBtn;
    public Transform content;

    private BotLoader botLoader;
    FirebaseAuth auth;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
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

        await botLoader.LoadBotsAndCreateButtons(userId, (botList) =>
        {
            foreach (var bot in botList)
            {
                GameObject btn = Instantiate(chatBtn, content.transform);
                btn.GetComponentInChildren<TMP_Text>().text = bot.Nickname;

                btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {

                    Debug.Log("Clicked bot: " + bot.BotId);
                    bottomBar.SetActive(false);
                    chatManager.botId = bot.BotId;
                    ChatEnter();
                });
            }
        });
    }

    public void ChatEnter()
    {
        chatListPanel.SetActive(false);
        chatPanel.SetActive(true);
    }


}
