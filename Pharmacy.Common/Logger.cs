using System.Diagnostics;

namespace Pharmacy.Common;

public static class Logger
{
    public static void Info(string message) => Trace.WriteLine($"[INFO] {DateTime.Now:O} {message}");

    public static void Warn(string message) => Trace.WriteLine($"[WARN] {DateTime.Now:O} {message}");

    public static void Error(string message, Exception? ex = null)
    {
        Trace.WriteLine($"[ERROR] {DateTime.Now:O} {message}");
        if (ex != null)
            Trace.WriteLine(ex.ToString());
    }
}
