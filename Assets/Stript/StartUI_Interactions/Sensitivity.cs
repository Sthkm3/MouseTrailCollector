using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Sensitivity : MonoBehaviour
{
    [Header("UI 參照")]
    [Tooltip("Slider")]
    public Slider targetSlider;
    
    [Tooltip("Text (TMP)")]
    public TextMeshProUGUI valueDisplayText;

    void Start()
    {
        if (targetSlider != null)
        {
            targetSlider.minValue = 0.1f;
            targetSlider.maxValue = 10.0f;
            targetSlider.wholeNumbers = false;

            UpdateDisplayText(targetSlider.value);

            targetSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    void OnSliderValueChanged(float rawValue)
    {
        float steppedValue = Mathf.Round(rawValue * 10f) / 10f;

        UpdateDisplayText(steppedValue);
    }

    void UpdateDisplayText(float val)
    {
        if (valueDisplayText != null)
        {
            valueDisplayText.text = $"靈敏度 :  {val:F1}";
        }
    }
}