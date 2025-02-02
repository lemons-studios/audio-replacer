using AudioReplacer.Util;
using AudioReplacer.Util.Logger;
using AudioReplacer.Windows.MainWindow.Util;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.LibraryLoader;
using Windows.Media.Core;

namespace AudioReplacer.Windows.MainWindow.Pages;

public sealed partial class RecordPage // This file is among the worst written files in the project. It works though so I won't be changing it until I'm bored
{
    private AudioRecordingUtils audioRecordingUtils;

    public RecordPage()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ProjectFileUtils.OnProjectLoaded += () => UpdateFileElements();

        InitializeComponent();
        if (ProjectFileUtils.IsProjectLoaded) 
            UpdateFileElements();

        AudioPreview.MediaPlayer.Pause(); // Needed to fix an issue where audio would play after second navigation to the page after launch
    }

    [Log]
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Looping needs to be on to work around a bug in which the audio gets cut off for a split second after the first play.
        AudioPreview.MediaPlayer.IsLoopingEnabled = true;
        audioRecordingUtils = new AudioRecordingUtils();

        App.DiscordController.SetDetails("On Record Page");
        if (ProjectFileUtils.IsProjectLoaded)
            App.DiscordController.SetState($"{ProjectFileUtils.CalculatePercentageComplete()}% Complete");
    }

    [Log]
    private void UpdateRecordingValues(object sender, object args)
    {
        if (audioRecordingUtils == null) return;
        // For some reason, C# throws ArgumentOutOfRangeExceptions here. I don't know why, because the code works as intended and doesn't cause any issues whatsoever
        audioRecordingUtils.pitchChange = Generic.PitchValues[PitchMenu.SelectedIndex];
        audioRecordingUtils.effectCommand = Generic.EffectValues[EffectsMenu.SelectedIndex];
    }

    [Log]
    private void ToggleExtraEdits(object sender, object args)
    {
        audioRecordingUtils.requiresExtraEdits = !audioRecordingUtils.requiresExtraEdits;
    }

    [Log]
    private void SkipCurrentAudioFile(object sender, RoutedEventArgs e)
    {
        skipFlyout.Hide();
        ProjectFileUtils.SkipAudioTrack();
        UpdateFileElements();
        App.MainWindow.ShowNotification(InfoBarSeverity.Success, "File Skipped!", string.Empty, true);
    }

    [Log]
    private async void StartRecordingAudio(object sender, RoutedEventArgs e)
    {
        Generic.InRecordState = true;
        AudioPreview.MediaPlayer.Pause();

        if (ProjectFileUtils.IsProjectLoaded)
        {
            await audioRecordingUtils.StartRecordingAudio();
            App.DiscordController.SetSmallAsset("recording", "Recording Audio");
        }
        App.MainWindow.ToggleProgressNotification("Recording In Progress", string.Empty);
    }

    [Log]
    private async void StopRecordingAudio(object sender, RoutedEventArgs e)
    {
        await audioRecordingUtils.StopRecordingAudio();

        // Update source of audio player and the title manually
        CurrentFile.Text = "Review your recording...";
        App.DiscordController.SetSmallAsset("reviewing", "In review phase");
        await AudioPreview.DispatcherQueue.EnqueueAsync(() =>
        {
            AudioPreview.Source = MediaSource.CreateFromUri(new Uri(ProjectFileUtils.GetOutFilePath()));
            AudioPreview.MediaPlayer.Play();
        });

        App.MainWindow.ShowNotification(InfoBarSeverity.Informational, "Recording Stopped", "Entering Review Phase", true, replaceExistingNotifications: true);
    }

    [Log]
    private void UpdateFileElements(bool transcribeAudio = true)
    {
        var progressPercentage = ProjectFileUtils.CalculatePercentageComplete();
        var projectPath = ProjectFileUtils.GetProjectPath();

        PitchMenu.DispatcherQueue.TryEnqueue(() =>
        {
            PitchMenu.ItemsSource = Generic.PitchTitles;
        });

        EffectsMenu.DispatcherQueue.TryEnqueue(() =>
        {
            EffectsMenu.ItemsSource = Generic.EffectTitles;
        });

        FileProgressPanel.DispatcherQueue.TryEnqueue(() =>
        {
            FileProgressPanel.Visibility = Visibility.Visible;
        });

        CurrentFile.DispatcherQueue.TryEnqueue(() =>
        {
            CurrentFile.Text = ProjectFileUtils.GetCurrentFile().Replace(@"\", "/");

        });

        RemainingFiles.DispatcherQueue.TryEnqueue(() =>
        {
            RemainingFiles.Text = $"Files Remaining: {ProjectFileUtils.GetFileCount(projectPath):N0} ({progressPercentage}%)";
        });

        RemainingFiles.DispatcherQueue.TryEnqueue(() =>
        {
            RemainingFilesProgress.Value = progressPercentage;
        });

        AudioPreview.DispatcherQueue.TryEnqueue(() =>
        {
            AudioPreview.Source = MediaSource.CreateFromUri(new Uri(ProjectFileUtils.GetCurrentFile(false)));
            AudioPreview.TransportControls.IsEnabled = true;
        });

        App.DiscordController.SetState($"{progressPercentage}% Complete");
        App.DiscordController.SetSmallAsset("idle", "Idle");
        App.DiscordController.SetLargeAsset("appicon", $"Current File: {ProjectFileUtils.GetCurrentFileName()}");
        if(transcribeAudio) TranscribeAudio();
    }

    // Should probably move this into its own file in the future due to the sheer length of this thing
    [Log]
    private void TranscribeAudio()
    {
        var dispatcherQueue = Transcription.DispatcherQueue;
        Transcription.Text = "Now Processing....";
        Task.Run(async () =>
        {
            try
            {
                // Check if the Whisper model path exists
                if (!Path.Exists(Generic.WhisperPath) || !Generic.IntToBool(App.AppSettings.EnableTranscription))
                {
                    await dispatcherQueue.EnqueueAsync(() =>
                    {
                        Transcription.Text = string.Empty;
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
                var whisperFactory = WhisperFactory.FromPath(Generic.WhisperPath);

                // Determine the best runtime for the model
                RuntimeOptions.RuntimeLibraryOrder = [RuntimeLibrary.Cuda, RuntimeLibrary.Vulkan, RuntimeLibrary.Cpu, RuntimeLibrary.CpuNoAvx];
                var whisperProcessor = whisperFactory.CreateBuilder()
                    .WithLanguage("auto")
                    .WithTranslate()
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
                        Transcription.Text = $"Transcription:\n{result.Text}";
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
        await audioRecordingUtils.CancelRecording();
        App.MainWindow.ShowNotification(InfoBarSeverity.Informational, "Recording Cancelled", string.Empty, true, replaceExistingNotifications: true);
    }

    [Log]
    private void UpdateAudioStatus(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        Generic.InRecordState = false;

        switch (button.Name == "SubmitRecordingButton")
        {
            case true:
                // Submission Accepted
                ProjectFileUtils.DeleteCurrentFile(/* This method essentially acts as a way to confirm the submission*/);
                App.MainWindow.ShowNotification(InfoBarSeverity.Success, "Recording Accepted", "Moving to next file...", true, replaceExistingNotifications: true);
                UpdateFileElements();
                break;
            case false:
                // Submission Rejected
                File.Delete(ProjectFileUtils.GetOutFilePath());
                App.MainWindow.ShowNotification(InfoBarSeverity.Informational, "Recording Rejected", "Moving back to current file...", true, replaceExistingNotifications: true);
                UpdateFileElements(false); // To prevent transcription when it's not needed
                break;
        }
    }

    [Log]
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Prevent audio from playing on other pages if the media player is left playing
        AudioPreview.MediaPlayer.Pause();
    }
}
