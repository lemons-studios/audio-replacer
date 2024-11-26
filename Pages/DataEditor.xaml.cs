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
        private bool starting = true;

        public DataEditor()
        {
            InitializeComponent();
            pitchDataFile = $@"{configFolder}\PitchData.json";
            effectsDataFile = $@"{configFolder}\EffectsData.json";

            CustomDataEditor.Editor.SetText(File.ReadAllText(pitchDataFile));
            starting = false;
        }

        private async void SaveFile(object sender, RoutedEventArgs e)
        {
            var confirmSave = new ContentDialog { Title = "Save Data?", Content = "App will restart", PrimaryButtonText = "Save", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot };
            var result = await confirmSave.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            long textLength = CustomDataEditor.Editor.TextLength;
            var fileSave = SelectedFile.SelectedIndex == 0 ? pitchDataFile : effectsDataFile;

            await File.WriteAllTextAsync(fileSave, CustomDataEditor.Editor.GetText(textLength));
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
            if (starting) return;
            switch (SelectedFile.SelectedIndex)
            {
                case 0:
                    CustomDataEditor.Editor.SetText(File.ReadAllText(pitchDataFile));
                    break;
                case 1:
                    CustomDataEditor.Editor.SetText(File.ReadAllText(effectsDataFile));
                    break;
            }
        }
    }
}
