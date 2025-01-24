using System;
using System.Collections.Generic;
using System.Drawing.Imaging.Effects;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioReplacer.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AudioReplacer.Windows.MainWindow.PageData;

internal partial class DataEditorData : ObservableObject
{
    [RelayCommand]
    private void OpenHelpPage()
    {
        Generic.OpenUrl("https://github.com/lemons-studios/audio-replacer/wiki");
    }

    [RelayCommand]
    private async Task ImportPitch()
    {
        await ImportFile(false);
    }

    [RelayCommand]
    private async Task ImportEffects()
    {
        await ImportFile(true);
    }

    private async Task ImportFile(bool isEffects)
    {
        var copyPath = isEffects ? Generic.EffectsDataFile : Generic.PitchDataFile;

        var openPicker = new FileOpenPicker();
        InitializeWithWindow.Initialize(openPicker, WindowNative.GetWindowHandle(App.MainWindow));
        openPicker.FileTypeFilter.Add(".json");
        var file = await openPicker.PickSingleFileAsync();
        if (file != null)
        {
            File.Copy(file.Path, copyPath, overwrite: true);
            Generic.RestartApp();
        }
    }
}

