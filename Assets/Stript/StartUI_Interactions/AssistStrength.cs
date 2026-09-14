using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AssistStrength : MonoBehaviour
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
            // 設定拉桿的屬性
            targetSlider.minValue = 1f;
            targetSlider.maxValue = 100f;
            targetSlider.wholeNumbers = true; // 每 1 一個階梯，使用整數

            // 初始化顯示
            UpdateDisplayText(targetSlider.value);

            // 註冊拉桿數值改變事件
            targetSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    void OnSliderValueChanged(float rawValue)
    {
        int steppedValue = Mathf.RoundToInt(rawValue);
        UpdateDisplayText(steppedValue);
    }

    void UpdateDisplayText(float val)
    {
        if (valueDisplayText != null)
        {
            int intVal = Mathf.RoundToInt(val);
            valueDisplayText.text = $"輔助強度 : {intVal}%";
        }
    }
}