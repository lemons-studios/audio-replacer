using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using System.Linq;
using Windows.Foundation;
using Windows.Graphics;
using WinRT.Interop;

namespace AudioReplacer.Util;
public class DragRegions // Stolen from random github repo
{
    private readonly AppWindow appWindow;
    private readonly FrameworkElement titleBar;
    private bool Extended { get { return appWindow.TitleBar.ExtendsContentIntoTitleBar; } }
    private readonly FrameworkElement[] nonDragElements;
    public FrameworkElement[] NonDragElements
    {
        get
        {
            return nonDragElements;
        }
        init
        {
            nonDragElements = value;
            UpdateTitleBarDragRegions();
        }
    }

    public DragRegions(Window mainWindow, FrameworkElement titleBar)
    {
        appWindow = GetAppWindow(mainWindow);
        this.titleBar = titleBar;

        appWindow.Changed += AppWindow_Changed;
        this.titleBar.Loaded += TitleBar_Loaded;
        this.titleBar.SizeChanged += TitleBar_SizeChanged;
    }

    ~DragRegions()
    {
        appWindow.Changed -= AppWindow_Changed;
        titleBar.Loaded -= TitleBar_Loaded;
        titleBar.SizeChanged -= TitleBar_SizeChanged;
    }

    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (!args.DidPresenterChange)
            return;

        switch (sender.Presenter.Kind)
        {
            case AppWindowPresenterKind.CompactOverlay:
                titleBar.Visibility = Visibility.Collapsed;
                sender.TitleBar.ResetToDefault();
                break;

            case AppWindowPresenterKind.FullScreen:
                titleBar.Visibility = Visibility.Collapsed;
                sender.TitleBar.ExtendsContentIntoTitleBar = true;
                break;

            case AppWindowPresenterKind.Overlapped:
                titleBar.Visibility = Visibility.Visible;
                sender.TitleBar.ExtendsContentIntoTitleBar = true;
                break;

            case AppWindowPresenterKind.Default:
                break;
            default:
                sender.TitleBar.ResetToDefault();
                break;
        }
    }

    private void TitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (Extended) 
            UpdateTitleBarDragRegions();
    }

    private void TitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        if (Extended) 
            UpdateTitleBarDragRegions();
    }

    private static AppWindow GetAppWindow(object mainWindow)
    {
        var hWnd = WindowNative.GetWindowHandle(mainWindow);
        var wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(wndId);
    }

    private void UpdateTitleBarDragRegions()
    {
        var xamlRoot = titleBar.XamlRoot;
        if (xamlRoot == null)
            return;

        var scaleAdjustment = xamlRoot.RasterizationScale;

        var rectArray = (from item in NonDragElements let transform = item.TransformToVisual(null) select transform.TransformBounds(new Rect(0, 0, item.ActualWidth, item.ActualHeight)) into bounds select GetRect(bounds, scaleAdjustment)).ToArray();
        var nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(appWindow.Id);
        nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);
    }

    private RectInt32 GetRect(Rect bounds, double scale)
    {
        return new RectInt32(
            _X: (int) Math.Round(bounds.X * scale),
            _Y: (int) Math.Round(bounds.Y * scale),
            _Width: (int) Math.Round(bounds.Width * scale),
            _Height: (int) Math.Round(bounds.Height * scale)
        );
    }
}

