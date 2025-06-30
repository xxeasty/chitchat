using TMPro;
using UnityEngine;

public class ChatInputCursor : MonoBehaviour
{
    TMP_InputField inputField;

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    public void ScrollToCursor()
    {
        if (inputField == null) return;

        inputField.ForceLabelUpdate();
    }
}