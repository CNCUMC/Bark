using System;
using System.IO;
using Bark.Tool;

namespace Bark.ScriptApi;

public class LogApi
{
    private readonly string _modName;
    private readonly StreamWriter? _archiveWriter;
    private static StreamWriter? _sharedLatestWriter;
    private static readonly object _latestLock = new();

    public LogApi(string modName, string logsDir)
    {
        _modName = modName;

        try
        {
            Directory.CreateDirectory(logsDir);

            lock (_latestLock)
            {
                if (_sharedLatestWriter == null)
                {
                    var latestPath = Path.Combine(logsDir, "latest.log");
                    _sharedLatestWriter = new StreamWriter(latestPath, append: false) { AutoFlush = true };
                }
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
            var archivePath = Path.Combine(logsDir, $"{timestamp}.log");
            _archiveWriter = new StreamWriter(archivePath, append: true) { AutoFlush = true };
        }
        catch
        {
            // ignored
        }
    }

    public void NewLine()
    {
        LogUtil.NewLine();
    }

    public void Divider(char divider = '-', int length = 27)
    {
        LogUtil.Divider(divider, length);
    }

    public void Info(string msg)
    {
        var text = Format(msg);
        LogUtil.Info(text, Plugin.Logger);
        WriteToFile("INFO", text);
    }

    public void Error(string msg)
    {
        var text = Format(msg);
        LogUtil.Error(text, Plugin.Logger);
        WriteToFile("ERROR", text);
    }

    public void Warning(string msg)
    {
        var text = Format(msg);
        LogUtil.Warning(text, Plugin.Logger);
        WriteToFile("WARNING", text);
    }

    public void Debug(string msg)
    {
        var text = Format(msg);
        LogUtil.Debug(text, Plugin.Logger);
        WriteToFile("DEBUG", text);
    }

    public void Message(string msg)
    {
        var text = Format(msg);
        LogUtil.Message(text, Plugin.Logger);
        WriteToFile("MESSAGE", text);
    }

    private string Format(string msg)
    {
        return $"[{_modName}] {msg}";
    }

    private void WriteToFile(string type, string text)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] [{type}] {text}";
        lock (_latestLock)
        {
            try
            {
                _sharedLatestWriter?.WriteLine(line);
            }
            catch
            {
                /* ignored */
            }
        }

        try
        {
            _archiveWriter?.WriteLine(line);
        }
        catch
        {
            /* ignored */
        }
    }
}