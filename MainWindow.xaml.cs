using AudioReplacer2.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;
using WinUIEx;

namespace AudioReplacer2
{
    public sealed partial class MainWindow : WindowEx
    {
        private readonly AppWindow appWindow;
        private readonly AudioRecordingUtils audioRecordingUtils;
        private ProjectFileManagementUtils projectFileManagementUtils;
        private readonly MainWindowFunctionality windowBackend;
        private string previousPitchSelection = "None Selected";
        
        // Needed for button state switching
        private bool isProcessing;
        private bool isRecording;

        public MainWindow()
        {
            InitializeComponent();
            windowBackend = new MainWindowFunctionality(VoiceTuneMenu, [ToastNotification, SavingToast, UpdateToast]);
            
            VoiceTuneMenu.ItemsSource = windowBackend.GetPitchTitles();
            RequiresEffectsPrompt.ItemsSource = new List<string> {"Yes", "No"}; // Prevents clutter on the .xaml file (1 line added here is 3 lines removed from the xaml file)

            // Looping needs to be on to work around a bug in which the audio gets cut off for a split second after the first play.
            AudioPreview.MediaPlayer.IsLoopingEnabled = true; 
            audioRecordingUtils = new AudioRecordingUtils();

            appWindow = windowBackend.GetAppWindowForCurrentWindow(this);
            appWindow.Closing += OnWindowClose;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            AppTitle.Text = $"Audio Replacer {windowBackend.GetAppVersion()}";

            if (!windowBackend.FFMpegAvailable())
            {
                // Do not check for updates if FFMpeg is not installed. Also disable any interactable elements on the page
                windowBackend.ToggleButton(FolderSelector, false);
                VoiceTuneMenu.IsEnabled = false;
                RequiresEffectsPrompt.IsEnabled = false;
                DependencyToast.IsOpen = true;
            }
            else
            {
                var updatesAvailable = windowBackend.IsUpdateAvailable();
                if (updatesAvailable)
                {
                    UpdateToast.Message = $"Latest Version: {windowBackend.GetWebVersion()}";
                    UpdateToast.IsOpen = true;
                }
            }

            // Enable mica if it's supported. If not, fall back to Acrylic
            switch (MicaController.IsSupported())
            {
                case true:
                    var micaBackdrop = new MicaBackdrop();
                    micaBackdrop.Kind = MicaKind.Base;
                    SystemBackdrop = micaBackdrop;
                    break;
                case false:
                    var acrylicBackdrop = new DesktopAcrylicBackdrop();
                    SystemBackdrop = acrylicBackdrop;
                    break;
            }
        }

        private void UpdateRecordingValues(object sender, SelectionChangedEventArgs e)
        {
            if (audioRecordingUtils == null) return;
            if (VoiceTuneMenu.SelectedItem != null)
            {
                audioRecordingUtils.pitchChange = windowBackend.GetPitchModifier(VoiceTuneMenu.SelectedIndex);
                previousPitchSelection = VoiceTuneMenu.SelectedItem.ToString();
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

            ProjectSetup(folder.Path);
        }

        private async void SkipCurrentAudioFile(object sender, RoutedEventArgs e)
        {
            if (projectFileManagementUtils == null) return;
            AudioPreview.MediaPlayer.Pause();
            var confirmSkip = new ContentDialog { Title = "Skip this file?", Content = "Are you sure you want to skip this file?", PrimaryButtonText = "Skip", CloseButtonText = "Don't Skip", XamlRoot = base.Content.XamlRoot };
            var confirmResult = await confirmSkip.ShowAsync();

            if (confirmResult == ContentDialogResult.Primary)
            {
                projectFileManagementUtils.SkipAudioTrack();
                UpdateFileElements();
                windowBackend.UpdateInfoBar(ToastNotification, "Success!", "File skipped!", InfoBarSeverity.Success);
            }
            else if (confirmResult == ContentDialogResult.Secondary)
            {
                AudioPreview.MediaPlayer.Play();
                windowBackend.UpdateInfoBar(ToastNotification, "Cancelled", "File skip cancelled" , InfoBarSeverity.Informational);
            }
        }

        private async void StartRecordingAudio(object sender, RoutedEventArgs e)
        {
            isRecording = true;
            AudioPreview.MediaPlayer.Pause();
            windowBackend.UpdateInfoBar(SavingToast, "Recording In Progress...", "", 0, autoClose: false);

            if (projectFileManagementUtils != null)
            {
                StorageFolder currentOutFolder = await projectFileManagementUtils.GetDirectoryAsStorageFolder();
                await audioRecordingUtils.StartRecordingAudio(currentOutFolder, projectFileManagementUtils.GetCurrentFileName());
            }
            ToggleButtonStates(true);
        }

        private async void StopRecordingAudio(object sender, RoutedEventArgs e)
        {
            isRecording = false; isProcessing = true;
            windowBackend.UpdateInfoBar(SavingToast, "Saving File....", "", 0);

            if (projectFileManagementUtils == null) return;
            await audioRecordingUtils.StopRecordingAudio(projectFileManagementUtils.GetOutFilePath());
            ToggleFinalReviewButtons(true);
            windowBackend.UpdateInfoBar(ToastNotification, "Save Completed!", "Entering review phase...", InfoBarSeverity.Success);

            // Update source of audio player and the title manually
            CurrentFile.Text = "Review your recording...";
            AudioPreview.Source = windowBackend.MediaSourceFromUri(projectFileManagementUtils.GetOutFilePath());
        }

        private void UpdateFileElements()
        {
            CurrentFile.Text = windowBackend.GetFormattedCurrentFile(projectFileManagementUtils.GetCurrentFile());
            RemainingFiles.Text = $"Files Remaining: {projectFileManagementUtils.GetFilesRemaining():N0}";
            AudioPreview.Source = windowBackend.MediaSourceFromUri(projectFileManagementUtils.GetCurrentFile(false));
        }

        private async void CancelCurrentRecording(object sender, RoutedEventArgs e)
        {
            isProcessing = false;
            await audioRecordingUtils.CancelRecording(projectFileManagementUtils.GetOutFilePath());
            ToggleButtonStates(false);
            windowBackend.UpdateInfoBar(ToastNotification, "Recording Cancelled", "", InfoBarSeverity.Informational);
        }

        private void UpdateAudioStatus(object sender, RoutedEventArgs e)
        {
            if (sender is Button button) 
            {
                isProcessing = false;
                bool isSubmitButton = button.Name == "SubmitRecordingButton";
                switch (isSubmitButton)
                {
                    case true:
                        // Submission Accepted
                        projectFileManagementUtils.DeleteCurrentFile(/* This method essentially acts as a way to confirm the submission*/);
                        windowBackend.UpdateInfoBar(ToastNotification, "Submission Accepted!!", "Moving to next file...", InfoBarSeverity.Success);
                        break;
                    case false:
                        // Submission Rejected
                        File.Delete(projectFileManagementUtils.GetOutFilePath());
                        windowBackend.UpdateInfoBar(ToastNotification, "Submission Rejected", "Returning to record phase...", InfoBarSeverity.Informational);
                        break;
                }
                
                ToggleFinalReviewButtons(false);
                ToggleButtonStates(false);
                UpdateFileElements();
            }
        }

        private void ProjectSetup(string path)
        {
            windowBackend.UpdateInfoBar(SavingToast, "Setting up project...", "", InfoBarSeverity.Informational, autoClose: false);
            RemainingFiles.Visibility = Visibility.Visible;
            windowBackend.ToggleButton(SkipAudioButton, true);
            windowBackend.ToggleButton(StartRecordingButton, true);

            FolderSelector.Visibility = Visibility.Collapsed;
            AudioPreviewControls.IsEnabled = true;

            projectFileManagementUtils = new ProjectFileManagementUtils(path);
            UpdateFileElements();
            windowBackend.UpdateInfoBar(ToastNotification, "Success!", "Project loaded!", InfoBarSeverity.Success);
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

        private void OpenGithubReleases(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/lemons-studios/audio-replacer-2/releases/latest";
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        private void OnWindowClose(object sender, AppWindowClosingEventArgs args) { if (projectFileManagementUtils != null && (isProcessing || isRecording)) File.Delete(projectFileManagementUtils.GetOutFilePath()); }

        private void DownloadRuntimeDependencies(object sender, RoutedEventArgs e)
        {
            DependencyToast.IsOpen = false;
            windowBackend.UpdateInfoBar(SavingToast, "Installing Dependencies", "App will restart after finishing. Please stay connected to the internet", InfoBarSeverity.Informational, autoClose: false);
            windowBackend.DownloadDependencies();

            // Restart app
            Windows.ApplicationModel.Core.AppRestartFailureReason failureReason = Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
        }
    }
}
