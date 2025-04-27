using System.Linq;
using AudioReplacer.MainWindow.Util;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Windows.Media.Core;

// ReSharper disable AsyncVoidMethod
namespace AudioReplacer.MainWindow.Pages;
/// <summary>
/// Record Page Functions
/// </summary>
public sealed partial class RecordPage
{
    // Least clean class in audio replacer, but it works, so I am NOT complaining
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

        App.DiscordController.SetDetails("Record Page");
        if (ProjectFileUtils.IsProjectLoaded)
            App.DiscordController.SetState($"{ProjectFileUtils.CalculateCompletion()}% Complete");
    }

    [Log]
    private void SkipCurrentAudioFile(object sender, RoutedEventArgs e)
    {
        // Hide popup regardless of if a project is loaded
        SkipFileFlyout.Hide();
        if (!ProjectFileUtils.IsProjectLoaded) return;
        
        ProjectFileUtils.SkipFile();
        UpdateFileElements();
        App.MainWindow.ShowNotification(InfoBarSeverity.Success, "File Skipped!", string.Empty, true);
    }

    [Log]
    private async void StartRecordingAudio(object sender, RoutedEventArgs e)
    {
        if (!ProjectFileUtils.IsProjectLoaded) return;

        AppProperties.InRecordState = true;
        AudioPreview.MediaPlayer.Pause();
        await audioRecordingUtils.StartRecordingAudio();
        App.DiscordController.SetSmallAsset("recording", "Recording Audio");
        App.MainWindow.ShowProgressNotification("Recording In Progress", string.Empty);
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
    private void UpdateFileElements(bool firstLoad = false)
    {
        var progressPercentage = ProjectFileUtils.CalculateCompletion();
        var projectPath = ProjectFileUtils.GetProjectPath();

        DispatcherQueue.TryEnqueue(() =>
        {
            Transcription.Visibility = AppFunctions.IntToBool(App.AppSettings.EnableTranscription) 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            FileProgressPanel.Visibility = Visibility.Visible;
            CurrentFile.Text = ProjectFileUtils.GetCurrentFile().Replace(@"\", "/");
            RemainingFiles.Text = $"Files Remaining: {ProjectFileUtils.GetFileCount(projectPath):N0} ({progressPercentage}%)";
            RemainingFilesProgress.Value = progressPercentage;
            try
            {
                AudioPreview.Source = MediaSource.CreateFromUri(new Uri(ProjectFileUtils.GetCurrentFile(false)));
                AudioPreview.TransportControls.IsEnabled = true;
            }
            catch (UriFormatException) // This happens when project directories are empty
            {
                AudioPreview.Source = null;
            }
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

        if (AppFunctions.IntToBool(App.AppSettings.EnableTranscription))
        {
            var dispatcherQueue = Transcription.DispatcherQueue;
            Transcription.DispatcherQueue.TryEnqueue(() =>
            {
                Transcription.Text = "Now Processing....";
            });

            Task.Run(async () =>
            {
                var transcription = await AppFunctions.TranscribeFile(ProjectFileUtils.GetCurrentFile(false));
                await dispatcherQueue.EnqueueAsync(() =>
                {
                    Transcription.Text = transcription;
                });
            });
        }
    }

    private async void CancelCurrentRecording(object sender, RoutedEventArgs e)
    {
        AppProperties.InRecordState = false;
        await audioRecordingUtils.CancelRecording();
        App.MainWindow.ShowNotification(InfoBarSeverity.Informational, "Recording Cancelled", string.Empty, true, replaceExistingNotifications: true);
    }

    [Log]
    private void AcceptSubmission(object sender, RoutedEventArgs e)
    {
        AppProperties.InRecordState = false;
        viewingOriginal = false;

        ProjectFileUtils.SubmitFile(); // Extra edits get renamed in this method
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
        UpdateFileElements();
    }
    
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Prevent audio from playing on other pages if the media player is left playing
        AudioPreview.MediaPlayer.Pause();
    }

    [Log]
    private void OnPitchSearchChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
            return;
        
        var suggestions = AppProperties.PitchTitles
            .Where(item => item.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase))
            .ToList();
        if (!string.IsNullOrEmpty(sender.Text))
            sender.ItemsSource = suggestions;
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
        var selectedPitchPosition = GetPositionOfElementInData(selectedText, false);
        audioRecordingUtils.PitchChange = AppProperties.PitchValues[selectedPitchPosition];
    }

    [Log]
    private void EffectQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var selectedText = args.ChosenSuggestion?.ToString() ?? args.QueryText;
        var selectedEffectPosition = GetPositionOfElementInData(selectedText, true);
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
    private int GetPositionOfElementInData(string x, bool effectsData)
    {
        var dataList = effectsData ? AppProperties.EffectTitles : AppProperties.PitchTitles;
        for (var i = 0; i < dataList.Count; i++)
        {
            if (x == dataList[i]) return i;
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
