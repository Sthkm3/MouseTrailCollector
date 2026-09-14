using UnityEngine;
using TMPro;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class Win32PollingAnalyzer : MonoBehaviour
{
    [Header("UI 參照")]
    [Tooltip("請把顯示輪詢率的 Text (TMP) 拖進這裡")]
    public TextMeshProUGUI pollingDisplay;

    private const int WH_MOUSE_LL = 14;
    private const int WM_MOUSEMOVE = 0x0200;

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    private static LowLevelMouseProc _proc;
    private static IntPtr _hookID = IntPtr.Zero;

    private static Stopwatch stopwatch = new Stopwatch();
    private static long lastTicks = 0;
    private static List<double> intervals = new List<double>();
    private static readonly object lockObj = new object();

    void OnEnable()
    {
        stopwatch.Start();
        _proc = HookCallback;
        // 在 Unity 啟動時安裝 Windows 底層滑鼠勾子
        _hookID = SetHook(_proc);
    }

    void OnDisable()
    {
        // 離開或關閉時務必解除安裝，否則容易造成 Unity 編輯器卡頓
        UnhookWindowsHookEx(_hookID);
    }

    private IntPtr SetHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEMOVE)
        {
            long currentTicks = stopwatch.ElapsedTicks;
            if (lastTicks != 0)
            {
                // 計算兩次底層滑鼠事件之間的高精度毫秒差
                double ms = (double)(currentTicks - lastTicks) / Stopwatch.Frequency * 1000.0;
                
                lock (lockObj)
                {
                    if (ms > 0.001 && ms < 100.0) // 過濾極端異常值
                    {
                        intervals.Add(ms);
                    }
                }
            }
            lastTicks = currentTicks;
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    void Update()
    {
        // 每幀從清單中抓取並清空累積的數據，計算平均值更新到 UI
        List<double> currentBatch;
        lock (lockObj)
        {
            currentBatch = new List<double>(intervals);
            intervals.Clear();
        }

        if (currentBatch.Count > 0 && pollingDisplay != null)
        {
            double sum = 0;
            foreach (var val in currentBatch) sum += val;
            double avgMs = sum / currentBatch.Count;
            int hz = avgMs > 0 ? (int)Math.Round(1000.0 / avgMs) : 0;

            pollingDisplay.text = $"延遲: {avgMs:F2} ms | 實測回報率: {hz} Hz";
        }
    }

    #region Win32 API 匯入
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    #endregion
}