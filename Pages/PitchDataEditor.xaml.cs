using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AudioReplacer.Pages
{
    public sealed partial class PitchDataEditor
    {
        private readonly string pitchDataFile = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\AudioReplacer2-Config\PitchData.json";

        public PitchDataEditor()
        {
            InitializeComponent();
            PitchEditor.Editor.SetText(File.ReadAllText(pitchDataFile));
        }

        private async void SaveFile(object sender, RoutedEventArgs e)
        {
            var confirmSave = new ContentDialog { Title = "Save Pitch Data?", Content = "App will restart", PrimaryButtonText = "Save", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot };
            var result = await confirmSave.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            long textLength = PitchEditor.Editor.TextLength;
            await File.WriteAllTextAsync(pitchDataFile, PitchEditor.Editor.GetText(textLength));
            Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
        }

        private async void DiscardChanges(object sender, RoutedEventArgs e)
        {
            var confirmDiscard = new ContentDialog { Title = "Discard Changes?", Content = "File will reset to last save", PrimaryButtonText = "Discard", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot };
            var result = await confirmDiscard.ShowAsync();
            if(result == ContentDialogResult.Primary) PitchEditor.Editor.SetText(await File.ReadAllTextAsync(pitchDataFile));
        }
    }
}
