using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage.Pickers;
using WinRT.Interop;
using WinUIEditor;

namespace AudioReplacer.MainWindow.PageData;

public partial class DataEditorData : ObservableObject
{
    public static CodeEditorControl CodeEditor;
    
    [RelayCommand]
    private void OpenHelpPage()
    {
        AppFunctions.OpenUrl("https://github.com/lemons-studios/audio-replacer/wiki");
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
        var formattedText = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(currentEditorText),
            new JsonSerializerOptions { WriteIndented = true });
        CodeEditor.Editor.SetText(formattedText);
    }

    [RelayCommand]
    private void Reload()
    {
        AppFunctions.RestartApp();
    }

    [Log]
    private async Task ImportFile(bool isEffects)
    {
        var copyPath = isEffects ? AppProperties.EffectsDataFile : AppProperties.PitchDataFile;

        var openPicker = new FileOpenPicker();
        InitializeWithWindow.Initialize(openPicker, WindowNative.GetWindowHandle(App.MainWindow));
        openPicker.FileTypeFilter.Add(".json");
        var file = await openPicker.PickSingleFileAsync();
        if (file != null)
        {
            File.Copy(file.Path, copyPath, overwrite: true);
            AppFunctions.RestartApp();
        }
    }
}

