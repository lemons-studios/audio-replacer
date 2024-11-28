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

        private string GetEditorText() { return CustomDataEditor.Editor.GetText(CustomDataEditor.Editor.TextLength); }
    }
}
