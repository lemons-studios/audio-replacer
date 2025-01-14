using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
using WinUIEx;

namespace AudioReplacer.Windows.FirstTimeSetup;

public sealed partial class FirstTimeSetupWindow  : WindowEx
{
    public FirstTimeSetupWindow()
    {
        InitializeComponent();
        this.ExtendsContentIntoTitleBar = true;
        SystemBackdrop = MicaController.IsSupported() ? new MicaBackdrop() : new DesktopAcrylicBackdrop();
    }
}

