using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls;

namespace AudioReplacer.MainWindow.Util;

/// <summary>
/// Utilities to record and process audio for Audio Replacer projects
/// </summary>
public class AudioRecordingUtils
{
    public float PitchChange = 1;
    public string EffectCommand = "";
    private MediaCapture recordingCapture;

    public AudioRecordingUtils()
    {
        Task.Run(InitializeMediaCapture);
    }

    [Log]
    private async Task InitializeMediaCapture()
    {
        recordingCapture = new MediaCapture();
        var captureSettings = new MediaCaptureInitializationSettings
        {
            StreamingCaptureMode = StreamingCaptureMode.Audio,
            AudioProcessing = AudioProcessing.Raw // I have tried the other AudioProcessing modes, but they always seem to screw up the audio in some way
        };
        await recordingCapture.InitializeAsync(captureSettings);
    }

    /// <summary>
    /// Begin writing audio from a microphone to an output file (determined by ProjectFilesUtils)
    /// </summary>
    [Log]
    public async Task StartRecordingAudio()
    {
        var fileName = ProjectFileUtils.GetCurrentFileName();
        var outputFolder = await ProjectFileUtils.GetDirectoryAsStorageFolder();
        var fileSaveLocation = await outputFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
        var encodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);

        await Task.Delay(App.AppSettings.RecordStartWaitTime);
        await recordingCapture.StartRecordToStorageFileAsync(encodingProfile, fileSaveLocation);
    }

    /// <summary>
    /// Stop Recording and apply audio filters
    /// </summary>
    [Log]
    public async Task StopRecordingAudio()
    {
        var file = ProjectFileUtils.GetOutFilePath();

        await Task.Delay(App.AppSettings.RecordEndWaitTime);
        await recordingCapture.StopRecordAsync();
        await ApplyFilters(file);
    }
    
    /// <summary>
    /// Stop recording and delete the output file
    /// </summary>
    [Log]
    public async Task CancelRecording()
    {
        await recordingCapture.StopRecordAsync();
        File.Delete(ProjectFileUtils.GetOutFilePath());
    }

    [Log]
    private async Task ApplyFilters(string file)
    {
        // FFMpeg cannot write over a file that it is using as its input, so we will create a temporary output file to apply filters onto
        var tempOutFile = $"{file}.wav";
        var validatedPitchChange = MathF.Max(PitchChange, 0.001f);
        var filter = string.IsNullOrWhiteSpace(EffectCommand)
            ? $"rubberband=pitch={validatedPitchChange}"
            : $"rubberband=pitch={validatedPitchChange}, {EffectCommand}";
        
        await AppFunctions.FfMpegCommand(file, $"-af \"{filter}\" -y", tempOutFile);
        if (File.Exists(tempOutFile))
        {
            // Move the file with filters applied to the intended location (if audio filters applied properly)
            File.Move(tempOutFile, file, true);
            return;
        }

        await App.MainWindow.ShowNotification(InfoBarSeverity.Error, "Error", "Failed to apply custom filters. Output is not modified");
    }
}
