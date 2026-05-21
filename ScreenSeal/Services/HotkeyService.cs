using System.Windows.Input;
using System.Windows.Interop;
using ScreenSeal.Services.Native;

namespace ScreenSeal.Services;

public sealed class HotkeyService : IDisposable
{
    public const int IdTogglePrecise = 1;
    public const int IdToggleCustomRegion = 2;
    public const int IdUnlockAll = 3;

    private readonly Dictionary<int, (uint Modifiers, uint Key)> _registered = new();
    private HwndSource? _source;
    private IntPtr _hwnd;

    public event Action<int>? HotkeyPressed;

    public void Attach(IntPtr hwnd)
    {
        _hwnd = hwnd;
        _source = HwndSource.FromHwnd(hwnd);
        if (_source != null)
            _source.AddHook(WndProc);
    }

    public bool Register(int id, string hotkeyText)
    {
        Unregister(id);
        if (!TryParse(hotkeyText, out var modifiers, out var key))
            return false;

        if (!User32.RegisterHotKey(_hwnd, id, modifiers, key))
            return false;

        _registered[id] = (modifiers, key);
        return true;
    }

    public void Unregister(int id)
    {
        if (_registered.Remove(id))
            User32.UnregisterHotKey(_hwnd, id);
    }

    public void UnregisterAll()
    {
        foreach (var id in _registered.Keys.ToList())
            Unregister(id);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == User32.WM_HOTKEY)
        {
            HotkeyPressed?.Invoke(wParam.ToInt32());
            handled = true;
        }
        return IntPtr.Zero;
    }

    public static bool TryParse(string text, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var parts = text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return false;

        for (var i = 0; i < parts.Length - 1; i++)
        {
            modifiers |= parts[i].ToUpperInvariant() switch
            {
                "CTRL" or "CONTROL" => User32.MOD_CONTROL,
                "SHIFT" => User32.MOD_SHIFT,
                "ALT" => User32.MOD_ALT,
                "WIN" => User32.MOD_WIN,
                _ => 0
            };
        }

        var keyPart = parts[^1];
        if (!Enum.TryParse<Key>(keyPart, true, out var wpfKey))
        {
            if (keyPart.Length != 1)
                return false;
            wpfKey = Key.None;
            foreach (Key k in Enum.GetValues(typeof(Key)))
            {
                if (k.ToString().Equals(keyPart, StringComparison.OrdinalIgnoreCase))
                {
                    wpfKey = k;
                    break;
                }
            }
            if (wpfKey == Key.None) return false;
        }

        vk = (uint)KeyInterop.VirtualKeyFromKey(wpfKey);
        return vk != 0;
    }

    public void Dispose()
    {
        UnregisterAll();
        _source?.RemoveHook(WndProc);
    }
}
