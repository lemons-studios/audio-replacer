using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AudioReplacer.Pages
{
    public sealed partial class DataEditor
    {
        private readonly string configFolder = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\AudioReplacer2-Config";
        private readonly string pitchDataFile, effectsDataFile;
        private string modifiedPitchDataContents = "", modifiedEffectsContents = "";
        private readonly bool isStarting = true;

        public DataEditor()
        {
            InitializeComponent();
            pitchDataFile = $@"{configFolder}\PitchData.json";
            effectsDataFile = $@"{configFolder}\EffectsData.json";

            CustomDataEditor.Editor.SetText(File.ReadAllText(pitchDataFile));
            isStarting = false;
        }

        private async void SaveData(object sender, RoutedEventArgs e)
        {
            string editorContent = GetEditorText();
            var confirmSave = new ContentDialog { Title = "Save Data?", Content = "App will restart", PrimaryButtonText = "Save", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot };
            var result = await confirmSave.ShowAsync();
            if (result != ContentDialogResult.Primary) return;
            switch (SelectedFile.SelectedIndex)
            {
                case 0:
                    await File.WriteAllTextAsync(pitchDataFile, editorContent);
                    if (modifiedEffectsContents != string.Empty) await File.WriteAllTextAsync(effectsDataFile, modifiedEffectsContents);
                    break;
                case 1:
                    await File.WriteAllTextAsync(effectsDataFile, editorContent);
                    if (modifiedPitchDataContents != string.Empty) await File.WriteAllTextAsync(pitchDataFile, modifiedPitchDataContents);
                    break;
            }
            Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
        }

        private async void DiscardChanges(object sender, RoutedEventArgs e)
        {
            var confirmDiscard = new ContentDialog { Title = "Discard Changes?", Content = "File will reset to last save", PrimaryButtonText = "Discard", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot };
            var result = await confirmDiscard.ShowAsync();
            if(result == ContentDialogResult.Primary) CustomDataEditor.Editor.SetText(await File.ReadAllTextAsync(pitchDataFile));
        }

        private void UpdateEditingFile(object sender, SelectionChangedEventArgs e)
        {
            if (isStarting) return;
            switch (SelectedFile.SelectedIndex)
            {
                case 0:
                    // Navigating from effects file to pitch file
                    modifiedEffectsContents = GetEditorText();
                    CustomDataEditor.Editor.SetText(modifiedPitchDataContents == string.Empty ? File.ReadAllText(pitchDataFile) : modifiedPitchDataContents);
                    break;
                case 1:
                    // Navigating from pitch file to effects file
                    modifiedPitchDataContents = GetEditorText();
                    CustomDataEditor.Editor.SetText(modifiedEffectsContents == string.Empty ? File.ReadAllText(effectsDataFile) : modifiedEffectsContents);
                    break;
            }
        }
        
        private async void ImportFile(object sender, RoutedEventArgs e)
        {
            var importType = new ContentDialog { Title = "Choose Import File", Content = "What data is the imported data meant for? App will restart after import", PrimaryButtonText = "Pitch Data", SecondaryButtonText = "Effect Data", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot, Width = 500};
            var result = await importType.ShowAsync();
            if (result == ContentDialogResult.None) return;

            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            var window = App.MainWindow;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            openPicker.FileTypeFilter.Add(".json");
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                string fileContents = await File.ReadAllTextAsync(file.Path);
                switch (result == ContentDialogResult.Primary)
                {
                    case true:
                        await File.WriteAllTextAsync(pitchDataFile, fileContents);
                        break;
                    case false:
                        await File.WriteAllTextAsync(effectsDataFile, fileContents);
                        break;
                }
                Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
            }
        }

        private string GetEditorText() { return CustomDataEditor.Editor.GetText(CustomDataEditor.Editor.TextLength); }

        private void GetHelp(object sender, RoutedEventArgs e)
        {
            // To be implemented when I have written a guide later; this shouldn't really be needed unless the user doesn't know how to operate with json
        }
    }
}
