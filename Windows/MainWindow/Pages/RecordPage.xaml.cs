using AudioReplacer.Util;
using AudioReplacer.Windows.MainWindow.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SevenZipExtractor;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AudioReplacer.Windows.MainWindow.Pages
{
    public sealed partial class RecordPage
    {
        private AudioRecordingUtils audioRecordingUtils;

        public RecordPage()
        {
            Loaded += OnLoaded;
            ProjectFileUtils.OnProjectLoaded += ProjectFileUtilsOnOnProjectLoaded;
            InitializeComponent();
            if(ProjectFileUtils.IsProjectLoaded)
                UpdateFileElements();
        }

        private void ProjectFileUtilsOnOnProjectLoaded()
        {
            UpdateFileElements();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            VoiceTuneMenu.ItemsSource = Generic.pitchMenuTitles;
            EffectsMenu.ItemsSource = Generic.effectMenuTitles;

            // Looping needs to be on to work around a bug in which the audio gets cut off for a split second after the first play.
            AudioPreview.MediaPlayer.IsLoopingEnabled = true;
            audioRecordingUtils = new AudioRecordingUtils();

            switch (IsFfMpegAvailable())
            {
                case true: // Check For Updates
                    break;
                case false: // Show popup for dependency install requirement
                    VoiceTuneMenu.IsEnabled = false;
                    EffectsMenu.IsEnabled = false;
                    App.MainWindow.DisableFolderChanger();
                    break;
            }

            App.DiscordController.SetDetails("On Record Page");
            if(ProjectFileUtils.IsProjectLoaded)
                App.DiscordController.SetState($"{ProjectFileUtils.CalculatePercentageComplete()}% Complete");
        }

        private void UpdateRecordingValues()
        {
            if (audioRecordingUtils == null) return;
            if (VoiceTuneMenu.SelectedItem != null)
            {
                audioRecordingUtils.pitchChange = Generic.pitchValues[VoiceTuneMenu.SelectedIndex];
            }
            if (EffectsMenu.SelectedItem != null)
            {
                audioRecordingUtils.effectCommand = Generic.effectMenuValues[EffectsMenu.SelectedIndex];
            }
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
            App.AppSettings.LastSelectedFolder = folder.Path;
            ProjectSetup(folder.Path);
        }

        private async void SkipCurrentAudioFile(object sender, RoutedEventArgs e)
        {
            if (!ProjectFileUtils.IsProjectLoaded) return;
            AudioPreview.MediaPlayer.Pause();
            var confirmSkip = new ContentDialog
            {
                Title = "Skip this file?", 
                Content = "Are you sure you want to skip this file?", 
                PrimaryButtonText = "Skip", 
                CloseButtonText = "Don't Skip", 
                XamlRoot = Content.XamlRoot
            };
            var confirmResult = await confirmSkip.ShowAsync();

            switch (confirmResult == ContentDialogResult.Primary)
            {
                case true:
                    ProjectFileUtils.SkipAudioTrack();
                    UpdateFileElements(); 
                    break;
                case false:
                    break;
            }
        }

        private async void StartRecordingAudio(object sender, RoutedEventArgs e)
        {
            Generic.InRecordState = true;
            AudioPreview.MediaPlayer.Pause();

            if (ProjectFileUtils.IsProjectLoaded)
            {
                await audioRecordingUtils.StartRecordingAudio();
                App.DiscordController.SetSmallAsset("recording", "Recording Audio");
            }
        }

        private async void StopRecordingAudio(object sender, RoutedEventArgs e)
        {
            await audioRecordingUtils.StopRecordingAudio();

            // Update source of audio player and the title manually
            CurrentFile.Text = "Review your recording...";
            App.DiscordController.SetSmallAsset("reviewing", "In review phase");
            AudioPreview.Source = MediaSourceFromUri(ProjectFileUtils.GetOutFilePath());
        }

        private void UpdateFileElements()
        {
            var progressPercentage = ProjectFileUtils.CalculatePercentageComplete();
            var projectPath = ProjectFileUtils.GetProjectPath();
            SkipAudioButton.IsEnabled = true;
            StartRecordingButton.IsEnabled = true;


            FileProgressPanel.Visibility = Visibility.Visible;
            CurrentFile.Text = GetFormattedCurrentFile(ProjectFileUtils.GetCurrentFile());
            RemainingFiles.Text = $"Files Remaining: {ProjectFileUtils.GetFileCount(projectPath):N0} ({progressPercentage}%)";
            RemainingFilesProgress.Value = progressPercentage;
            AudioPreview.Source = MediaSourceFromUri(ProjectFileUtils.GetCurrentFile(false));
            AudioPreview.TransportControls.IsEnabled = true;
            App.DiscordController.SetState($"{progressPercentage}% Complete");
            App.DiscordController.SetSmallAsset("idle", "Idle");
            App.DiscordController.SetLargeAsset("appicon", $"Current File: {ProjectFileUtils.GetCurrentFileName()}");
        }

        private async void CancelCurrentRecording(object sender, RoutedEventArgs e)
        {
            Generic.InRecordState = false;
            await audioRecordingUtils.StopRecordingAudio(true);
        }

        private void UpdateAudioStatus(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            Generic.InRecordState = false;
            bool isSubmitButton = button.Name == "SubmitRecordingButton";

            switch (isSubmitButton)
            {
                case true:
                    // Submission Accepted
                    ProjectFileUtils.DeleteCurrentFile(/* This method essentially acts as a way to confirm the submission*/);
                    break;
                case false:
                    // Submission Rejected
                    File.Delete(ProjectFileUtils.GetOutFilePath());
                    break;
            }
            UpdateFileElements();
        }

        public void ProjectSetup(string path)
        {
            ProjectFileUtils.SetProjectData(path);
            FileProgressPanel.Visibility = Visibility.Visible;
            AudioPreviewControls.IsEnabled = true;
            UpdateFileElements();
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

        public void DownloadDependencies()
        {
            string latestFfMpegVersion = Task.Run(() => Generic.GetWebData("https://www.gyan.dev/ffmpeg/builds/release-version")).Result;
            string ffMpegUrl = $"https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-{latestFfMpegVersion}-full_build.7z";
            string outPath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\audio-replacer\";
            string fullOutPath = $@"{outPath}\ffmpeg";
            string currentSystemPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

            Generic.DownloadFile(ffMpegUrl, outPath, "ffmpeg.7z");
            using (ArchiveFile ffmpegArchive = new ArchiveFile($"{fullOutPath}.7z")) { ffmpegArchive.Extract($"{fullOutPath}"); }

            Directory.Move(@$"{fullOutPath}\ffmpeg-{latestFfMpegVersion}-full_build\bin", @$"{outPath}\ffmpeg-bin");
            string updatedPath = $"{currentSystemPath};{Path.Combine(outPath, "ffmpeg-bin")}";
            Environment.SetEnvironmentVariable("PATH", updatedPath, EnvironmentVariableTarget.User);

            // Delete both the downloaded 7z archive and the ffmpeg folder it came in
            File.Delete($"{fullOutPath}.7z");
            Directory.Delete($"{fullOutPath}", true);
        }

        public bool IsFfMpegAvailable()
        {
            return File.Exists(Path.Combine(Generic.extraApplicationData, "ffmpeg-bin", "ffmpeg.exe"));
        }

        public void ToggleButton(Button button, bool toggle)
        {
            var toggleVisibility = ToVisibility(toggle);

            button.IsEnabled = toggle;
            button.Visibility = toggleVisibility;
        }

        private Visibility ToVisibility(bool x)
        {
            return x ? Visibility.Visible : Visibility.Collapsed;
        }

        public string GetFormattedCurrentFile(string input)
        {
            return input.Replace(@"\", "/");
        }

        public MediaSource MediaSourceFromUri(string path)
        {
            return MediaSource.CreateFromUri(new Uri(path));
        }
    }
}