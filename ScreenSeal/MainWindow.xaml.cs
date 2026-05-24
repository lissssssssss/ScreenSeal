using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
    private readonly Dictionary<string, CheckBox> _appChecks = new(StringComparer.OrdinalIgnoreCase);
    private SettingsWindow? _settingsWindow;
    private bool _isExiting;

    public MainWindow()
    {
        InitializeComponent();
        BuildAppList();
        _enumerator = new WindowEnumerator(_config);
        _overlayManager = new OverlayManager(_config, _enumerator, _lockService);
        _overlayManager.ModeChanged += OnOverlayModeChanged;

        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void BuildAppList()
    {
        foreach (var preset in PrivacyAppCatalog.All)
        {
            var check = new CheckBox
            {
                Content = preset.DisplayName,
                Tag = preset.Id,
                Margin = new Thickness(0, 6, 0, 6),
                FontSize = 14
            };
            _appChecks[preset.Id] = check;
            AppListPanel.Children.Add(check);
        }
    }

    private void LoadAppSelectionFromConfig()
    {
        var enabled = PrivacyAppCatalog.GetEffectiveEnabledIds(_config.Current);
        foreach (var (id, check) in _appChecks)
            check.IsChecked = enabled.Contains(id);
    }

    private void SaveAppSelection()
    {
        var enabled = _appChecks
            .Where(kv => kv.Value.IsChecked == true)
            .Select(kv => kv.Key)
            .ToList();

        _config.Update(c => c.EnabledAppIds = enabled);
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

        LoadAppSelectionFromConfig();
        UpdateTrayTooltip();
        StartupMenuItem.IsChecked = _config.Current.RunAtStartup;

        Show();
        Activate();
        WindowState = WindowState.Normal;
    }

    public void ShowMainWindow()
    {
        LoadAppSelectionFromConfig();
        ShowInTaskbar = true;
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_isExiting)
        {
            _overlayManager.Dispose();
            _hotkeys.Dispose();
            TrayIcon.Dispose();
            return;
        }

        SaveAppSelection();
        e.Cancel = true;
        Hide();
        ShowInTaskbar = false;
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

    private void OnTrayDoubleClick(object sender, RoutedEventArgs e) => ShowMainWindow();

    private void OnShowMainWindow(object sender, RoutedEventArgs e) => ShowMainWindow();

    private void OnSelectAll(object sender, RoutedEventArgs e)
    {
        foreach (var check in _appChecks.Values)
            check.IsChecked = true;
    }

    private void OnSelectNone(object sender, RoutedEventArgs e)
    {
        foreach (var check in _appChecks.Values)
            check.IsChecked = false;
    }

    private void OnSaveSelection(object sender, RoutedEventArgs e) => SaveAppSelection();

    private void OnStartPreciseMask(object sender, RoutedEventArgs e)
    {
        SaveAppSelection();
        _overlayManager.TogglePreciseMask();
        UpdateTrayTooltip();
        Hide();
        ShowInTaskbar = false;
    }

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

    private void OnExit(object sender, RoutedEventArgs e)
    {
        _isExiting = true;
        Application.Current.Shutdown();
    }
}
