using AudioReplacer.Util;
using AudioReplacer.Windows.MainWindow.Util;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.LibraryLoader;
using Windows.Media.Core;

namespace AudioReplacer.Windows.MainWindow.Pages;

public sealed partial class RecordPage
{
    private AudioRecordingUtils audioRecordingUtils;

    public RecordPage()
    {
        Loaded += OnLoaded;
        ProjectFileUtils.OnProjectLoaded += UpdateFileElements;
        InitializeComponent();
        if (ProjectFileUtils.IsProjectLoaded)
            UpdateFileElements();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        VoiceTuneMenu.ItemsSource = Generic.PitchTitles;
        EffectsMenu.ItemsSource = Generic.EffectTitles;

        // Looping needs to be on to work around a bug in which the audio gets cut off for a split second after the first play.
        AudioPreview.MediaPlayer.IsLoopingEnabled = true;
        audioRecordingUtils = new AudioRecordingUtils();

        App.DiscordController.SetDetails("On Record Page");
        if (ProjectFileUtils.IsProjectLoaded)
            App.DiscordController.SetState($"{ProjectFileUtils.CalculatePercentageComplete()}% Complete");
    }

    private void UpdateRecordingValues()
    {
        if (audioRecordingUtils == null) return;
        if (VoiceTuneMenu.SelectedItem != null)
        {
            audioRecordingUtils.pitchChange = Generic.PitchValues[VoiceTuneMenu.SelectedIndex];
        }
        if (EffectsMenu.SelectedItem != null)
        {
            audioRecordingUtils.effectCommand = Generic.EffectValues[EffectsMenu.SelectedIndex];
        }
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

        TranscribeAudio();
    }

    private void TranscribeAudio()
    {
        var dispatcherQueue = Transcription.DispatcherQueue;
        Transcription.Text = "Now Processing....";
        Task.Run(async () =>
        {
            try
            {
                // Check if the Whisper model path exists
                if (!Path.Exists(Generic.whisperPath))
                {
                    await dispatcherQueue.EnqueueAsync(() =>
                    {
                        Transcription.Text = "Download speech-to-text data from settings to transcribe audio files.";
                    });
                    return;
                }

                // Validate the current file format
                var currentFile = ProjectFileUtils.GetCurrentFile(false);
                if (!currentFile.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                {
                    await dispatcherQueue.EnqueueAsync(() =>
                    {
                        Transcription.Text = "Only .wav files support transcription.";
                    });
                    return;
                }

                // Initialize Whisper model and processor
                var whisperFactory = WhisperFactory.FromPath(Generic.whisperPath);
                RuntimeOptions.RuntimeLibraryOrder = [ RuntimeLibrary.Cuda, RuntimeLibrary.Vulkan, RuntimeLibrary.Cpu, RuntimeLibrary.CpuNoAvx];
                var whisperProcessor = whisperFactory.CreateBuilder()
                    .WithLanguage("auto")
                    .Build();

                // Do some funky wizardry to make the file work with Whisper.NET
                await using var fileStream = File.OpenRead(currentFile);
                using var wavStream = new MemoryStream();
                await using var reader = new WaveFileReader(fileStream);
                var resamplingProcessor = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
                WaveFileWriter.WriteWavFileToStream(wavStream, resamplingProcessor.ToWaveProvider16());
                wavStream.Seek(0, SeekOrigin.Begin);

                // Process audio file
                await foreach (var result in whisperProcessor.ProcessAsync(wavStream))
                {
                    await dispatcherQueue.EnqueueAsync(() =>
                    {
                        Transcription.Text = $"Transcription: \n{result.Text}";
                    });
                }
            }
            catch (Exception ex)
            {
                await dispatcherQueue.EnqueueAsync(() =>
                {
                    Transcription.Text = $"Error: {ex.Message}";
                });
                Console.WriteLine($"Error during transcription: {ex}");
            }
        });
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
                ProjectFileUtils.DeleteCurrentFile(
/* This method essentially acts as a way to confirm the submission*/);
                break;
            case false:
                // Submission Rejected
                File.Delete(ProjectFileUtils.GetOutFilePath());
                break;
        }

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

    public string GetFormattedCurrentFile(string input)
    {
        return input.Replace(@"\", "/");
    }

    public MediaSource MediaSourceFromUri(string path)
    {
        return MediaSource.CreateFromUri(new Uri(path));
    }
}