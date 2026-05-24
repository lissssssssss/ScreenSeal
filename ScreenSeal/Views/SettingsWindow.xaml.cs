using System.Windows;
using System.Windows.Input;
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
        PollIntervalBox.Text = c.PollIntervalMs.ToString();
        IdlePollIntervalBox.Text = c.IdlePollIntervalMs.ToString();
        CustomProcessBox.Text = string.Join("\r\n", c.CustomProcessNames);
        CustomTitleBox.Text = string.Join("\r\n", c.CustomWindowTitleKeywords);
    }

    private void Hotkey_GotFocus(object sender, RoutedEventArgs e)
    {
        ((TextBox)sender).SelectAll();
    }

    private void Hotkey_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // Allow Tab to move focus, capture Esc as a hotkey key
        if (key == Key.Tab)
            return;

        e.Handled = true;

        // Ignore standalone modifier-only presses
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift
            or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin
            or Key.None or Key.Clear)
            return;

        var mods = new List<string>();
        if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0) mods.Add("Ctrl");
        if ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != 0) mods.Add("Shift");
        if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) != 0) mods.Add("Alt");
        if ((e.KeyboardDevice.Modifiers & ModifierKeys.Windows) != 0) mods.Add("Win");

        var keyText = GetKeyText(key);
        ((TextBox)sender).Text = mods.Count > 0
            ? $"{string.Join("+", mods)}+{keyText}"
            : keyText;

        ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length;
    }

    private static string GetKeyText(Key key)
    {
        return key switch
        {
            Key.D0 => "0", Key.D1 => "1", Key.D2 => "2", Key.D3 => "3", Key.D4 => "4",
            Key.D5 => "5", Key.D6 => "6", Key.D7 => "7", Key.D8 => "8", Key.D9 => "9",
            Key.NumPad0 => "0", Key.NumPad1 => "1", Key.NumPad2 => "2", Key.NumPad3 => "3",
            Key.NumPad4 => "4", Key.NumPad5 => "5", Key.NumPad6 => "6", Key.NumPad7 => "7",
            Key.NumPad8 => "8", Key.NumPad9 => "9",
            Key.Multiply => "*", Key.Add => "+", Key.Subtract => "-", Key.Divide => "/",
            Key.OemPlus => "+", Key.OemMinus => "-",
            Key.OemPeriod => ".", Key.OemComma => ",",
            Key.OemQuestion => "/", Key.OemBackslash => "\\",
            Key.OemOpenBrackets => "[", Key.OemCloseBrackets => "]",
            Key.OemQuotes => "'", Key.OemSemicolon => ";",
            Key.OemTilde => "`", Key.Space => "Space",
            Key.Tab => "Tab", Key.Enter => "Enter",
            Key.Escape => "Esc", Key.Back => "Backspace", Key.Delete => "Delete",
            Key.PageUp => "PageUp", Key.PageDown => "PageDown",
            Key.Left => "Left", Key.Right => "Right", Key.Up => "Up", Key.Down => "Down",
            Key.F1 => "F1", Key.F2 => "F2", Key.F3 => "F3", Key.F4 => "F4",
            Key.F5 => "F5", Key.F6 => "F6", Key.F7 => "F7", Key.F8 => "F8",
            Key.F9 => "F9", Key.F10 => "F10", Key.F11 => "F11", Key.F12 => "F12",
            Key.CapsLock => "CapsLock",
            _ => key.ToString()
        };
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

            if (int.TryParse(PollIntervalBox.Text, out var poll))
                c.PollIntervalMs = Math.Clamp(poll, 200, 10000);
            if (int.TryParse(IdlePollIntervalBox.Text, out var idle))
                c.IdlePollIntervalMs = Math.Clamp(idle, 500, 30000);

            c.CustomProcessNames = CustomProcessBox.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();
            c.CustomWindowTitleKeywords = CustomTitleBox.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();
        });

        StartupService.SetEnabled(_config.Current.RunAtStartup);
        SettingsSaved?.Invoke();
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e) => Close();
}
