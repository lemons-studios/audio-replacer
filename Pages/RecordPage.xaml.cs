using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using AudioReplacer.Util;
using WinRT.Interop;

namespace AudioReplacer.Pages
{
    public sealed partial class RecordPage
    {
        private readonly AudioRecordingUtils audioRecordingUtils;
        private FileManagement fileManagement;
        private readonly RecordPageFunctionality recordPageBackend;
        private string previousPitchSelection = "None Selected";
        private string previousEffectSelection = "None";
        private bool projectNotSelected = true;
        private bool areUpdatesAvailable;

        public RecordPage()
        {
            InitializeComponent();
            PitchSettingsFeedback.Visibility = GlobalData.ShowAudioEffectDetails ? Visibility.Visible : Visibility.Collapsed; // Needed here to hide UI on app launch
            recordPageBackend = new RecordPageFunctionality([SuccessNotification, ProgressNotification, UpdateNotification]);
            VoiceTuneMenu.ItemsSource = recordPageBackend.GetPitchTitles();
            EffectsMenu.ItemsSource = recordPageBackend.GetEffectTitles();

            // Looping needs to be on to work around a bug in which the audio gets cut off for a split second after the first play.
            AudioPreview.MediaPlayer.IsLoopingEnabled = true;
            audioRecordingUtils = new AudioRecordingUtils();

            switch (recordPageBackend.IsFfMpegAvailable())
            {
                case true: // Check For Updates
                    if (!GlobalData.UpdateChecksAllowed) break;
                    areUpdatesAvailable = recordPageBackend.IsUpdateAvailable();
                    if (!areUpdatesAvailable) break;
                    UpdateNotification.Message = $"Latest Version: {recordPageBackend.GetWebVersion()}";
                    UpdateNotification.IsOpen = true;
                    break;
                case false: // Show popup for dependency install requirement
                    VoiceTuneMenu.IsEnabled = false;
                    EffectsMenu.IsEnabled = false;
                    DependencyNotification.IsOpen = true;
                    break;
            }

            // Check if folder memory is enabled and if the remembered path exists
            if (recordPageBackend.FolderMemoryAllowed()) { ProjectSetup(App.AppSettings.LastSelectedFolder, true); }
        }

        private void UpdateRecordingValues()
        {
            PitchSettingsFeedback.Visibility = GlobalData.ShowAudioEffectDetails ? Visibility.Visible : Visibility.Collapsed;
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
            if(PitchSettingsFeedback.Visibility == Visibility.Visible) PitchSettingsFeedback.Text = $"Pitch Modifier: {audioRecordingUtils.pitchChange} ({previousPitchSelection})\nEffect Selected: {previousEffectSelection}\nExtra Edits Required? {recordPageBackend.BoolToString(FurtherEditsCheckBox.IsChecked)}";
        }

        public async void SelectProjectFolder(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
            folderPicker.FileTypeFilter.Add("*");
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
                await audioRecordingUtils.StartRecordingAudio(currentOutFolder, fileManagement.GetCurrentFileName());
            }
            ToggleButtonStates(true);
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
            AudioPreview.Source = recordPageBackend.MediaSourceFromUri(fileManagement.GetOutFilePath());
        }

        private void UpdateFileElements()
        {
            float progressPercentage = fileManagement.CalculatePercentageComplete();
            string projectPath = fileManagement.GetProjectPath();

            CurrentFile.Text = recordPageBackend.GetFormattedCurrentFile(fileManagement.GetCurrentFile());
            RemainingFiles.Text = $"Files Remaining: {fileManagement.GetFileCount(projectPath):N0} ({progressPercentage}%)";
            RemainingFilesProgress.Value = progressPercentage;
            AudioPreview.Source = recordPageBackend.MediaSourceFromUri(fileManagement.GetCurrentFile(false));
            MainWindow.CurrentFile = fileManagement.GetOutFilePath();
        }

        private async void CancelCurrentRecording(object sender, RoutedEventArgs e)
        {
            MainWindow.IsRecording = false;
            await audioRecordingUtils.CancelRecording(fileManagement.GetOutFilePath());
            ToggleButtonStates(false);
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
                    recordPageBackend.UpdateInfoBar(SuccessNotification, "Submission Accepted!!", "Moving to next file...", InfoBarSeverity.Success);
                    break;
                case false:
                    // Submission Rejected
                    File.Delete(fileManagement.GetOutFilePath());
                    recordPageBackend.UpdateInfoBar(SuccessNotification, "Submission Rejected", "Returning to record phase...", InfoBarSeverity.Informational);
                    break;
            }

            ToggleFinalReviewButtons(false);
            ToggleButtonStates(false);
            if (GlobalData.EnableFanfare)
            {
                var fanfareEffectPath = new Uri("ms-appx:///Assets/Fanfare.mp3");
                var mediaSource = MediaSource.CreateFromUri(fanfareEffectPath);
                App.MainWindow.PlaySoundEffect(mediaSource);
            }
            UpdateFileElements();
        }

        public void ProjectSetup(string path, bool autoload = false)
        {
            if(!autoload && !areUpdatesAvailable) Task.Run(() => recordPageBackend.UpdateInfoBar(ProgressNotification, "Setting up project...", "", InfoBarSeverity.Informational, autoClose: false));
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
            if(!autoload && areUpdatesAvailable) recordPageBackend.UpdateInfoBar(SuccessNotification, "Success!", "Project loaded!", InfoBarSeverity.Success);

            // Delete any msix update packages after loading project
            Process projectFolderCleanup = ShellCommandManager.CreateProcess("cmd", @$"/c del /s /q {fileManagement.GetRootFolderPath()}\audio-replacer\*.msix");
            projectFolderCleanup.Start();
        }

        private void ToggleButtonStates(bool recording)
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
            for (int i = 0; i < buttons.Length; i++) { recordPageBackend.ToggleButton(buttons[i], i > 1 && toggled); } // Making my code slightly unreadable in exchange for fewer lines 🔥🔥🔥
        }

        private async void DownloadRuntimeDependencies(object sender, RoutedEventArgs e)
        {
            try
            {
                DependencyNotification.IsOpen = false;
                recordPageBackend.UpdateInfoBar(ProgressNotification, "Installing Dependencies", "App will restart after finishing. Please stay connected to the internet", InfoBarSeverity.Informational, autoClose: false);
                await Task.Run(recordPageBackend.DownloadDependencies); // Prevents window from freezing when installing dependencies

                // Restart app
                Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
            }
            catch (AggregateException) // Failsafe for if the computer is offline
            {
                recordPageBackend.UpdateInfoBar(ErrorNotification, "Error", "System appears to be offline. Application cannot run without dependencies. Try again by relaunching the application at a later time", InfoBarSeverity.Error, autoClose: false);
            }
        }

        private void OpenGithubReleases(object sender, RoutedEventArgs e)
        {
            Process openReleasesProcess = ShellCommandManager.CreateProcess("cmd", $"/c start https://github.com/lemons-studios/audio-replacer-2/releases/latest");
            openReleasesProcess.Start();
        }

        private void FlagFurtherEdits(object sender, RoutedEventArgs e) { audioRecordingUtils.requiresExtraEdits = true; }
        private void UnFlagFurtherEdits(object sender, RoutedEventArgs e) { audioRecordingUtils.requiresExtraEdits = false; }
        // Awesome boilerplate here
        private void ComboBoxRecordValuesUpdate(object sender, SelectionChangedEventArgs e) { UpdateRecordingValues(); }
        private void FurtherEditsValuesUpdate(object sender, RoutedEventArgs e) { UpdateRecordingValues(); }
    }
}
