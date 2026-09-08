using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Collections.Generic;

public class MouseTrailLogger : MonoBehaviour
{
    // 定義單筆數據結構：包含時間戳記與當下的微小位移量
    [System.Serializable]
    public struct MouseSample
    {
        public double timestamp; // 作業系統級別的時間戳記（秒，可換算為 ms）
        public Vector2 delta;    // 該瞬間的滑鼠位移量 (Delta)
    }

    private List<MouseSample> recordedSamples = new List<MouseSample>();
    private bool isRecording = false;

    void OnEnable()
    {
        // 註冊 Input System 的底層事件攔截器
        InputSystem.onEvent += OnDataEvent;
    }

    void OnDisable()
    {
        // 記得取消註冊，避免記憶體洩漏
        InputSystem.onEvent -= OnDataEvent;
    }

    public void StartRecording()
    {
        recordedSamples.Clear();
        isRecording = true;
    }

    public void StopRecording()
    {
        isRecording = false;
        // 這裡可以把 recordedSamples 序列化成 JSON 準備上傳到 GitHub 或後端
    }

    private void OnDataEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!isRecording) return;

        // 檢查這個事件是否由「滑鼠」發出，且是否包含狀態更新
        if (device is Mouse mouse)
        {
            // 判斷事件類型是否為滑鼠動態 (State Event)
            if (eventPtr.IsA<StateEvent>() || eventPtr.IsA<DeltaStateEvent>())
            {
                // 取得該事件發生時作業系統的高精度時間戳記 (Unscaled Time)
                double eventTime = eventPtr.time;

                // 讀取當下的滑鼠位移量
                Vector2 mouseDelta = mouse.delta.ReadValue();

                // 只有當滑鼠真有移動時才記錄，避免無效資料浪費記憶體
                if (mouseDelta != Vector2.zero)
                {
                    recordedSamples.Add(new MouseSample
                    {
                        timestamp = eventTime,
                        delta = mouseDelta
                    });
                }
            }
        }
    }
}