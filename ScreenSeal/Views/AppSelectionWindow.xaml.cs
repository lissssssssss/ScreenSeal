using System.Windows;
using System.Windows.Controls;
using ScreenSeal.Models;
using ScreenSeal.Services;

namespace ScreenSeal.Views;

public partial class AppSelectionWindow : Window
{
    private readonly ConfigService _config;
    private readonly Dictionary<string, CheckBox> _appChecks = new(StringComparer.OrdinalIgnoreCase);

    public event Action? SelectionSaved;
    public event Action? StartPreciseMaskRequested;

    public AppSelectionWindow(ConfigService config)
    {
        InitializeComponent();
        _config = config;
        BuildAppList();
        LoadFromConfig();
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

    public void LoadFromConfig()
    {
        var enabled = PrivacyAppCatalog.GetEffectiveEnabledIds(_config.Current);
        foreach (var (id, check) in _appChecks)
            check.IsChecked = enabled.Contains(id);
    }

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

    private void OnSave(object sender, RoutedEventArgs e)
    {
        SaveSelection();
        SelectionSaved?.Invoke();
    }

    private void OnStartPreciseMask(object sender, RoutedEventArgs e)
    {
        SaveSelection();
        SelectionSaved?.Invoke();
        StartPreciseMaskRequested?.Invoke();
        Hide();
    }

    private void SaveSelection()
    {
        var enabled = _appChecks
            .Where(kv => kv.Value.IsChecked == true)
            .Select(kv => kv.Key)
            .ToList();

        _config.Update(c => c.EnabledAppIds = enabled);
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        SaveSelection();
        e.Cancel = true;
        Hide();
    }
}
