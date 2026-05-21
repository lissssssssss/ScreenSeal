using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScreenSeal.Views;

public partial class RegionSelectorWindow : Window
{
    private Point _start;
    private bool _dragging;

    public Rect? SelectedRegion { get; private set; }

    public RegionSelectorWindow()
    {
        InitializeComponent();
        RootCanvas.MouseLeftButtonDown += OnMouseDown;
        RootCanvas.MouseMove += OnMouseMove;
        RootCanvas.MouseLeftButtonUp += OnMouseUp;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _start = e.GetPosition(RootCanvas);
        _dragging = true;
        RootCanvas.CaptureMouse();
        Canvas.SetLeft(SelectionRect, _start.X);
        Canvas.SetTop(SelectionRect, _start.Y);
        SelectionRect.Width = 0;
        SelectionRect.Height = 0;
        SelectionRect.Visibility = Visibility.Visible;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_dragging) return;
        var pos = e.GetPosition(RootCanvas);
        var x = Math.Min(_start.X, pos.X);
        var y = Math.Min(_start.Y, pos.Y);
        var w = Math.Abs(pos.X - _start.X);
        var h = Math.Abs(pos.Y - _start.Y);
        Canvas.SetLeft(SelectionRect, x);
        Canvas.SetTop(SelectionRect, y);
        SelectionRect.Width = w;
        SelectionRect.Height = h;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_dragging) return;
        _dragging = false;
        RootCanvas.ReleaseMouseCapture();

        var pos = e.GetPosition(RootCanvas);
        var screenPoint = PointToScreen(pos);
        var screenStart = PointToScreen(_start);

        var x = Math.Min(screenStart.X, screenPoint.X);
        var y = Math.Min(screenStart.Y, screenPoint.Y);
        var w = Math.Abs(screenPoint.X - screenStart.X);
        var h = Math.Abs(screenPoint.Y - screenStart.Y);

        if (w >= 20 && h >= 20)
        {
            SelectedRegion = new Rect(x, y, w, h);
            DialogResult = true;
            Close();
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
        else if (e.Key == Key.Enter && SelectedRegion != null)
        {
            DialogResult = true;
            Close();
        }
    }
}
