using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PageManager : MonoBehaviour
{

    public GameObject[] pages;
    public GameObject onBoarding, bottomBar;
    public Text topBar_text;
    public GameObject previousBtn;

    public int page = 0;

    public void ChangeSpriteColor()
    {

        previousBtn.GetComponent<Image>().color = new Color(255 / 255f, 255 / 255f, 255 / 255f);
        previousBtn.transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(183 / 255f, 183 / 255f, 183 / 255f);

        GameObject clicked = EventSystem.current.currentSelectedGameObject;
        previousBtn = clicked;
        clicked.GetComponent<Image>().color = new Color(102 / 255f, 126 / 255f, 234 / 255f);
        clicked.transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(102 / 255f, 126 / 255f, 234 / 255f);

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

    public void showOnBoarding()
    {
        onBoarding.SetActive(true);
        bottomBar.SetActive(false);
    }

    public void onBoardingBack()
    {
        onBoarding.SetActive(false);
        bottomBar.SetActive(true);
    }

}
