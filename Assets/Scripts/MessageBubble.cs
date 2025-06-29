using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageBubble : MonoBehaviour
{

    public float maxWidth = 4.5f;

    public void Init()
    {
        StartCoroutine(AdjustTextWidth());
    }        

    IEnumerator AdjustTextWidth()
    {
        yield return null;

        RectTransform rt = transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();

        float width = rt.rect.width;

        if (width > maxWidth)
        {
            transform.GetComponentInChildren<LayoutElement>().preferredWidth = maxWidth;
            //rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        }
        else
        {
            //rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
    }

    void Update()
    {
        
    }
}
