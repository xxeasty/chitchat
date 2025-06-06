using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatListManager : MonoBehaviour
{

    public GameObject chatPanel, chatListPanel;
    public ChatManager chatManager;

    public void ChatEnter()
    {
        chatManager.personName = gameObject.transform.GetChild(0).GetComponent<TMP_Text>().text;
        chatListPanel.SetActive(false);
        chatPanel.SetActive(true);
    }


}
