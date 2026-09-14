import time
from pynput import mouse

last_time = time.perf_counter()
event_intervals = []

def on_move(x, y):
    global last_time
    current_time = time.perf_counter()
    delta = current_time - last_time
    last_time = current_time
    
    if 0.0001 < delta < 0.1:
        event_intervals.append(delta)

def main():
    print("=" * 40)
    print("滑鼠輪詢率 (Polling Rate) 獨立檢測工具")
    print("請在視窗外「持續滑動滑鼠」以獲取數據...")
    print("按 Ctrl + C 可以結束程式並查看統計")
    print("=" * 40)

    listener = mouse.Listener(on_move=on_move)
    listener.start()

    try:
        while True:
            time.sleep(0.5)
            if event_intervals:
                current_batch = event_intervals.copy()
                event_intervals.clear()
                
                avg_interval = sum(current_batch) / len(current_batch)
                hz = 1.0 / avg_interval if avg_interval > 0 else 0
                
                print(f"平均間隔: {avg_interval * 1000:.2f} ms  |  實測回報率: {round(hz):4d} Hz")
                
    except KeyboardInterrupt:
        print("\n正在停止監聽...")
        listener.stop()
        print("已結束。")

if __name__ == "__main__":
    main()