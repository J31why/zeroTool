using System.Diagnostics;
using System.Text;

namespace Common;

public static class Utils
{
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
                isSuccess = true;
            else if (!process.HasExited)
                process.Kill();
        }
        catch (Exception ex)
        {
            error.AppendLine(ex.Message);
        }

        return isSuccess;
    }
}