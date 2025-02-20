using AudioReplacer.Setup.Pages;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace AudioReplacer.Setup;
public sealed partial class FirstTimeSetupWindow
{
    public FirstTimeSetupWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        SystemBackdrop = MicaController.IsSupported() ? new MicaBackdrop() : new DesktopAcrylicBackdrop();
        // Open first page of setup
        MainFrame.Navigate(typeof(SetupWelcome), null, new SlideNavigationTransitionInfo {Effect = SlideNavigationTransitionEffect.FromRight});
    }

    public Frame GetMainFrame()
    {
        return MainFrame;
    }
}
