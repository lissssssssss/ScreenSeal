using ScreenSeal.Services.Native;

namespace ScreenSeal.Services;

public sealed class WindowLockService
{
    private readonly HashSet<IntPtr> _locked = new();

    public IReadOnlyCollection<IntPtr> LockedHandles => _locked;

    public void Lock(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return;
        User32.EnableWindow(hwnd, false);
        _locked.Add(hwnd);
    }

    public void LockMany(IEnumerable<IntPtr> handles)
    {
        foreach (var h in handles)
            Lock(h);
    }

    public void Unlock(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return;
        User32.EnableWindow(hwnd, true);
        _locked.Remove(hwnd);
    }

    public void UnlockAll()
    {
        foreach (var h in _locked.ToList())
            User32.EnableWindow(h, true);
        _locked.Clear();
    }
}
