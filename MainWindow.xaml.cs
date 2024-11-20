
using AudioReplacer2.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using WinUIEx;

namespace AudioReplacer2
{
    public sealed partial class MainWindow : WindowEx
    {
        private readonly AppWindow appWindow;
        private readonly AudioRecordingUtils audioRecordingUtils;
        private FileInteractionUtils fileInteractionUtils;
        private readonly MainWindowFunctionality windowBackend;
        private string previousPitchSelection = "None Selected";
        
        private bool isProcessing;
        private bool isRecording;

        public MainWindow() // This class has been somewhat minified for fun. Everything is still pretty readable though!!
        {
            InitializeComponent();
            windowBackend = new MainWindowFunctionality(voiceTuneMenu);
            this.Title = $"Audio Replacer {windowBackend.GetAppVersion()}";
            voiceTuneMenu.ItemsSource = windowBackend.GetPitchTitles();
            RequiresEffectsPrompt.ItemsSource = new List<string> {"Yes", "No"}; // Prevents clutter on the .xaml file (1 line added here is 3 lines removed from the xaml file)

            // Looping needs to be on to work around a bug in which the audio gets cut off for a split second after the first play.
            AudioPreview.MediaPlayer.IsLoopingEnabled = true; 
            audioRecordingUtils = new AudioRecordingUtils();

            appWindow = windowBackend.GetAppWindowForCurrentWindow(this);
            appWindow.Closing += OnWindowClose;
        }

        private void UpdateRecordingValues(object sender, SelectionChangedEventArgs e)
        {
            if (audioRecordingUtils == null) return;
            if (voiceTuneMenu.SelectedItem != null)
            {
                audioRecordingUtils.pitchChange = windowBackend.GetPitchModifier(voiceTuneMenu.SelectedIndex);
                previousPitchSelection = voiceTuneMenu.SelectedItem.ToString();
            }
            if (RequiresEffectsPrompt.SelectedItem != null) audioRecordingUtils.requiresExtraEdits = !windowBackend.ToBool(RequiresEffectsPrompt.SelectedIndex); // Inverse because "Yes" is the first option in the ComboBox

            PitchSettingsFeedback.Text = $"Pitch Modifier: {audioRecordingUtils.pitchChange} ({previousPitchSelection})\nDoes file require extra edits? {windowBackend.BoolToYesNo(audioRecordingUtils.requiresExtraEdits)}";
        }

        private async void SelectProjectFolder(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
            folderPicker.FileTypeFilter.Add("*");

            // For Win10 Compat
            var hWnd = WindowNative.GetWindowHandle(this);

            InitializeWithWindow.Initialize(folderPicker, hWnd);
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null) return;
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);

            FolderSelector.Visibility = Visibility.Collapsed;
            InitialFileSetup(folder.Path);
        }

        private async void SkipCurrentAudioFile(object sender, RoutedEventArgs e)
        {
            if (fileInteractionUtils == null) return;
            AudioPreview.MediaPlayer.Pause();
            var confirmSkip = new ContentDialog { Title = "Skip this file?", Content = "Are you sure you want to skip this file?", PrimaryButtonText = "Skip", CloseButtonText = "Don't Skip", XamlRoot = base.Content.XamlRoot };
            var confirmResult = await confirmSkip.ShowAsync();

            if (confirmResult == ContentDialogResult.Primary)
            {
                fileInteractionUtils.SkipAudioTrack();
                UpdateFileElements();
            }
            else AudioPreview.MediaPlayer.Play();
        }

        private async void StartRecordingAudio(object sender, RoutedEventArgs e)
        {
            isRecording = true;
            AudioPreview.MediaPlayer.Pause();

            if (fileInteractionUtils != null)
            {
                StorageFolder currentOutFolder = await fileInteractionUtils.GetDirectoryAsStorageFolder();
                await audioRecordingUtils.StartRecordingAudio(currentOutFolder, fileInteractionUtils.GetCurrentFileName());
            }
            ToggleButtonStates(true);
        }

        private async void StopRecordingAudio(object sender, RoutedEventArgs e)
        {
            isRecording = false; isProcessing = true;

            if (fileInteractionUtils == null) return;
            await audioRecordingUtils.StopRecordingAudio(fileInteractionUtils.GetOutFilePath());
            ToggleFinalReviewButtons(true);

            // Update source of audio player and the title manually
            CurrentFile.Text = "Review your recording...";
            AudioPreview.Source = windowBackend.MediaSourceFromURI(fileInteractionUtils.GetOutFilePath());
        }

        private void UpdateFileElements()
        {
            CurrentFile.Text = windowBackend.GetFormattedCurrentFile(fileInteractionUtils.GetCurrentFile());
            RemainingFiles.Text = $"Files Remaining: {fileInteractionUtils.GetFilesRemaining():N0}";
            AudioPreview.Source = windowBackend.MediaSourceFromURI(fileInteractionUtils.GetCurrentFile(false));
        }

        private async void CancelCurrentRecording(object sender, RoutedEventArgs e)
        {
            isProcessing = false;
            await audioRecordingUtils.CancelRecording(fileInteractionUtils.GetOutFilePath());
            ToggleButtonStates(false);
        }

        private void UpdateAudioStatus(object sender, RoutedEventArgs e)
        {
            if (sender is Button button) 
            {
                isProcessing = false;
                bool isSubmitButton = button.Name == "SubmitRecordingButton";
                if(isSubmitButton) fileInteractionUtils.DeleteCurrentFile(/* This method essentially acts as a way to confirm the submission*/); else File.Delete(fileInteractionUtils.GetOutFilePath());
                
                ToggleFinalReviewButtons(false);
                ToggleButtonStates(false);
                UpdateFileElements();
            }
        }

        private void InitialFileSetup(string path)
        {
            RemainingFiles.Visibility = Visibility.Visible;
            windowBackend.ToggleButton(SkipAudioButton, true);
            windowBackend.ToggleButton(StartRecordingButton, true);

            fileInteractionUtils = new FileInteractionUtils(path);
            UpdateFileElements();
        }

        private void ToggleButtonStates(bool recording)
        {
            Button[] buttonsRecording = [EndRecordingButton, CancelRecordingButton]; 
            Button[] buttonsNotRecording = [StartRecordingButton, SkipAudioButton];
            for (int i = 0; i < buttonsRecording.Length; i++)
            {
                windowBackend.ToggleButton(buttonsRecording[i], recording); // Any buttons that appear during recording get toggled by the recording bool
                windowBackend.ToggleButton(buttonsNotRecording[i], !recording); // Any buttons that appear before recording get toggled by the inverse of the recording bool
            }
        }

        private void ToggleFinalReviewButtons(bool toggled)
        {
            // No reason to toggle between the EndRecordingButton states, hard-code to disable
            windowBackend.ToggleButton(EndRecordingButton, false);
            windowBackend.ToggleButton(CancelRecordingButton, false);

            windowBackend.ToggleButton(DiscardRecordingButton, toggled);
            windowBackend.ToggleButton(SubmitRecordingButton, toggled);
        }

        private void OnWindowClose(object sender, AppWindowClosingEventArgs args) { if (fileInteractionUtils != null && (isProcessing || isRecording)) File.Delete(fileInteractionUtils.GetOutFilePath()); }
    }
}
