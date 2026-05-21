using System.Windows;

namespace ScreenSeal.Models;

public sealed class WindowInfo
{
    public required IntPtr Handle { get; init; }
    public required string Title { get; init; }
    public required string ProcessName { get; init; }
    public required int ProcessId { get; init; }
    public required Rect Bounds { get; init; }
    public bool IsTopmost { get; init; }
    public bool IsVisible { get; init; }
}
