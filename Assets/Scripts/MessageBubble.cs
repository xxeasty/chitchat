using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBubble : MonoBehaviour
{

    public float maxWidth = 1300f;

    // Start is called before the first frame update
    void Start()
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
            transform.GetChild(0).GetChild(0).GetComponent<LayoutElement>().preferredWidth = maxWidth;
            transform.GetChild(0).GetComponent<LayoutElement>().preferredWidth = maxWidth;
            //rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        }
        else
        {
            //rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
