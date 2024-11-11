using AudioReplacer2.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage.Pickers;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Audio_Replacer_2.Util;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Newtonsoft.Json.Linq;
using Microsoft.UI;


namespace Audio_Replacer_2
{
    // TODO: Split class into two different files to improve on readability
    public sealed partial class MainWindow : Window
    {
        private AppWindow appWindow;

        private AudioRecordingUtils audioRecordingUtils;
        private FileInteractionUtils fileInteractionUtils;

        private string previousPitchSelection = "No Pitch Change";
        private readonly string pitchData = PitchData.pitchJsonData; // Remove later
        
        private bool isProcessing;
        private bool isRecording;

        public MainWindow()
        {
            InitializeComponent();
            AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(500, 500, 950, 450));
            AudioPreview.MediaPlayer.IsLoopingEnabled = true;
            AudioPreview.MediaPlayer.MediaEnded += OnMediaEnded;
            audioRecordingUtils = new AudioRecordingUtils();
            AppWindow.SetIcon("Assets/Titlebar.ico");

            appWindow = GetAppWindowForCurrentWindow();
            appWindow.Closing += OnWindowClose;
        }

        private void OnWindowClose(object sender, AppWindowClosingEventArgs args)
        {
            if (fileInteractionUtils != null && (isProcessing || isRecording))
            {
                audioRecordingUtils.DeleteRecording(fileInteractionUtils.GetOutFilePath());
            }
        }

        private async void SelectProjectFolder(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");

            // For Windows 10 Combatibility
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(folderPicker, hWnd);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                FolderSelector.Visibility = Visibility.Collapsed;
                InitialFileSetup(folder.Path);
            }
        }

        private async void SkipCurrentAudioFile(object sender, RoutedEventArgs e)
        {
            if (fileInteractionUtils == null) return;
            AudioPreview.MediaPlayer.Pause();

            var confirmSkip = new ContentDialog
            {
                Title = "Skip this file?",
                Content = "Are you sure you want to skip this file? You cannot reverse this action",
                PrimaryButtonText = "Skip",
                CloseButtonText = "Do not skip",
                XamlRoot = Content.XamlRoot
            };

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

            if (fileInteractionUtils == null) return;
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
            isRecording = false;
            isProcessing = true;

            if (fileInteractionUtils == null) return;
            await audioRecordingUtils.StopRecordingAudio(fileInteractionUtils.GetOutFilePath());
            ToggleFinalReviewButtons(true);

            // Update source of audio player and the title manually
            CurrentFile.Text = "Review your recording";
            AudioPreview.Source = mediaSourceFromURI(fileInteractionUtils.GetOutFilePath());
        }

        private void ConfirmAudioProfile(object sender, RoutedEventArgs e)
        {
            if (voiceTuneMenu.SelectedItem != null)
            {
                audioRecordingUtils.pitchChange = GetPitchModifier(voiceTuneMenu.SelectedIndex);
                previousPitchSelection = voiceTuneMenu.SelectedItem.ToString();
            }
            if (RequiresEffectsPrompt.SelectedItem != null)
            {
                audioRecordingUtils.requiresExtraEdits = ToBool(RequiresEffectsPrompt.SelectedIndex);
            }

            PitchSettingsFeedback.Text = $"Pitch Modifier: {audioRecordingUtils.pitchChange} ({previousPitchSelection})\nDoes file require extra edits? {BoolToYesNo(audioRecordingUtils.requiresExtraEdits)}";
        }

        private void UpdateFileElements()
        {
            CurrentFile.Text = fileInteractionUtils.GetCurrentFile();
            RemainingFiles.Text = $"Files Remaining: {fileInteractionUtils.GetFilesRemaining().ToString("N0")}";
            AudioPreview.Source = mediaSourceFromURI(fileInteractionUtils.GetCurrentFile(false));
        }

        private async void CancelCurrentRecording(object sender, RoutedEventArgs e)
        {
            isProcessing = false;
            await audioRecordingUtils.CancelRecording(fileInteractionUtils.GetOutFilePath());
            ToggleButtonStates(false);
        }

        private void ConfirmAudioSubmission(object sender, RoutedEventArgs e)
        {
            isProcessing = false;
            fileInteractionUtils.DeleteCurrentFile();
            ToggleFinalReviewButtons(false);
            ToggleButtonStates(false);
            UpdateFileElements();
        }

        private void DiscardRecording(object sender, RoutedEventArgs e)
        {
            isProcessing = false;
            audioRecordingUtils.DeleteRecording(fileInteractionUtils.GetOutFilePath());
            ToggleFinalReviewButtons(false);
            ToggleButtonStates(false);
            UpdateFileElements();
        }

        private void InitialFileSetup(string path)
        {
            RemainingFiles.Visibility = Visibility.Visible;
            fileInteractionUtils = new FileInteractionUtils(path);

            UpdateFileElements();
        }

        private void ToggleButtonStates(bool recording)
        {
            Button[] buttonsRecording = [EndRecordingButton, CancelRecordingButton];
            Button[] buttonsNotRecording = [StartRecordingButton, SkipAudioButton];

            // Toggle Visibility for non recording panel
            for (int i = 0; i < buttonsRecording.Length; i++)
            {
                ToggleButton(buttonsRecording[i], recording);
                ToggleButton(buttonsNotRecording[i], !recording);
            }
        }

        private void ToggleFinalReviewButtons(bool toggled)
        {
            // No reason to toggle between the EndRecordingButton states, hard-code to disable
            ToggleButton(EndRecordingButton, false);
            ToggleButton(CancelRecordingButton, false);

            ToggleButton(DiscardRecordingButton, toggled);
            ToggleButton(SubmitRecordingButton, toggled);
        }

        private void ToggleButton(Button button, bool toggle)
        {
            Visibility toggleVisibility = ToVisibility(toggle);

            button.IsEnabled = toggle;
            button.Visibility = toggleVisibility;
        }



        private float GetPitchModifier(int index)
        { 
           // TODO: JSON array is more evil than the evil switch case, replace with 2d array containing information on both character and pitch modification for (somewhat) easy edit-ability by any end user
           JArray jPitchData = JArray.Parse(pitchData);
           try
           {
               return (float) jPitchData[index]["pitchModification"];
           }
           catch (Exception e)
           {
               Console.WriteLine(e);
               return 1;
           }
        }

        private bool ToBool(int value)
        {
            return value > 0;
        }

        private string BoolToYesNo(bool value)
        {
            return value ? "Yes" : "No";
        }

        private Visibility ToVisibility(bool x)
        {
            return x ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnMediaEnded(MediaPlayer sender, object args)
        {
            sender.PlaybackSession.Position = TimeSpan.Zero;
        }

        private MediaSource mediaSourceFromURI(string path)
        {
            return MediaSource.CreateFromUri(new Uri(path));
        }

        // Thanks StackOverflow man!
        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }
    }
}
