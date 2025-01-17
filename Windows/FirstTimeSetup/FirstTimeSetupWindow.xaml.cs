using AudioReplacer.Windows.FirstTimeSetup.Pages;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace AudioReplacer.Windows.FirstTimeSetup;
public sealed partial class FirstTimeSetupWindow
{
    public FirstTimeSetupWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        SystemBackdrop = MicaController.IsSupported() ? new MicaBackdrop() : new DesktopAcrylicBackdrop();

        // Open first page of setup
        MainFrame.Navigate(typeof(SetupWelcome));
    }

    public Frame GetMainFrame()
    {
        return MainFrame;
    }
}
