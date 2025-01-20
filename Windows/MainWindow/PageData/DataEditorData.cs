using AudioReplacer.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
namespace AudioReplacer.Windows.MainWindow.PageData;

public partial class DataEditorData : ObservableObject
{
    [RelayCommand]
    private async Task ReloadChanges()
    {
        var confirmSave = new ContentDialog
        {
            Title = "Reload?",
            Content = "In order to load your changes, the app must restart. Restart now?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        var result = await confirmSave.ShowAsync();
        if (result != ContentDialogResult.Primary) return;
        Generic.RestartApp();
    }


    [RelayCommand]
    private void ImportPitch()
    {

    }
    [RelayCommand]
    private void ImportEffects()
    {

    }
}

