using AudioReplacer.MainWindow.PageData;
using Microsoft.UI.Xaml.Controls;
using WinUIEditor;

namespace AudioReplacer.MainWindow.Pages;
public sealed partial class DataEditor
{
    private bool isLoaded;
    public DataEditor()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        CustomDataEditor.Editor.Modified += AutoSave;
    }

    [Log]
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CustomDataEditor.Editor.SetText(File.ReadAllText(GetEditingFilePath()));
        App.DiscordController.SetDetails("In the data editor");
        App.DiscordController.SetState("");
        App.DiscordController.SetSmallImage("");

        DataEditorData.CodeEditor = CustomDataEditor;
        isLoaded = true;
    }

    private void AutoSave(Editor sender, ModifiedEventArgs args)
    {
        if (!isLoaded) return;
        var editorContent = CustomDataEditor.Editor.GetText(CustomDataEditor.Editor.TextLength);
        var currentFilePath = GetEditingFilePath();
        File.WriteAllText(currentFilePath, editorContent);
    }

    private void UpdateEditingFile(object sender, SelectionChangedEventArgs e)
    {
        if (!isLoaded) return;
        var currentFilePath = GetEditingFilePath();
        CustomDataEditor.Editor.SetText(File.ReadAllText(currentFilePath));
    }

    private string GetEditingFilePath()
    {
        return SelectedFile.SelectedIndex == 0
            ? AppProperties.PitchDataFile
            : AppProperties.EffectsDataFile;
    }
}
