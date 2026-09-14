using UnityEngine;
using TMPro;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class Polling : MonoBehaviour
{
    [Header("UI 參照")]
    [Tooltip("Text (TMP) ")]
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

    // EMA
    private double smoothedMs = 0;
    private const double alpha = 0.25; 

    // 無動作計時
    private List<int> recentRates = new List<int>();
    private float timeSinceLastEvent = 0f;
    private const float idleTimeout = 2f; // 超過 2 秒沒動就歸零

    void OnEnable()
    {
        stopwatch.Start();
        _proc = HookCallback;
        _hookID = SetHook(_proc);
    }

    void OnDisable()
    {
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
                double ms = (double)(currentTicks - lastTicks) / Stopwatch.Frequency * 1000.0;
                
                lock (lockObj)
                {
                    if (ms > 0.001 && ms < 100.0)
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
        List<double> currentBatch;
        lock (lockObj)
        {
            currentBatch = new List<double>(intervals);
            intervals.Clear();
        }

        if (currentBatch.Count > 0)
        {
            timeSinceLastEvent = 0f;

            double sum = 0;
            foreach (var val in currentBatch) sum += val;
            double batchAvgMs = sum / currentBatch.Count;

            if (smoothedMs == 0)
            {
                smoothedMs = batchAvgMs;
            }
            else
            {
                smoothedMs = (alpha * batchAvgMs) + ((1.0 - alpha) * smoothedMs);
            }

            if (smoothedMs > 0)
            {
                double rawHz = 1000.0 / smoothedMs;
                int standardHz = SnapToStandardPollingRate(rawHz);
                recentRates.Add(standardHz);
            }
        }
        else
        {
            timeSinceLastEvent += Time.unscaledDeltaTime;
        }

        if (timeSinceLastEvent >= idleTimeout)
        {
            recentRates.Clear();
            smoothedMs = 0;
        }

        // 更新 UI
        if (pollingDisplay != null)
        {
            if (recentRates.Count > 0)
            {
                int maxHz = recentRates[0];
                foreach (int rate in recentRates)
                {
                    if (rate >= maxHz) maxHz = rate;
                }

                pollingDisplay.text = $"滑鼠回報率: {maxHz} Hz";
                
                if (recentRates.Count > 60)
                {
                    recentRates.RemoveAt(0);
                }
            }
            else
            {
                pollingDisplay.text = "滑鼠回報率: 0 Hz";
            }
        }
    }

    private int SnapToStandardPollingRate(double rawHz)
    {
        int[] standardRates = {63, 125, 250, 500, 1000, 2000, 4000, 8000, 16000};
        int closestRate = standardRates[0];
        double minDifference = double.MaxValue;

        foreach (int rate in standardRates)
        {
            double diff = Math.Abs(rawHz - rate);
            if (diff < minDifference)
            {
                minDifference = diff;
                closestRate = rate;
            }
        }

        return closestRate;
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