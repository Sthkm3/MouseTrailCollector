using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("UI 參照")]
    [Tooltip("Text (TMP)")]
    public TextMeshProUGUI movementVectorText;
    private Vector2 smoothedDelta = Vector2.zero;
    private const float alpha = 0.1f; // 數值越大 越貼最新數值

    void Update()
    {
        if (Mouse.current != null)
        {
            Vector2 currentDelta = Mouse.current.delta.ReadValue();

            if (smoothedDelta == Vector2.zero)
            {
                smoothedDelta = currentDelta;
            }
            else
            {
                smoothedDelta = (alpha * currentDelta) + ((1.0f - alpha) * smoothedDelta);
            }
        }
        else
        {
            smoothedDelta = Vector2.zero;
        }

        if (movementVectorText != null)
        {
            string xStr = (smoothedDelta.x >= 0 ? "+" : "") + smoothedDelta.x.ToString("F1");
            string yStr = (smoothedDelta.y >= 0 ? "+" : "") + smoothedDelta.y.ToString("F1");
            
            movementVectorText.text = $"移動向量: X: {xStr}, Y: {yStr}";
        }
    }
}