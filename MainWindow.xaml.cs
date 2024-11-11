using AudioReplacer2.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage.Pickers;
using Windows.Storage;
using Audio_Replacer_2.Util;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace Audio_Replacer_2
{
    public sealed partial class MainWindow : Window
    {
        private readonly AppWindow appWindow;
        private readonly AudioRecordingUtils audioRecordingUtils;
        private FileInteractionUtils fileInteractionUtils;
        private readonly MainWindowFunctionality windowFunc;
        private string previousPitchSelection = "No Pitch Change";
        
        private bool isProcessing;
        private bool isRecording;

        public MainWindow() // This class has been somewhat minified for fun. Everything is still pretty readable though!!
        {
            InitializeComponent();
            windowFunc = new MainWindowFunctionality();
            AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(500, 500, 950, 450));

            AudioPreview.MediaPlayer.IsLoopingEnabled = true;
            audioRecordingUtils = new AudioRecordingUtils();

            AppWindow.SetIcon("Assets/Titlebar.ico");
            appWindow = windowFunc.GetAppWindowForCurrentWindow(this);
            appWindow.Closing += OnWindowClose;
        }

        private void OnWindowClose(object sender, AppWindowClosingEventArgs args) { if (fileInteractionUtils != null && (isProcessing || isRecording)) audioRecordingUtils.DiscardRecording(fileInteractionUtils.GetOutFilePath()); }

        private async void SelectProjectFolder(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
            folderPicker.FileTypeFilter.Add("*");

            // For Win10 Compat
            var hWnd = WindowNative.GetWindowHandle(this);

            InitializeWithWindow.Initialize(folderPicker, hWnd);
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null) return;
            
            FolderSelector.Visibility = Visibility.Collapsed;
            InitialFileSetup(folder.Path);
        }

        private async void SkipCurrentAudioFile(object sender, RoutedEventArgs e)
        {
            if (fileInteractionUtils == null) return;
            AudioPreview.MediaPlayer.Pause();
            var confirmSkip = new ContentDialog { Title = "Skip this file?", Content = "Are you sure you want to skip this file? You cannot reverse this action", PrimaryButtonText = "Skip", CloseButtonText = "Do not skip", XamlRoot = Content.XamlRoot };
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
            AudioPreview.Source = windowFunc.MediaSourceFromURI(fileInteractionUtils.GetOutFilePath());
        }

        private void ConfirmAudioProfile(object sender, RoutedEventArgs e)
        {
            if (voiceTuneMenu.SelectedItem != null)
            {
                audioRecordingUtils.pitchChange = windowFunc.GetPitchModifier(voiceTuneMenu.SelectedIndex, PitchData.pitchJsonData);
                previousPitchSelection = voiceTuneMenu.SelectedItem.ToString();
            }
            if (RequiresEffectsPrompt.SelectedItem != null) audioRecordingUtils.requiresExtraEdits = windowFunc.ToBool(RequiresEffectsPrompt.SelectedIndex);
            
            PitchSettingsFeedback.Text = $"Pitch Modifier: {audioRecordingUtils.pitchChange} ({previousPitchSelection})\nDoes file require extra edits? {windowFunc.BoolToYesNo(audioRecordingUtils.requiresExtraEdits)}";
        }

        private void UpdateFileElements()
        {
            CurrentFile.Text = fileInteractionUtils.GetCurrentFile();
            RemainingFiles.Text = $"Files Remaining: {fileInteractionUtils.GetFilesRemaining().ToString("N0")}";
            AudioPreview.Source = windowFunc.MediaSourceFromURI(fileInteractionUtils.GetCurrentFile(false));
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
                if(isSubmitButton) fileInteractionUtils.DeleteCurrentFile(/* This method essentially acts as a way to confirm the submission*/); else audioRecordingUtils.DiscardRecording(fileInteractionUtils.GetOutFilePath());
                
                ToggleFinalReviewButtons(false);
                ToggleButtonStates(false);
                UpdateFileElements();
            }
        }

        private void InitialFileSetup(string path)
        {
            RemainingFiles.Visibility = Visibility.Visible;
            windowFunc.ToggleButton(SkipAudioButton, true);
            windowFunc.ToggleButton(StartRecordingButton, true);

            fileInteractionUtils = new FileInteractionUtils(path);
            UpdateFileElements();
        }

        private void ToggleButtonStates(bool recording)
        {
            Button[] buttonsRecording = [EndRecordingButton, CancelRecordingButton]; 
            Button[] buttonsNotRecording = [StartRecordingButton, SkipAudioButton];
            for (int i = 0; i < buttonsRecording.Length; i++)
            {
                windowFunc.ToggleButton(buttonsRecording[i], recording); // Any buttons that appear during recording get toggled by the recording bool
                windowFunc.ToggleButton(buttonsNotRecording[i], !recording); // Any buttons that appear before recording get toggled by the inverse of the recording bool
            }
        }

        private void ToggleFinalReviewButtons(bool toggled)
        {
            // No reason to toggle between the EndRecordingButton states, hard-code to disable
            windowFunc.ToggleButton(EndRecordingButton, false);
            windowFunc.ToggleButton(CancelRecordingButton, false);

            windowFunc.ToggleButton(DiscardRecordingButton, toggled);
            windowFunc.ToggleButton(SubmitRecordingButton, toggled);
        }
    }
}
