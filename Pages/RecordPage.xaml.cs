using AudioReplacer.Generic;
using AudioReplacer.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AudioReplacer.Pages
{
    public sealed partial class RecordPage
    {
        private AudioRecordingUtils audioRecordingUtils;
        private FileManagement fileManagement;
        private RecordPageFunctionality recordPageBackend;
        private string previousPitchSelection = "None Selected";
        private string previousEffectSelection = "None";
        private bool projectNotSelected = true;
        private bool areUpdatesAvailable;

        public RecordPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PitchSettingsFeedback.Visibility = AppGeneric.ShowAudioEffectDetails ? Visibility.Visible : Visibility.Collapsed; // Hide UI of the verbose pitch value text on launch (if disabled)
            recordPageBackend = new RecordPageFunctionality([SuccessNotification, ProgressNotification, UpdateNotification]);
            VoiceTuneMenu.ItemsSource = recordPageBackend.GetPitchTitles();
            EffectsMenu.ItemsSource = recordPageBackend.GetEffectTitles();

            // Looping needs to be on to work around a bug in which the audio gets cut off for a split second after the first play.
            AudioPreview.MediaPlayer.IsLoopingEnabled = true;
            audioRecordingUtils = new AudioRecordingUtils();

            switch (recordPageBackend.IsFfMpegAvailable())
            {
                case true: // Check For Updates
                    if (!AppGeneric.UpdateChecksAllowed) break;
                    areUpdatesAvailable = recordPageBackend.IsUpdateAvailable();

                    // Debugger.IsAttached is checked here to prevent annoying popups when developing new versions
                    if (!areUpdatesAvailable ||  Debugger.IsAttached ) break;

                    UpdateNotification.Message = $"Latest Version: {recordPageBackend.GetWebVersion()}";
                    UpdateNotification.IsOpen = true;
                    break;
                case false: // Show popup for dependency install requirement
                    VoiceTuneMenu.IsEnabled = false;
                    EffectsMenu.IsEnabled = false;
                    DependencyNotification.IsOpen = true;
                    App.MainWindow.DisableFolderChanger();
                    break;
            }

            // Check if folder memory is enabled and if the remembered path exists
            if (recordPageBackend.FolderMemoryAllowed())
            {
                ProjectSetup(App.AppSettings.LastSelectedFolder, true);
                PauseMediaPlayer();
            }
            App.DiscordController.SetDetails("On Record Page");
            if(fileManagement != null)
                App.DiscordController.SetState($"{fileManagement.CalculatePercentageComplete()}% Complete");
        }

        private void UpdateRecordingValues()
        {
            PitchSettingsFeedback.Visibility = AppGeneric.ShowAudioEffectDetails ? Visibility.Visible : Visibility.Collapsed;
            if (audioRecordingUtils == null) return;
            if (VoiceTuneMenu.SelectedItem != null)
            {
                audioRecordingUtils.pitchChange = recordPageBackend.GetPitchModifier(VoiceTuneMenu.SelectedIndex);
                previousPitchSelection = VoiceTuneMenu.SelectedItem.ToString();
            }
            if (EffectsMenu.SelectedItem != null)
            {
                audioRecordingUtils.effectCommand = recordPageBackend.GetEffectValues(EffectsMenu.SelectedIndex);
                previousEffectSelection = EffectsMenu.SelectedItem.ToString();
            }
            if (App.AppSettings.ShowEffectSelection != 1) return;
            
            PitchSettingsFeedback.Text = 
                $"Pitch Modifier: {audioRecordingUtils.pitchChange} " +
                $"({previousPitchSelection})\nEffect Selected: {previousEffectSelection}\nExtra Edits Required? " +
                $"{AppGeneric.BoolToString((bool) FurtherEditsCheckBox.IsChecked!)}";
        }

        public async void SelectProjectFolder(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                FileTypeFilter = { "*" }
            };

            IntPtr hWnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(folderPicker, hWnd);
            var folder = await folderPicker.PickSingleFolderAsync();

            if (folder == null) return;
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            App.AppSettings.LastSelectedFolder = folder.Path;
            ProjectSetup(folder.Path);
        }

        private async void SkipCurrentAudioFile(object sender, RoutedEventArgs e)
        {
            if (fileManagement == null) return;
            AudioPreview.MediaPlayer.Pause();
            var confirmSkip = new ContentDialog { Title = "Skip this file?", Content = "Are you sure you want to skip this file?", PrimaryButtonText = "Skip", CloseButtonText = "Don't Skip", XamlRoot = Content.XamlRoot };
            var confirmResult = await confirmSkip.ShowAsync();

            switch (confirmResult == ContentDialogResult.Primary)
            {
                case true:
                    fileManagement.SkipAudioTrack();
                    UpdateFileElements();
                    recordPageBackend.UpdateInfoBar(SuccessNotification, "Success!", "File skipped!", InfoBarSeverity.Success);
                    break;
                case false:
                    AudioPreview.MediaPlayer.Play();
                    recordPageBackend.UpdateInfoBar(SuccessNotification, "Cancelled", "File skip cancelled", InfoBarSeverity.Informational);
                    break;
            }
        }

        private async void StartRecordingAudio(object sender, RoutedEventArgs e)
        {
            MainWindow.IsRecording = true;
            AudioPreview.MediaPlayer.Pause();
            recordPageBackend.UpdateInfoBar(ProgressNotification, "Recording In Progress...", "", 0, autoClose: false);

            if (fileManagement != null)
            {
                StorageFolder currentOutFolder = await fileManagement.GetDirectoryAsStorageFolder();
                await audioRecordingUtils.StartRecordingAudio(fileManagement.GetOutFolderStructure(), fileManagement.GetCurrentFileName());
            }
            App.DiscordController.SetSmallAsset("recording", "Recording Audio");
            SetButtonStates(true);
        }

        private async void StopRecordingAudio(object sender, RoutedEventArgs e)
        {
            MainWindow.IsRecording = false; MainWindow.IsProcessing = true;
            recordPageBackend.UpdateInfoBar(ProgressNotification, "Saving File....", "", 0);

            if (fileManagement == null) return;
            await audioRecordingUtils.StopRecordingAudio(fileManagement.GetOutFilePath());
            ToggleFinalReviewButtons(true);
            recordPageBackend.UpdateInfoBar(SuccessNotification, "Save Completed!", "Entering review phase...", InfoBarSeverity.Success);

            // Update source of audio player and the title manually
            CurrentFile.Text = "Review your recording...";
            App.DiscordController.SetSmallAsset("reviewing", "In review phase");
            AudioPreview.Source = recordPageBackend.MediaSourceFromUri(fileManagement.GetOutFilePath());
        }

        private void UpdateFileElements()
        {
            var progressPercentage = fileManagement.CalculatePercentageComplete();
            var projectPath = fileManagement.GetProjectPath();

            CurrentFile.Text = recordPageBackend.GetFormattedCurrentFile(fileManagement.GetCurrentFile());
            RemainingFiles.Text = $"Files Remaining: {fileManagement.GetFileCount(projectPath):N0} ({progressPercentage}%)";
            RemainingFilesProgress.Value = progressPercentage;
            AudioPreview.Source = recordPageBackend.MediaSourceFromUri(fileManagement.GetCurrentFile(false));
            MainWindow.CurrentFile = fileManagement.GetOutFilePath();

            App.DiscordController.SetState($"{progressPercentage}% Complete");
            App.DiscordController.SetSmallAsset("idle", "Idle");
            App.DiscordController.SetLargeAsset("appicon", $"Current File: {fileManagement.GetCurrentFileName()}");
        }

        private async void CancelCurrentRecording(object sender, RoutedEventArgs e)
        {
            MainWindow.IsRecording = false;
            await audioRecordingUtils.StopRecordingAudio(fileManagement.GetOutFilePath(), true);
            SetButtonStates(false);
            recordPageBackend.UpdateInfoBar(SuccessNotification, "Recording Cancelled", "", InfoBarSeverity.Informational);
        }

        private void UpdateAudioStatus(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            MainWindow.IsProcessing = false;
            bool isSubmitButton = button.Name == "SubmitRecordingButton";

            switch (isSubmitButton)
            {
                case true:
                    // Submission Accepted
                    fileManagement.DeleteCurrentFile(/* This method essentially acts as a way to confirm the submission*/);
                    recordPageBackend.UpdateInfoBar(SuccessNotification, "Submission Accepted!", "Moving to next file...", InfoBarSeverity.Success);
                    break;
                case false:
                    // Submission Rejected
                    File.Delete(fileManagement.GetOutFilePath());
                    recordPageBackend.UpdateInfoBar(SuccessNotification, "Submission Rejected", "Returning to record phase...", InfoBarSeverity.Informational);
                    break;
            }

            ToggleFinalReviewButtons(false);
            SetButtonStates(false);
            UpdateFileElements();
        }

        public void ProjectSetup(string path, bool autoload = false)
        {
            // Not adding awaits or async to this call here continues execution
            if (!autoload && !areUpdatesAvailable) Task.Run(() => recordPageBackend.UpdateInfoBar(ProgressNotification, "Setting up project...", "", InfoBarSeverity.Informational, autoClose: false));
            switch (projectNotSelected)
            {
                case true:
                    AudioPreviewControls.IsEnabled = true;
                    fileManagement = new FileManagement(path);
                    recordPageBackend.ToggleButton(SkipAudioButton, true);
                    recordPageBackend.ToggleButton(StartRecordingButton, true);
                    FileProgressPanel.Visibility = Visibility.Visible;
                    MainWindow.ProjectInitialized = true;
                    projectNotSelected = false;
                    break;
                case false:
                    fileManagement = null;
                    fileManagement = new FileManagement(path);
                    break;
            }
            UpdateFileElements();
            if (!autoload && areUpdatesAvailable) 
                recordPageBackend.UpdateInfoBar(SuccessNotification, "Success!", "Project loaded!", InfoBarSeverity.Success);

            // Delete any msix update packages after loading project
            Task.Run(async () => await AppGeneric.SpawnProcess("cmd", @$"/c del /s /q {fileManagement.GetRootFolderPath()}\audio-replacer\*.msix"));
        }

        private void SetButtonStates(bool recording)
        {
            Button[] buttonsRecording = [EndRecordingButton, CancelRecordingButton];
            Button[] buttonsNotRecording = [StartRecordingButton, SkipAudioButton];
            for (int i = 0; i < buttonsRecording.Length; i++)
            {
                recordPageBackend.ToggleButton(buttonsRecording[i], recording); // Any buttons that appear during recording get toggled by the recording bool
                recordPageBackend.ToggleButton(buttonsNotRecording[i], !recording); // Any buttons that appear before recording get toggled by the inverse of the recording bool
            }
        }

        private void ToggleFinalReviewButtons(bool toggled)
        {
            Button[] buttons = [EndRecordingButton, CancelRecordingButton, DiscardRecordingButton, SubmitRecordingButton];
            for (int i = 0; i < buttons.Length; i++)
            {
                recordPageBackend.ToggleButton(buttons[i], i > 1 && toggled);
            }
        }

        private async void DownloadDependencies(object sender, RoutedEventArgs e)
        {
            try
            {
                DependencyNotification.IsOpen = false;
                recordPageBackend.UpdateInfoBar(ProgressNotification, "Installing Dependencies", "App will restart after finishing. Please stay connected to the internet", InfoBarSeverity.Informational, autoClose: false);
                await Task.Run(recordPageBackend.DownloadDependencies); // Prevents window from freezing when installing dependencies

                // Restart app
                AppGeneric.RestartApp();
            }
            catch (AggregateException) // Failsafe for if the computer is offline
            {
                recordPageBackend.UpdateInfoBar(ErrorNotification, "Error", "System appears to be offline. Application cannot run without dependencies. Try again by relaunching the application at a later time", InfoBarSeverity.Error, autoClose: false);
            }
        }

        private void OpenReleases(object sender, RoutedEventArgs e)
        {
            AppGeneric.OpenUrl("https://github.com/lemons-studios/audio-replacer-2/releases/latest");
        }

        private void FlagFurtherEdits(object sender, RoutedEventArgs e)
        {
            audioRecordingUtils.requiresExtraEdits = !audioRecordingUtils.requiresExtraEdits;
            UpdateRecordingValues();
        }

        // Awesome boilerplate here
        private void EffectsValueUpdate(object sender, SelectionChangedEventArgs e)
        {
            UpdateRecordingValues();
        }

        private void PauseMediaPlayer()
        {
            AudioPreview.MediaPlayer.Pause();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Prevent audio from playing on other pages if the media player is left playing
            PauseMediaPlayer();
        }
    }
}
