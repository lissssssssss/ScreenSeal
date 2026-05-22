namespace ScreenSeal.Models;

public sealed class AppConfig
{
    public bool RunAtStartup { get; set; }
    public MaskStyle DefaultMaskStyle { get; set; } = MaskStyle.SolidBlack;
    public double MaskOpacity { get; set; } = 0.92;
    public int PollIntervalMs { get; set; } = 1500;
    public int IdlePollIntervalMs { get; set; } = 3000;
    public HotkeyConfig Hotkeys { get; set; } = new();
    /// <summary>null 表示尚未配置（默认全部启用）；空列表表示用户未选择任何应用。</summary>
    public List<string>? EnabledAppIds { get; set; }
    public List<string> CustomProcessNames { get; set; } = [];
    public List<string> CustomWindowTitleKeywords { get; set; } = [];
}

public sealed class HotkeyConfig
{
    public string TogglePreciseMask { get; set; } = "Ctrl+Shift+Q";
    public string ToggleCustomRegion { get; set; } = "Ctrl+Shift+W";
    public string UnlockAll { get; set; } = "Ctrl+Shift+E";
}

public enum MaskStyle
{
    SolidBlack,
    Blur
}

public enum OverlayMode
{
    None,
    Precise,
    CustomRegion,
    FullScreenBlur
}
