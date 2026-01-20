#region

using System.Diagnostics;
using System.Text;

#endregion

namespace Common;

public static class Utils
{
    private static string? _currentDir;
    
    public static string CurrentDir
    {
        get
        {
            if(_currentDir != null)
                return _currentDir;
            var currentDir = Environment.ProcessPath ?? throw new DirectoryNotFoundException();
            _currentDir = Path.GetDirectoryName(currentDir) ?? throw new DirectoryNotFoundException();
            return _currentDir;
        }
    }
    
    public static bool RunExe(string exePath, string exeArgs, int timeoutSeconds)
    {
        StringBuilder output = new(), error = new();
        var isSuccess = false;

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = exeArgs,
            WorkingDirectory = Path.GetDirectoryName(exePath),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process();
        process.StartInfo = psi;
        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null) output.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null) error.AppendLine(e.Data);
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (process.WaitForExit(timeoutSeconds * 1000))
            {
                isSuccess = true;
            }
            else if (!process.HasExited)
            {
                process.Kill();
                throw new Exception($"error: {exeArgs}");
            }
        }
        catch (Exception ex)
        {
            error.AppendLine(ex.Message);
        }

        return isSuccess;
    }
}