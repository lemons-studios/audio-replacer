using AudioReplacer.Util;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls;
using AudioReplacer.Generic;

namespace AudioReplacer.Windows.MainWindow.Util;
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
            AudioProcessing = AudioProcessing.Raw
        };
        await recordingCapture.InitializeAsync(captureSettings);
    }

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

    [Log]
    public async Task StopRecordingAudio(bool discarding = false)
    {
        var file = ProjectFileUtils.GetOutFilePath();

        await Task.Delay(App.AppSettings.RecordEndWaitTime);
        await recordingCapture.StopRecordAsync();
        await ApplyFilters(file);
    }

    [Log]
    public async Task CancelRecording()
    {
        await recordingCapture.StopRecordAsync();
        File.Delete(ProjectFileUtils.GetOutFilePath());
    }

    [Log]
    private async Task ApplyFilters(string file)
    {
        var tempOutFile = $"{file}.wav";
        float validatedPitchChange = MathF.Max(PitchChange, 0.001f);
        string filter = string.IsNullOrWhiteSpace(EffectCommand)
            ? $"rubberband=pitch={validatedPitchChange}"
            : $"rubberband=pitch={validatedPitchChange}, {EffectCommand}";

        await AppFunctions.SpawnProcess(AppProperties.FfmpegPath, $"-i \"{file}\" -af \"{filter}\" -y {tempOutFile}");

        if (File.Exists(tempOutFile))
        {
            File.Delete(file);
            File.Move(tempOutFile, file);
        }
        else
        {
            await App.MainWindow.ShowNotification(InfoBarSeverity.Error, "Error",
                "FFMpeg command has failed to execute. Output is not modified");
        }
    }
}
