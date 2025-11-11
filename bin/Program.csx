using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!!!");
Console.WriteLine(Directory.GetCurrentDirectory());

try
{
    {
        var folderWatcher = new FolderWacher();
        folderWatcher.Start(".");
        while (true)
        {
            if (folderWatcher.IsChanged == true)
            {
                {
                    var handle = Process.GetCurrentProcess().MainWindowHandle;
                    Console.WriteLine(handle);
                    Console.WriteLine(Process.GetCurrentProcess());
                }
                
                
                var  process = new System.Diagnostics.Process();
                var path = folderWatcher.path;
                switch (Path.GetExtension(path))
                {
                    case ".py":
                        process.StartInfo = new("python", $"{path}");
                        break;
                }
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();
            }
            else
            {
                Thread.Sleep(100);
            }
        }
    }
}
catch (Exception e)
{
    Console.Write(e.Message);
}

public class FolderWacher
{
    private System.IO.FileSystemWatcher watcher = null;
    private bool bNeedRestart = false;
    public string path;

    public void Start(string path)
    {
        if (watcher != null) return;

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
        watcher.IncludeSubdirectories = true;

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
        watcher.EnableRaisingEvents = false;
        watcher.Dispose();
        watcher = null;
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
        if (bNeedRestart == true || fileName.StartsWith(".") || fileName.StartsWith("#") || fileName.StartsWith("__") || fileName.EndsWith(".pyc"))
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
