using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;


class Program
{

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, uint uFlags);

    const uint SWP_NOMOVE = 0x0002;  // 現在位置を保持します
    const uint SWP_NOSIZE = 0x0001;  // 現在のサイズを保持します
    const uint SWP_NOZORDER = 0x0010;  // 現在の Z オーダーを保持します
    const uint SWP_SHOWWINDOW = 0x0040; // ウィンドウが表示されます。
    const int HWND_TOPMOST = -1;  // 最上位以外のすべてのウィンドウの上にウィンドウをPlacesします。
    const int HWND_NOTOPMOST = -2; // 最上位以外のすべてのウィンドウ (つまり、すべての最上位ウィンドウの背後) の上にウィンドウをPlacesします。
    const uint GW_OWNER = 4;


    static void Main(string[] args)
    {
        Console.WriteLine($"arg0a{args[0]}");
        Foreground();
        var folderWatcher = new FolderWatcher();
        folderWatcher.Start(".");
        Process? process = null;
        while (true)
        {
            if (folderWatcher.IsChanged == true)
            {
                var path = folderWatcher.path;
                Console.WriteLine($"Change {path}");
                Console.WriteLine("---------");
                Foreground();
                if (process != null)
                {
                    process.Kill(true);
                    process = null;
                }
                process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                var hit = Directory.GetFiles(args[0]).Any(x => x.EndsWith("run.bat"));
                var ok = true;
                if (hit)
                {
                    process.StartInfo = new("cmd", $"/k run.bat");
                    ok = true;
                }
                else
                {
                    ok = true;
                    switch (Path.GetExtension(path))
                    {
                        case ".py":
                            process.StartInfo = new("python", $"{path}");
                            break;
                        case ".bat":
                            process.StartInfo = new("cmd", $"/k {path}");
                            break;
                        case ".csx":
                            process.StartInfo = new("dotnet", $"script {path}");
                            break;
                        case ".ps1":
                            process.StartInfo = new("PowerShell", $"-ExecutionPolicy RemoteSigned {path}");
                            break;
                        default:
                            ok = false;
                            break;
                    }
                }
                if (ok)
                {
                    process.Start();
                    Thread.Sleep(1000);
                }
                else
                {
                    process = null;
                    Console.WriteLine("No associated runner.");
                }
                // process.WaitForExit();
                // Console.WriteLine("End");
            }
            else
            {
                Thread.Sleep(100);
            }
        }
    }

    #pragma warning disable CA1416
    static void Foreground()
    {
        {
            var title = Console.Title;
            Console.Title = "Hello, World!";
            Thread.Sleep(40);
            foreach (Process pList in Process.GetProcesses())
            {
                if (pList.MainWindowTitle == "Hello, World!")
                {
                    SetWindowPos(pList.MainWindowHandle, HWND_TOPMOST, 4512, 400, 0, 0, SWP_SHOWWINDOW | SWP_NOSIZE | SWP_NOMOVE);
                    SetWindowPos(pList.MainWindowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                }
            }
            Console.Title = title;
        }
    }
}

public class FolderWatcher
{
    private System.IO.FileSystemWatcher? watcher;
    private bool bNeedRestart = false;
    public string? path;

    public void Start(string path)
    {
        watcher = new System.IO.FileSystemWatcher();
        //監視するディレクトリを指定
        watcher.Path = path;
        //最終アクセス日時、最終更新日時、ファイル、フォルダ名の変更を監視する
        watcher.NotifyFilter =
            (System.IO.NotifyFilters.LastAccess
            | System.IO.NotifyFilters.LastWrite
            | System.IO.NotifyFilters.FileName
            | System.IO.NotifyFilters.DirectoryName);
        //すべてのファイルを監視
        watcher.Filter = "";
        //UIのスレッドにマーシャリングする
        //コンソールアプリケーションでの使用では必要ない
        watcher.SynchronizingObject = null;
        watcher.IncludeSubdirectories = false;

        //イベントハンドラの追加
        watcher.Changed += new System.IO.FileSystemEventHandler(watcher_Changed);
        watcher.Created += new System.IO.FileSystemEventHandler(watcher_Changed);
        watcher.Deleted += new System.IO.FileSystemEventHandler(watcher_Changed);
        watcher.Renamed += new System.IO.RenamedEventHandler(watcher_Renamed);

        //監視を開始する
        watcher.EnableRaisingEvents = true;
        Console.WriteLine("Watcher Start: path = " + path);
    }

    public void Stop()
    {
        //監視を終了
        bNeedRestart = false;
        if (watcher != null)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;
        }
    }

    public bool IsChanged
    {
        get
        {
            if (bNeedRestart)
            {
                bNeedRestart = false;
                return true;
            }
            return false;
        }
    }

    private void watcher_Changed(System.Object source,
        System.IO.FileSystemEventArgs e)
    {
        string fileName = Path.GetFileName(e.FullPath);
        // Console.WriteLine(fileName);
        if (fileName.StartsWith(".") || fileName.StartsWith("#") || fileName.StartsWith("__") || fileName.EndsWith(".pyc"))
        {
            return;
        }

        switch (e.ChangeType)
        {
            case System.IO.WatcherChangeTypes.Changed:
                break;
            case System.IO.WatcherChangeTypes.Created:
                break;
            case System.IO.WatcherChangeTypes.Deleted:
                break;
        }
        path = e.FullPath;
        bNeedRestart = true;
    }

    private void watcher_Renamed(System.Object source,
        System.IO.RenamedEventArgs e)
    {
        string t = Path.GetFileName(e.FullPath).Substring(0, 1);
        if (bNeedRestart == true || t == "." || t == "#" || t.EndsWith(".pyc"))
        {
            return;
        }
        Console.WriteLine(
            "ファイル 「" + e.FullPath + "」の名前が変更されました。");
        bNeedRestart = true;
    }
}
