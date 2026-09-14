using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

class Program
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_MOUSEMOVE = 0x0200;

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    private static LowLevelMouseProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    private static Stopwatch stopwatch = Stopwatch.StartNew();
    private static long lastTicks = 0;
    private static List<double> intervals = new List<double>();
    private static readonly object lockObj = new object();

    static void Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("純 C# 獨立滑鼠輪詢率 (Polling Rate) 檢測工具");
        Console.WriteLine("請在視窗外「持續滑動滑鼠」...");
        Console.WriteLine("關閉主控台視窗即可結束。");
        Console.WriteLine("========================================");

        // 安裝全域滑鼠 Hook
        _hookID = SetHook(_proc);

        // 啟動背景執行緒，每 0.5 秒計算並印出數據
        Thread statsThread = new Thread(PrintStats);
        statsThread.IsBackground = true;
        statsThread.Start();

        // Windows 低階 Hook 需要訊息迴圈 (Message Loop) 來接收事件
        MSG msg;
        while (GetMessage(out msg, IntPtr.Zero, 0, 0) > 0)
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }

        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelMouseProc proc)
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
                // 計算兩次滑鼠移動事件之間的高精度毫秒差
                double ms = (double)(currentTicks - lastTicks) / Stopwatch.Frequency * 1000.0;
                
                lock (lockObj)
                {
                    if (ms > 0.001 && ms < 100.0) // 過濾異常極端值
                    {
                        intervals.Add(ms);
                    }
                }
            }
            lastTicks = currentTicks;
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private static void PrintStats()
    {
        while (true)
        {
            Thread.Sleep(500);
            List<double> currentBatch;
            
            lock (lockObj)
            {
                currentBatch = new List<double>(intervals);
                intervals.Clear();
            }

            if (currentBatch.Count > 0)
            {
                double sum = 0;
                foreach (var val in currentBatch) sum += val;
                double avgMs = sum / currentBatch.Count;
                int hz = avgMs > 0 ? (int)Math.Round(1000.0 / avgMs) : 0;

                Console.WriteLine($"平均間隔: {avgMs,6:F2} ms  |  實測回報率: {hz,5} Hz (樣本數: {currentBatch.Count})");
            }
            else
            {
                Console.WriteLine("等待滑鼠移動...");
            }
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int x; public int y; }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG { public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam; public uint time; public POINT pt; }
}