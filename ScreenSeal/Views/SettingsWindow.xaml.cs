using System.Windows;
using ScreenSeal.Models;
using ScreenSeal.Services;

namespace ScreenSeal.Views;

public partial class SettingsWindow : Window
{
    private readonly ConfigService _config;
    public event Action? SettingsSaved;

    public SettingsWindow(ConfigService config)
    {
        InitializeComponent();
        _config = config;
        LoadFromConfig();
        OpacitySlider.ValueChanged += (_, _) =>
            OpacityLabel.Text = $"{OpacitySlider.Value:P0}";
    }

    private void LoadFromConfig()
    {
        var c = _config.Current;
        HotkeyPrecise.Text = c.Hotkeys.TogglePreciseMask;
        HotkeyCustom.Text = c.Hotkeys.ToggleCustomRegion;
        HotkeyUnlock.Text = c.Hotkeys.UnlockAll;
        MaskStyleCombo.SelectedIndex = c.DefaultMaskStyle == MaskStyle.Blur ? 1 : 0;
        OpacitySlider.Value = c.MaskOpacity;
        OpacityLabel.Text = $"{c.MaskOpacity:P0}";
        StartupCheck.IsChecked = c.RunAtStartup;
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        _config.Update(c =>
        {
            c.Hotkeys.TogglePreciseMask = HotkeyPrecise.Text.Trim();
            c.Hotkeys.ToggleCustomRegion = HotkeyCustom.Text.Trim();
            c.Hotkeys.UnlockAll = HotkeyUnlock.Text.Trim();
            c.DefaultMaskStyle = MaskStyleCombo.SelectedIndex == 1
                ? MaskStyle.Blur
                : MaskStyle.SolidBlack;
            c.MaskOpacity = OpacitySlider.Value;
            c.RunAtStartup = StartupCheck.IsChecked == true;
        });

        StartupService.SetEnabled(_config.Current.RunAtStartup);
        SettingsSaved?.Invoke();
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e) => Close();
}
