using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using ScreenSeal.Models;
using ScreenSeal.Services.Native;

namespace ScreenSeal.Views;

public partial class OverlayWindow : Window
{
    public IntPtr TargetHandle { get; }

    public OverlayWindow(
        Rect bounds,
        MaskStyle style,
        double opacity,
        OverlayMode mode,
        IntPtr targetHandle,
        bool fullScreen)
    {
        InitializeComponent();
        TargetHandle = targetHandle;

        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;

        ApplyStyle(style, opacity, mode, fullScreen);
        Loaded += OnLoaded;
    }

    public void UpdateBounds(Rect bounds)
    {
        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        var exStyle = User32.GetWindowLongPtr(helper.Handle, User32.GWL_EXSTYLE);
        var newStyle = (long)exStyle | User32.WS_EX_TOOLWINDOW | User32.WS_EX_NOACTIVATE;
        SetWindowLong(helper.Handle, User32.GWL_EXSTYLE, newStyle);

        User32.SetWindowPos(
            helper.Handle,
            User32.HWND_TOPMOST,
            0, 0, 0, 0,
            User32.SWP_NOMOVE | User32.SWP_NOSIZE | User32.SWP_NOACTIVATE | User32.SWP_SHOWWINDOW);
    }

    private void ApplyStyle(MaskStyle style, double opacity, OverlayMode mode, bool fullScreen)
    {
        if (style == MaskStyle.Blur || mode == OverlayMode.FullScreenBlur)
        {
            MaskBorder.Background = new SolidColorBrush(Color.FromArgb(
                (byte)(opacity * 180), 20, 20, 24));
            Blur.Radius = fullScreen ? 28 : 16;
            MaskBorder.Effect = Blur;
        }
        else
        {
            MaskBorder.Background = new SolidColorBrush(Color.FromArgb(
                (byte)(opacity * 255), 0, 0, 0));
            MaskBorder.Effect = null;
        }
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    private static void SetWindowLong(IntPtr hWnd, int nIndex, long value)
    {
        if (IntPtr.Size == 8)
            SetWindowLongPtr64(hWnd, nIndex, new IntPtr(value));
        else
            SetWindowLong32(hWnd, nIndex, (int)value);
    }
}
