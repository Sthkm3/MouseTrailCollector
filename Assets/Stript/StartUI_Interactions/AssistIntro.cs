using UnityEngine;
using TMPro;

public class AssistIntro : MonoBehaviour
{
    [Header("UI 參照")]
    [Tooltip("Dropdown")]
    public TMP_Dropdown assistDropdown;

    [Tooltip("Text (TMP)")]
    public TextMeshProUGUI introDisplayText;

    private readonly string[] intros = new string[]
    {
        "       不使用任何瞄準輔助，使用你最純粹的反應力與控制力瞄準。如果可以的話，無輔助與有輔助的測試次數請盡量控制在 1 : 1。",
        "       依照敵方在螢幕上的座標，直接移動準心以實現瞄準。輔助強度為移動量，如設定 100% 會形成暴力鎖。",
        "       依照敵方在螢幕上與你準心的距離，動態更改你的靈敏度(接近敵方時降低靈敏度)以吸附準心。輔助強度為靈敏度降低程度。"
    };

    void Start()
    {
        if (assistDropdown != null)
        {
            UpdateIntro(assistDropdown.value);

            assistDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
    }

    void OnDropdownChanged(int index)
    {
        UpdateIntro(index);
    }

    void UpdateIntro(int index)
    {
        if (introDisplayText != null && index >= 0 && index < intros.Length)
        {
            introDisplayText.text = intros[index];
        }
    }
}