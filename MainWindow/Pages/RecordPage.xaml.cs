using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AudioReplacer.Generic;
using AudioReplacer.MainWindow.Util;
using AudioReplacer.Util;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Whisper.net;
using Whisper.net.LibraryLoader;
using Windows.Media.Core;

namespace AudioReplacer.MainWindow.Pages;
public sealed partial class RecordPage // This file is among the worst written files in the project. It works though so I won't be changing it until I'm bored
{
    private AudioRecordingUtils audioRecordingUtils;
    private bool viewingOriginal;

    public RecordPage()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ProjectFileUtils.OnProjectLoaded += () => UpdateFileElements(firstLoad: true);

        InitializeComponent();
        if (ProjectFileUtils.IsProjectLoaded) 
            UpdateFileElements();

        AudioPreview.MediaPlayer.Pause(); // Needed to fix an issue where audio would play after second navigation to the page after launch
    }

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
    private void SkipCurrentAudioFile(object sender, RoutedEventArgs e)
    {
        // Hide popup regardless of if a project is loaded
        SkipFileFlyout.Hide();

        if (ProjectFileUtils.IsProjectLoaded)
        {
            
            ProjectFileUtils.SkipAudioTrack();
            UpdateFileElements();
            App.MainWindow.ShowNotification(InfoBarSeverity.Success, "File Skipped!", string.Empty, true);
        }
    }

    [Log]
    private void StartRecordingAudio(object sender, RoutedEventArgs e)
    {
        if (!ProjectFileUtils.IsProjectLoaded) 
            return;
        
        Task.Run(async () =>
        {
            AppProperties.InRecordState = true;
            AudioPreview.MediaPlayer.Pause();
            await audioRecordingUtils.StartRecordingAudio();
            App.DiscordController.SetSmallAsset("recording", "Recording Audio");
            App.MainWindow.ToggleProgressNotification("Recording In Progress", string.Empty);
        });
    }

    [Log]
    private void StopRecordingAudio(object sender, RoutedEventArgs e)
    {
        Task.Run(async () =>
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
        });
    }

    [Log]
    private void UpdateFileElements(bool transcribeAudio = true, bool firstLoad = false)
    {
        var progressPercentage = ProjectFileUtils.CalculatePercentageComplete();
        var projectPath = ProjectFileUtils.GetProjectPath();

        DispatcherQueue.TryEnqueue(() =>
        {
            FileProgressPanel.Visibility = Visibility.Visible;
            CurrentFile.Text = ProjectFileUtils.GetCurrentFile().Replace(@"\", "/");
            RemainingFiles.Text = $"Files Remaining: {ProjectFileUtils.GetFileCount(projectPath):N0} ({progressPercentage}%)";
            RemainingFilesProgress.Value = progressPercentage;
            AudioPreview.Source = MediaSource.CreateFromUri(new Uri(ProjectFileUtils.GetCurrentFile(false)));
            AudioPreview.TransportControls.IsEnabled = true;
        });

        App.DiscordController.SetState($"{progressPercentage}% Complete");
        App.DiscordController.SetSmallAsset("idle", "Idle");
        App.DiscordController.SetLargeAsset("appicon", $"Current File: {ProjectFileUtils.GetCurrentFileName()}");

        if (firstLoad)
        {
            AudioPreview.DispatcherQueue.TryEnqueue(() =>
            {
                AudioPreview.MediaPlayer.Pause();
            });
        }

        if (transcribeAudio)
            TranscribeAudio();
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
                if (!Path.Exists(AppProperties.WhisperPath) || !AppFunctions.IntToBool(App.AppSettings.EnableTranscription))
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
                var whisperFactory = WhisperFactory.FromPath(AppProperties.WhisperPath);

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

    private void CancelCurrentRecording(object sender, RoutedEventArgs e)
    {
        Task.Run(async () =>
        {
            AppProperties.InRecordState = false;
            await audioRecordingUtils.CancelRecording();
            App.MainWindow.ShowNotification(InfoBarSeverity.Informational, "Recording Cancelled", string.Empty, true, replaceExistingNotifications: true);
        });
    }

    [Log]
    private void AcceptSubmission(object sender, RoutedEventArgs e)
    {
        AppProperties.InRecordState = false;
        viewingOriginal = false;

        ProjectFileUtils.SubmitAudioFile(); // Extra edits get renamed in this method
        App.MainWindow.ShowNotification(InfoBarSeverity.Success, "Recording Accepted", "Moving to next file...", true, replaceExistingNotifications: true);
        UpdateFileElements();
    }

    [Log]
    private void RejectSubmission(object sender, RoutedEventArgs e)
    {
        AppProperties.InRecordState = false;
        viewingOriginal = false;

        File.Delete(ProjectFileUtils.GetOutFilePath());
        App.MainWindow.ShowNotification(InfoBarSeverity.Informational, "Recording Rejected", "Moving back to current file...", true, replaceExistingNotifications: true);
        UpdateFileElements(false); // To prevent transcription when it's not needed
    }
    
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Prevent audio from playing on other pages if the media player is left playing
        AudioPreview.MediaPlayer.Pause();
    }

    // TODO: Maybe reduce how much boilerplate and reused code this uses

    [Log]
    private void OnPitchSearchChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suggestions = AppProperties.PitchTitles
                .Where(item => item.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
            if (!string.IsNullOrEmpty(sender.Text))
            {
                sender.ItemsSource = suggestions;
            }
        }
    }

    [Log]
    private void OnPitchFocus(object sender, RoutedEventArgs e)
    {
        var suggestionBox = (AutoSuggestBox) sender;
        suggestionBox.ItemsSource = AppProperties.PitchTitles;
        suggestionBox.IsSuggestionListOpen = true;
        OnPitchSearchChanged(suggestionBox, new AutoSuggestBoxTextChangedEventArgs() { Reason = AutoSuggestionBoxTextChangeReason.UserInput });
    }

    [Log]
    private void PitchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var selectedText = args.ChosenSuggestion?.ToString() ?? args.QueryText;
        int selectedPitchPosition = GetPositionOfElementInData(selectedText, false);
        audioRecordingUtils.PitchChange = AppProperties.PitchValues[selectedPitchPosition];
    }

    [Log]
    private void EffectQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var selectedText = args.ChosenSuggestion?.ToString() ?? args.QueryText;
        int selectedEffectPosition = GetPositionOfElementInData(selectedText, true);
        audioRecordingUtils.EffectCommand = AppProperties.EffectValues[selectedEffectPosition];
    }

    [Log]
    private void OnEffectSearchChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        var suggestions = AppProperties.EffectTitles
            .Where(item => item.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase))
            .ToList();
        if (!string.IsNullOrEmpty(sender.Text))
        {
            sender.ItemsSource = suggestions;
        }
    }

    [Log]
    private void OnEffectFocus(object sender, RoutedEventArgs e)
    {
        var suggestionBox = (AutoSuggestBox) sender;
        suggestionBox.ItemsSource = AppProperties.EffectTitles;
        suggestionBox.IsSuggestionListOpen = true;
        OnEffectSearchChanged(suggestionBox, new AutoSuggestBoxTextChangedEventArgs() { Reason = AutoSuggestionBoxTextChangeReason.UserInput });
    }

    [Log]
    public int GetPositionOfElementInData(string x, bool effectsData)
    {
        var dataList = effectsData ? AppProperties.EffectTitles : AppProperties.PitchTitles;
        for (int i = 0; i < dataList.Count; i++)
        {
            if (x == dataList[i])
                return i;
        }
        throw new Exception(); 
    }

    // Sometimes the best solution to a problem is the easiest one
    [Log]
    private void SwitchViewingAudio(object sender, RoutedEventArgs e)
    {
        AudioPreview.MediaPlayer.Source = MediaSource.CreateFromUri(new Uri(viewingOriginal ? ProjectFileUtils.GetOutFilePath() : ProjectFileUtils.GetCurrentFile(false)));
        viewingOriginal = !viewingOriginal;
    }
}
