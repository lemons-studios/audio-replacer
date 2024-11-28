using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using AudioReplacer.Util;
using WinRT.Interop;

namespace AudioReplacer.Pages
{
    public sealed partial class RecordPage
    {
        private readonly AudioRecordingUtils audioRecordingUtils;
        private ProjectFileManagementUtils projectFileManagementUtils;
        private readonly RecordPageFunctionality recordPageBackend;
        private string previousPitchSelection = "None Selected";
        private string previousEffectSelection = "None";
        private bool projectNotSelected = true;

        public RecordPage()
        {
            InitializeComponent();
            recordPageBackend = new RecordPageFunctionality([ToastNotification, ProgressToast, UpdateToast]);
            VoiceTuneMenu.ItemsSource = recordPageBackend.GetPitchTitles();
            EffectsMenu.ItemsSource = recordPageBackend.GetEffectTitles();

            // Looping needs to be on to work around a bug in which the audio gets cut off for a split second after the first play.
            AudioPreview.MediaPlayer.IsLoopingEnabled = true;
            audioRecordingUtils = new AudioRecordingUtils();

            switch (recordPageBackend.IsFfMpegAvailable())
            {
                case true: // Check For Updates
                    if (!GlobalData.UpdateChecksAllowed) break;
                    bool updatesAvailable = recordPageBackend.IsUpdateAvailable();
                    if (!updatesAvailable) break;
                    UpdateToast.Message = $"Latest Version: {recordPageBackend.GetWebVersion()}";
                    UpdateToast.IsOpen = true;
                    break;
                case false: // Show popup for dependency install requirement
                    recordPageBackend.ToggleButton(App.MainWindow.GetProjectButton(), false);
                    VoiceTuneMenu.IsEnabled = false;
                    EffectsMenu.IsEnabled = false;
                    DependencyToast.IsOpen = true;
                    break;
            }

            // Check if folder memory is enabled and if the remembered path exists
            if (recordPageBackend.FolderMemoryAllowed()) { ProjectSetup(App.AppSettings.LastSelectedFolder); }
        }

        private void UpdateRecordingValues()
        {
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
            PitchSettingsFeedback.Text = $"Pitch Modifier: {audioRecordingUtils.pitchChange} ({previousPitchSelection})\nEffect Selected: {previousEffectSelection}\nExtra Edits Required? {recordPageBackend.BoolToString(FurtherEditsCheckBox.IsChecked)}";
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
            if (projectFileManagementUtils == null) return;
            AudioPreview.MediaPlayer.Pause();
            var confirmSkip = new ContentDialog { Title = "Skip this file?", Content = "Are you sure you want to skip this file?", PrimaryButtonText = "Skip", CloseButtonText = "Don't Skip", XamlRoot = Content.XamlRoot };
            var confirmResult = await confirmSkip.ShowAsync();

            switch (confirmResult == ContentDialogResult.Primary)
            {
                case true:
                    projectFileManagementUtils.SkipAudioTrack();
                    UpdateFileElements();
                    recordPageBackend.UpdateInfoBar(ToastNotification, "Success!", "File skipped!", InfoBarSeverity.Success);
                    break;
                case false:
                    AudioPreview.MediaPlayer.Play();
                    recordPageBackend.UpdateInfoBar(ToastNotification, "Cancelled", "File skip cancelled", InfoBarSeverity.Informational);
                    break;
            }
        }

        private async void StartRecordingAudio(object sender, RoutedEventArgs e)
        {
            MainWindow.IsRecording = true;
            AudioPreview.MediaPlayer.Pause();
            recordPageBackend.UpdateInfoBar(ProgressToast, "Recording In Progress...", "", 0, autoClose: false);

            if (projectFileManagementUtils != null)
            {
                StorageFolder currentOutFolder = await projectFileManagementUtils.GetDirectoryAsStorageFolder();
                await audioRecordingUtils.StartRecordingAudio(currentOutFolder, projectFileManagementUtils.GetCurrentFileName());
            }
            ToggleButtonStates(true);
        }

        private async void StopRecordingAudio(object sender, RoutedEventArgs e)
        {
            MainWindow.IsRecording = false; MainWindow.IsProcessing = true;
            recordPageBackend.UpdateInfoBar(ProgressToast, "Saving File....", "", 0);

            if (projectFileManagementUtils == null) return;
            await audioRecordingUtils.StopRecordingAudio(projectFileManagementUtils.GetOutFilePath());
            ToggleFinalReviewButtons(true);
            recordPageBackend.UpdateInfoBar(ToastNotification, "Save Completed!", "Entering review phase...", InfoBarSeverity.Success);

            // Update source of audio player and the title manually
            CurrentFile.Text = "Review your recording...";
            AudioPreview.Source = recordPageBackend.MediaSourceFromUri(projectFileManagementUtils.GetOutFilePath());
        }

        private void UpdateFileElements()
        {
            float progressPercentage = projectFileManagementUtils.CalculatePercentageComplete();
            string projectPath = projectFileManagementUtils.GetProjectPath();

            CurrentFile.Text = recordPageBackend.GetFormattedCurrentFile(projectFileManagementUtils.GetCurrentFile());
            RemainingFiles.Text = $"Files Remaining: {projectFileManagementUtils.GetFileCount(projectPath):N0} ({progressPercentage}%)";
            RemainingFilesProgress.Value = progressPercentage;
            AudioPreview.Source = recordPageBackend.MediaSourceFromUri(projectFileManagementUtils.GetCurrentFile(false));
            MainWindow.CurrentFile = projectFileManagementUtils.GetOutFilePath();
        }

        private async void CancelCurrentRecording(object sender, RoutedEventArgs e)
        {
            MainWindow.IsRecording = false;
            await audioRecordingUtils.CancelRecording(projectFileManagementUtils.GetOutFilePath());
            ToggleButtonStates(false);
            recordPageBackend.UpdateInfoBar(ToastNotification, "Recording Cancelled", "", InfoBarSeverity.Informational);
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
                    projectFileManagementUtils.DeleteCurrentFile(/* This method essentially acts as a way to confirm the submission*/);
                    recordPageBackend.UpdateInfoBar(ToastNotification, "Submission Accepted!!", "Moving to next file...", InfoBarSeverity.Success);
                    break;
                case false:
                    // Submission Rejected
                    File.Delete(projectFileManagementUtils.GetOutFilePath());
                    recordPageBackend.UpdateInfoBar(ToastNotification, "Submission Rejected", "Returning to record phase...", InfoBarSeverity.Informational);
                    break;
            }

            ToggleFinalReviewButtons(false);
            ToggleButtonStates(false);
            UpdateFileElements();
        }

        private void ProjectSetup(string path)
        {
            Task.Run(() => recordPageBackend.UpdateInfoBar(ProgressToast, "Setting up project...", "", InfoBarSeverity.Informational, autoClose: false));
            switch (projectNotSelected)
            {
                case true:
                    AudioPreviewControls.IsEnabled = true;
                    projectFileManagementUtils = new ProjectFileManagementUtils(path);
                    recordPageBackend.ToggleButton(SkipAudioButton, true);
                    recordPageBackend.ToggleButton(StartRecordingButton, true);
                    FileProgressPanel.Visibility = Visibility.Visible;
                    MainWindow.ProjectInitialized = true;
                    projectNotSelected = false;
                    break;
                case false:
                    projectFileManagementUtils = null;
                    projectFileManagementUtils = new ProjectFileManagementUtils(path);
                    break;
            }
            UpdateFileElements();
            recordPageBackend.UpdateInfoBar(ToastNotification, "Success!", "Project loaded!", InfoBarSeverity.Success);
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
            DependencyToast.IsOpen = false;
            recordPageBackend.UpdateInfoBar(ProgressToast, "Installing Dependencies", "App will restart after finishing. Please stay connected to the internet", InfoBarSeverity.Informational, autoClose: false);
            await Task.Run(recordPageBackend.DownloadDependencies); // Prevents window from freezing when installing dependencies

            // Restart app
            Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
        }

        private void OpenGithubReleases(object sender, RoutedEventArgs e)
        {
            const string url = "https://github.com/lemons-studios/audio-replacer-2/releases/latest";
            Process openReleasesProcess = ShellCommandManager.CreateProcess("cmd", $"/c start {url}");
            openReleasesProcess.Start();
        }

        private void FlagFurtherEdits(object sender, RoutedEventArgs e) { audioRecordingUtils.requiresExtraEdits = true; }
        private void UnFlagFurtherEdits(object sender, RoutedEventArgs e) { audioRecordingUtils.requiresExtraEdits = false; }

        // Awesome boilerplate here
        private void ComboBoxRecordValuesUpdate(object sender, SelectionChangedEventArgs e) { UpdateRecordingValues(); }
        private void FurtherEditsValuesUpdate(object sender, RoutedEventArgs e) { UpdateRecordingValues(); }
    }
}
