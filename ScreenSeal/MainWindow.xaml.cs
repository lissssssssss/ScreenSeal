using System.Windows;
using System.Windows.Interop;
using ScreenSeal.Models;
using ScreenSeal.Services;
using ScreenSeal.Views;

namespace ScreenSeal;

public partial class MainWindow : Window
{
    private readonly ConfigService _config = new();
    private readonly WindowEnumerator _enumerator;
    private readonly WindowLockService _lockService = new();
    private readonly HotkeyService _hotkeys = new();
    private readonly OverlayManager _overlayManager;
    private SettingsWindow? _settingsWindow;

    public MainWindow()
    {
        InitializeComponent();
        _enumerator = new WindowEnumerator(_config);
        _overlayManager = new OverlayManager(_config, _enumerator, _lockService);
        _overlayManager.ModeChanged += OnOverlayModeChanged;

        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        helper.EnsureHandle();

        _hotkeys.Attach(helper.Handle);
        _hotkeys.HotkeyPressed += OnHotkeyPressed;
        RegisterHotkeys();

        if (_config.Current.RunAtStartup != StartupService.IsEnabled())
            StartupService.SetEnabled(_config.Current.RunAtStartup);

        UpdateTrayTooltip();
        StartupMenuItem.IsChecked = _config.Current.RunAtStartup;
    }

    private void RegisterHotkeys()
    {
        _hotkeys.UnregisterAll();
        var h = _config.Current.Hotkeys;
        _hotkeys.Register(HotkeyService.IdTogglePrecise, h.TogglePreciseMask);
        _hotkeys.Register(HotkeyService.IdToggleCustomRegion, h.ToggleCustomRegion);
        _hotkeys.Register(HotkeyService.IdUnlockAll, h.UnlockAll);
    }

    private void OnHotkeyPressed(int id)
    {
        Dispatcher.Invoke(() =>
        {
            switch (id)
            {
                case HotkeyService.IdTogglePrecise:
                    _overlayManager.TogglePreciseMask();
                    break;
                case HotkeyService.IdToggleCustomRegion:
                    _overlayManager.StartCustomRegionSelection();
                    break;
                case HotkeyService.IdUnlockAll:
                    _overlayManager.StopAll();
                    break;
            }
            UpdateTrayTooltip();
        });
    }

    private void OnOverlayModeChanged(OverlayMode mode) =>
        Dispatcher.Invoke(UpdateTrayTooltip);

    private void UpdateTrayTooltip()
    {
        var mode = _overlayManager.CurrentMode;
        TrayIcon.ToolTipText = mode switch
        {
            OverlayMode.Precise => "屏谧 - IM 精准遮挡已开启",
            OverlayMode.CustomRegion => "屏谧 - 自定义区域遮挡",
            OverlayMode.FullScreenBlur => "屏谧 - 全屏模糊遮挡",
            _ => "屏谧 ScreenSeal - 就绪"
        };
    }

    private void OnTrayDoubleClick(object sender, RoutedEventArgs e) =>
        _overlayManager.TogglePreciseMask();

    private void OnTogglePrecise(object sender, RoutedEventArgs e)
    {
        _overlayManager.TogglePreciseMask();
        UpdateTrayTooltip();
    }

    private void OnCustomRegion(object sender, RoutedEventArgs e)
    {
        _overlayManager.StartCustomRegionSelection();
        UpdateTrayTooltip();
    }

    private void OnFullScreenBlur(object sender, RoutedEventArgs e)
    {
        _overlayManager.ToggleFullScreenBlur();
        UpdateTrayTooltip();
    }

    private void OnUnlockAll(object sender, RoutedEventArgs e)
    {
        _overlayManager.StopAll();
        UpdateTrayTooltip();
    }

    private void OnSettings(object sender, RoutedEventArgs e)
    {
        if (_settingsWindow?.IsVisible == true)
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(_config);
        _settingsWindow.SettingsSaved += () =>
        {
            RegisterHotkeys();
            StartupMenuItem.IsChecked = _config.Current.RunAtStartup;
        };
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
    }

    private void OnToggleStartup(object sender, RoutedEventArgs e)
    {
        var enabled = StartupMenuItem.IsChecked;
        _config.Update(c => c.RunAtStartup = enabled);
        StartupService.SetEnabled(enabled);
    }

    private void OnExit(object sender, RoutedEventArgs e) =>
        Application.Current.Shutdown();

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _overlayManager.Dispose();
        _hotkeys.Dispose();
        TrayIcon.Dispose();
    }
}
