using AudioReplacer.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;
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
        string editorContent = GetEditorText();
        string currentFilePath = GetEditingFilePath();
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

    private async void DiscardChanges(object sender, RoutedEventArgs e)
    {
        if (!isLoaded) return;
        var confirmDiscard = new ContentDialog
        {
            Title = "Discard Changes?",
            Content = "File will reset to last save",
            PrimaryButtonText = "Discard",
            CloseButtonText = "Cancel",
            XamlRoot = Content.XamlRoot
        };
        var result = await confirmDiscard.ShowAsync();
        if (result == ContentDialogResult.Primary)
            CustomDataEditor.Editor.SetText(await File.ReadAllTextAsync(Generic.PitchDataFile));
    }

    private void UpdateEditingFile(object sender, SelectionChangedEventArgs e)
    {
        if (!isLoaded) return;
        string currentFilePath = GetEditingFilePath();
        CustomDataEditor.Editor.SetText(File.ReadAllText(currentFilePath));
    }

    private void ImportEffects(object sender, RoutedEventArgs e)
    {
        Task.Run(() => ImportFile(true));
    }

    private void ImportPitch(object sender, RoutedEventArgs e)
    {
       Task.Run(() => ImportFile(false));
    }

    private async Task ImportFile(bool effect)
    {
        if (!isLoaded) return;
        var copyPath = effect ? Generic.EffectsDataFile : Generic.PitchDataFile;

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

    private void FormatData(object sender, RoutedEventArgs e)
    {
        if (!isLoaded) return;
        string currentEditorText = GetEditorText();
        CustomDataEditor.Editor.SetText(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(currentEditorText),
            Formatting.Indented));
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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        App.DiscordController.SetState("In the data editor");
    }

    private void OpenHelpPage(object sender, RoutedEventArgs e)
    {
        Generic.OpenUrl("https://github.com/lemons-studios/audio-replacer/wiki");
    }
}
