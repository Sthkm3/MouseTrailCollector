using UnityEngine;
using TMPro;

public class ResolutionDisplay : MonoBehaviour
{
    [Header("UI 參照")]
    public TextMeshProUGUI resolutionText;
    private int lastWidth = 0;
    private int lastHeight = 0;

    void Update()
    {
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;

        if (currentWidth != lastWidth || currentHeight != lastHeight)
        {
            lastWidth = currentWidth;
            lastHeight = currentHeight;
            UpdateResolutionText(currentWidth, currentHeight);
        }
    }

    void UpdateResolutionText(int w, int h)
    {
        if (resolutionText == null) return;


        int refreshRate = 60;
        
        if (Screen.currentResolution.refreshRateRatio.value > 0)
        {
            refreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
        }
        resolutionText.text = $"解析度 : {w}*{h}*{refreshRate}hz";
    }
}