using UnityEngine;
using TMPro;

public class IDManager : MonoBehaviour
{
    [Header("UI 參照")]
    public TMP_InputField inputField;
    public TextMeshProUGUI modeStatusText;
    private const int MaxLength = 12;

    void Start()
    {
        if (inputField != null)
        {
            inputField.characterLimit = MaxLength;
            inputField.onValueChanged.AddListener(OnIDChanged);
            OnIDChanged(inputField.text);
        }
    }

    void OnIDChanged(string currentText)
    {
        if (currentText.Length > MaxLength)
        {
            currentText = currentText.Substring(0, MaxLength);
            inputField.text = currentText;
        }

        if (string.IsNullOrEmpty(currentText.Trim()))
        {
            if (modeStatusText != null)
            {
                modeStatusText.text = "匿名模式";
            }
        }
        else
        {
            if (modeStatusText != null)
            {
                modeStatusText.text = "具名模式 : " + currentText;
            }
        }
    }

    public string GetFinalContributorID()
    {
        string text = inputField.text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return "Anonymous";
        }
        return text;
    }
}