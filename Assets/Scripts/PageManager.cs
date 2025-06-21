using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageManager : MonoBehaviour
{

    public GameObject homePanel, chatPanel, friendPanel, eduPanel;

    private int page = 2;

    public void HomeClicked()
    {
        if (page != 1) {
            if (page == 2)
                chatPanel.SetActive(false);
            if (page == 3)
                friendPanel.SetActive(false);
            if (page == 4)
                eduPanel.SetActive(false);
            homePanel.SetActive(true);
            page = 1;
        }
    }

    public void ChatClicked()
    {
        if (page != 2)
        {
            if (page == 1)
                homePanel.SetActive(false);
            if (page == 3)
                friendPanel.SetActive(false);
            if (page == 4)
                eduPanel.SetActive(false);
            chatPanel.SetActive(true);
            page = 2;
        }
    }

    public void FriendClicked()
    {
        if (page != 3)
        {
            if (page == 1)
                homePanel.SetActive(false);
            if (page == 2)
                chatPanel.SetActive(false);
            if (page == 4)
                eduPanel.SetActive(false);
            friendPanel.SetActive(true);
            page = 3;
        }
    }


    public void EduClicked()
    {
        if (page != 4)
        {
            if (page == 1)
                homePanel.SetActive(false);
            if (page == 2)
                chatPanel.SetActive(false);
            if (page == 3)
                friendPanel.SetActive(false);
            eduPanel.SetActive(true);
            page = 4;
        }
    }

}
