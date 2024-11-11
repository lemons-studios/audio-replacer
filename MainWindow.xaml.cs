using AudioReplacer2.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage.Pickers;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Audio_Replacer_2.Util;
using WinRT.Interop;
using System.IO;
using Newtonsoft.Json.Linq;


namespace Audio_Replacer_2
{
    public sealed partial class MainWindow : Window
    {
        private FileInteractionUtils fileInteractionUtils;
        private AudioRecordingUtils audioRecordingUtils;
        private string previousSuccessfulCharacterConfirm = "No Pitch Change";
        private readonly string pitchData = PitchData.pitchJsonData;

        public MainWindow()
        {
            InitializeComponent();
            AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(500, 500, 950, 450));
            audioPreview.MediaPlayer.IsLoopingEnabled = true;
            audioPreview.MediaPlayer.MediaEnded += OnMediaEnded;
            audioRecordingUtils = new AudioRecordingUtils();
            // pitchJSONData = File.ReadAllText(pathToPitchJSON);

            AppWindow.SetIcon("Assets/Titlebar.ico");
        }

        private void OnMediaEnded(MediaPlayer sender, object args)
        {
            sender.PlaybackSession.Position = TimeSpan.Zero;
        }

        private async void SelectProjectFolder(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            folderPicker.FileTypeFilter.Add("*");

            // Initialize with window handle (required for desktop apps in WinUI 3, especially on Windows 10)
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                folderSelectButton.Visibility = Visibility.Collapsed;
                InitialFileSetup(folder.Path);
            }
        }

        private async void SkipCurrentAudioFile(object sender, RoutedEventArgs e)
        {
            if (fileInteractionUtils == null) return;
            audioPreview.MediaPlayer.Pause();

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
            else audioPreview.MediaPlayer.Play();
            
        }

        private async void StartRecordingAudio(object sender, RoutedEventArgs e)
        {
            if (fileInteractionUtils == null) return;
            audioPreview.MediaPlayer.Pause();

            submitAudioButton.Visibility = Visibility.Visible;
            recordAudioButton.IsEnabled = false;
            recordAudioButton.Visibility = Visibility.Collapsed;
            if (fileInteractionUtils != null)
            {
                StorageFolder currentOutFolder = await fileInteractionUtils.GetDirNameAsStorageFolder();
                await audioRecordingUtils.StartRecordingAudio(currentOutFolder, fileInteractionUtils.GetCurrentFileName());
            }

            // Prevent audio from being submitted until after everything gets initialized
            submitAudioButton.IsEnabled = true;
            cancelRecordingButton.Visibility = Visibility.Visible;
            cancelRecordingButton.IsEnabled = true;
        }

        private async void SubmitRecordedAudio(object sender, RoutedEventArgs e)
        {
            if (fileInteractionUtils == null) return;

            recordAudioButton.Visibility = Visibility.Visible;
            submitAudioButton.IsEnabled = false;
            submitAudioButton.Visibility = Visibility.Collapsed;
            if (fileInteractionUtils != null)
            {
                await audioRecordingUtils.StopRecordingAudio(fileInteractionUtils.GetOutFolderFile());
                fileInteractionUtils.DeleteCurrentFile();
            }

            // Prevent accidental record starts before the previous file is done processing
            recordAudioButton.IsEnabled = true;
            cancelRecordingButton.Visibility = Visibility.Collapsed;
            cancelRecordingButton.IsEnabled = false;

            UpdateFileElements();
        }

        private void ConfirmAudioProfile(object sender, RoutedEventArgs e)
        {
            if (voiceTuneMenu.SelectedItem != null)
            {
                audioRecordingUtils.pitchChange = GetPitchModifier(voiceTuneMenu.SelectedIndex);
                previousSuccessfulCharacterConfirm = voiceTuneMenu.SelectedItem.ToString();
            }
            if (extraEffectsPrompt.SelectedItem != null)
            {
                audioRecordingUtils.requiresExtraEdits = ToBool(extraEffectsPrompt.SelectedIndex);
            }

            pitchValuesTest.Text = $"Pitch Modifier: {audioRecordingUtils.pitchChange} ({previousSuccessfulCharacterConfirm})\nDoes file require extra edits? {BoolToYesNo(audioRecordingUtils.requiresExtraEdits)}";
        }

        private void InitialFileSetup(string path)
        {
            remainingFilesCounter.Visibility = Visibility.Visible;

            fileInteractionUtils = new FileInteractionUtils(path);

            currentFile.Text = fileInteractionUtils.GetCurrentFile();
            remainingFilesCounter.Text = $"Files Remaining: {fileInteractionUtils.GetFilesRemaining().ToString("N0")}";
            audioPreview.Source = MediaSource.CreateFromUri(new Uri(fileInteractionUtils.GetCurrentFile(false)));
        }

        private void UpdateFileElements()
        {
            currentFile.Text = fileInteractionUtils.GetCurrentFile();
            remainingFilesCounter.Text = $"Files Remaining: {fileInteractionUtils.GetFilesRemaining().ToString("N0")}";
            audioPreview.Source = MediaSource.CreateFromUri(new Uri(fileInteractionUtils.GetCurrentFile(false)));
        }

        private async void CancelCurrentRecording(object sender, RoutedEventArgs e)
        {
            await audioRecordingUtils.CancelRecording(fileInteractionUtils.GetOutFolderFile());

            recordAudioButton.Visibility = Visibility.Visible;
            recordAudioButton.IsEnabled = true;
            submitAudioButton.IsEnabled = false;
            submitAudioButton.Visibility = Visibility.Collapsed;
            cancelRecordingButton.Visibility = Visibility.Collapsed;
            cancelRecordingButton.IsEnabled = false;
        }

        private float GetPitchModifier(int index)
        { 
           // The evil switch case is no more
           // Each element in the JSON array in Util/PitchData.cs corresponds to the index of the dropdown menu to select the pitch modification from. 
           // Easily modifiable if you know what you're doing

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
    }
}
