using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PageManager : MonoBehaviour
{

    public GameObject[] pages;
    public TMP_Text topBar_text;
    GameObject previousBtn = null;

    public int page = 0;

    public void ChangeSpriteColor()
    {

        if (previousBtn != null)
        {
            previousBtn.GetComponent<Image>().color = new Color(255f, 255f, 255f);
            previousBtn.transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(183f, 183f, 183f);
        }

        GameObject clicked = EventSystem.current.currentSelectedGameObject;
        previousBtn = clicked;
        clicked.GetComponent<Image>().color = new Color(102f, 126f, 234f);
        clicked.transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(102f, 126f, 234f);

    }

    public void ShowPage(int pageIndex)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == pageIndex);
        }
        if (pageIndex == 0)
            topBar_text.text = "ChitChat";
        else if (pageIndex == 1)
            topBar_text.text = "³» Æê";
        else if (pageIndex == 2)
            topBar_text.text = "Äù½ºÆ®";

        page = pageIndex;
    }

}
