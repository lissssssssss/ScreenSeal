using System.Windows;
using System.Windows.Threading;
using ScreenSeal.Models;
using ScreenSeal.Views;

namespace ScreenSeal.Services;

public sealed class OverlayManager : IDisposable
{
    private readonly ConfigService _config;
    private readonly WindowEnumerator _enumerator;
    private readonly WindowLockService _lockService;
    private readonly List<OverlayWindow> _overlays = [];
    private readonly DispatcherTimer _pollTimer;
    private OverlayMode _mode = OverlayMode.None;
    private bool _preciseActive;

    public OverlayMode CurrentMode => _mode;

    public event Action<OverlayMode>? ModeChanged;

    public OverlayManager(
        ConfigService config,
        WindowEnumerator enumerator,
        WindowLockService lockService)
    {
        _config = config;
        _enumerator = enumerator;
        _lockService = lockService;

        _pollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(_config.Current.PollIntervalMs)
        };
        _pollTimer.Tick += (_, _) => RefreshPreciseOverlays();
    }

    public void TogglePreciseMask()
    {
        if (_preciseActive)
        {
            StopPrecise();
            return;
        }

        StopAll();
        _preciseActive = true;
        _mode = OverlayMode.Precise;
        RefreshPreciseOverlays();
        _pollTimer.Interval = TimeSpan.FromMilliseconds(_config.Current.PollIntervalMs);
        _pollTimer.Start();
        ModeChanged?.Invoke(_mode);
    }

    public void StartCustomRegionSelection()
    {
        StopAll();
        var selector = new RegionSelectorWindow();
        if (selector.ShowDialog() != true || selector.SelectedRegion == null)
            return;

        CreateOverlay(selector.SelectedRegion.Value, OverlayMode.CustomRegion);
        _mode = OverlayMode.CustomRegion;
        ModeChanged?.Invoke(_mode);
    }

    public void ToggleFullScreenBlur()
    {
        if (_mode == OverlayMode.FullScreenBlur)
        {
            StopAll();
            return;
        }

        StopAll();
        var bounds = GetVirtualScreenBounds();
        CreateOverlay(bounds, OverlayMode.FullScreenBlur, fullScreen: true);
        _mode = OverlayMode.FullScreenBlur;
        ModeChanged?.Invoke(_mode);
    }

    public void StopAll()
    {
        _pollTimer.Stop();
        _preciseActive = false;

        foreach (var overlay in _overlays.ToList())
        {
            overlay.Close();
        }
        _overlays.Clear();
        _lockService.UnlockAll();

        if (_mode != OverlayMode.None)
        {
            _mode = OverlayMode.None;
            ModeChanged?.Invoke(_mode);
        }
    }

    private void StopPrecise()
    {
        _pollTimer.Stop();
        _preciseActive = false;
        ClearOverlays();
        _lockService.UnlockAll();
        _mode = OverlayMode.None;
        ModeChanged?.Invoke(_mode);
    }

    private void RefreshPreciseOverlays()
    {
        if (!_preciseActive) return;

        var windows = _enumerator.FindPrivacyWindows();
        var handles = windows.Select(w => w.Handle).ToHashSet();

        foreach (var overlay in _overlays.Where(o => o.TargetHandle != IntPtr.Zero).ToList())
        {
            if (!handles.Contains(overlay.TargetHandle))
            {
                _overlays.Remove(overlay);
                overlay.Close();
            }
        }

        foreach (var win in windows)
        {
            var existing = _overlays.FirstOrDefault(o => o.TargetHandle == win.Handle);
            if (existing != null)
            {
                existing.UpdateBounds(win.Bounds);
                continue;
            }

            var overlay = CreateOverlay(win.Bounds, OverlayMode.Precise, win.Handle);
            _lockService.Lock(win.Handle);
        }

        _pollTimer.Interval = windows.Count > 0
            ? TimeSpan.FromMilliseconds(_config.Current.PollIntervalMs)
            : TimeSpan.FromMilliseconds(_config.Current.IdlePollIntervalMs);
    }

    private OverlayWindow CreateOverlay(
        Rect bounds,
        OverlayMode mode,
        IntPtr targetHandle = default,
        bool fullScreen = false)
    {
        var overlay = new OverlayWindow(
            bounds,
            _config.Current.DefaultMaskStyle,
            _config.Current.MaskOpacity,
            mode,
            targetHandle,
            fullScreen);
        overlay.Show();
        _overlays.Add(overlay);
        return overlay;
    }

    private void ClearOverlays()
    {
        foreach (var o in _overlays.ToList())
            o.Close();
        _overlays.Clear();
    }

    private static Rect GetVirtualScreenBounds()
    {
        var left = SystemParameters.VirtualScreenLeft;
        var top = SystemParameters.VirtualScreenTop;
        var width = SystemParameters.VirtualScreenWidth;
        var height = SystemParameters.VirtualScreenHeight;
        return new Rect(left, top, width, height);
    }

    public void Dispose() => StopAll();
}
