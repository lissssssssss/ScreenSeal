using System.Diagnostics;
using System.Windows;
using ScreenSeal.Models;
using ScreenSeal.Services.Native;

namespace ScreenSeal.Services;

public sealed class WindowEnumerator
{
    private readonly ConfigService _config;

    public WindowEnumerator(ConfigService config) => _config = config;

    public IReadOnlyList<WindowInfo> FindPrivacyWindows()
    {
        var results = new List<WindowInfo>();
        var enabledIds = PrivacyAppCatalog.GetEffectiveEnabledIds(_config.Current);

        var processNames = PrivacyAppCatalog.All
            .Where(p => enabledIds.Contains(p.Id))
            .SelectMany(p => p.ProcessNames)
            .Concat(_config.Current.CustomProcessNames)
            .Select(n => n.Trim())
            .Where(n => n.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var titleKeywords = PrivacyAppCatalog.All
            .Where(p => enabledIds.Contains(p.Id))
            .SelectMany(p => p.TitleKeywords)
            .Concat(_config.Current.CustomWindowTitleKeywords)
            .Select(k => k.Trim())
            .Where(k => k.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        User32.EnumWindows((hWnd, _) =>
        {
            if (!User32.IsWindowVisible(hWnd)) return true;
            if (!User32.GetWindowRect(hWnd, out var rect)) return true;

            var bounds = rect.ToRect();
            if (bounds.Width < 80 || bounds.Height < 80) return true;

            var title = User32.GetWindowTitle(hWnd);
            User32.GetWindowThreadProcessId(hWnd, out var pid);
            var processName = GetProcessName((int)pid);

            if (!Matches(processName, title, processNames, titleKeywords)) return true;

            results.Add(new WindowInfo
            {
                Handle = hWnd,
                Title = title,
                ProcessName = processName,
                ProcessId = (int)pid,
                Bounds = bounds,
                IsTopmost = User32.IsTopmost(hWnd),
                IsVisible = true
            });

            return true;
        }, IntPtr.Zero);

        return results;
    }

    private static bool Matches(
        string processName,
        string title,
        HashSet<string> processNames,
        List<string> titleKeywords)
    {
        if (processNames.Contains(processName)) return true;
        return titleKeywords.Any(k =>
            title.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetProcessName(int processId)
    {
        try
        {
            return Process.GetProcessById(processId).ProcessName;
        }
        catch
        {
            return string.Empty;
        }
    }
}
