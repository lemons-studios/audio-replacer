using AudioReplacer.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.IO;
using WinUIEditor;

namespace AudioReplacer.Windows.MainWindow.Pages;
public sealed partial class DataEditor
{
    private bool isLoaded;
    public DataEditor()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        CustomDataEditor.Editor.Modified += AutoSave;
    }

    private void AutoSave(Editor sender, ModifiedEventArgs args)
    {
        if (!isLoaded) return;
        var editorContent = GetEditorText();
        var currentFilePath = GetEditingFilePath();
        File.WriteAllText(currentFilePath, editorContent);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CustomDataEditor.Editor.SetText(File.ReadAllText(Generic.PitchDataFile));
        App.DiscordController.SetDetails("In the data editor");
        App.DiscordController.SetState("");
        isLoaded = true;
    }

    private async void ReloadChanges(object sender, RoutedEventArgs e)
    {
        if (!isLoaded) return;
        var confirmSave = new ContentDialog
        {
            Title = "Reload?",
            Content = "In order to load your changes, the app must restart. Restart now?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            XamlRoot = Content.XamlRoot
        };

        var result = await confirmSave.ShowAsync();
        if (result != ContentDialogResult.Primary) return;
        Generic.RestartApp();
    }

    private void UpdateEditingFile(object sender, SelectionChangedEventArgs e)
    {
        if (!isLoaded) return;
        var currentFilePath = GetEditingFilePath();
        CustomDataEditor.Editor.SetText(File.ReadAllText(currentFilePath));
    }

    private void FormatData(object sender, RoutedEventArgs e)
    {
        if (!isLoaded) return;
        var currentEditorText = GetEditorText();
        CustomDataEditor.Editor.SetText(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(currentEditorText), Formatting.Indented));
    }

    private void Undo(object sender, RoutedEventArgs e)
    {
        if (!isLoaded) return;
        CustomDataEditor.Editor.Undo();
    }

    private void Redo(object sender, RoutedEventArgs e)
    {
        if (!isLoaded) return;
        CustomDataEditor.Editor.Redo();
    }

    private string GetEditorText()
    {
        if (!isLoaded) return string.Empty;
        try
        {
            return CustomDataEditor.Editor.GetText(CustomDataEditor.Editor.TextLength);
        }
        catch (NullReferenceException)
        {
            return string.Empty;
        }
    }

    private string GetEditingFilePath()
    {
        return SelectedFile.SelectedIndex == 0
            ? Generic.PitchDataFile
            : Generic.EffectsDataFile;
    }
}
