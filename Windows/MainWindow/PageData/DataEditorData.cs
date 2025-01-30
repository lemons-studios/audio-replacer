using AudioReplacer.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;
using WinUIEditor;

namespace AudioReplacer.Windows.MainWindow.PageData;

internal partial class DataEditorData : ObservableObject
{
    public static CodeEditorControl CodeEditor;
    
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

    [RelayCommand]
    private void Undo()
    {
        CodeEditor.Editor.Undo();
    }

    [RelayCommand]
    private void Redo()
    {
        CodeEditor.Editor.Redo();
    }

    [RelayCommand]
    private void FormatContent()
    {
        var currentEditorText = CodeEditor.Editor.GetText(CodeEditor.Editor.TextLength);
        CodeEditor.Editor.SetText(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(currentEditorText), Formatting.Indented));
    }

    [RelayCommand]
    private void Reload()
    {
        Generic.RestartApp();
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

