using AudioReplacer.Util;
using AudioReplacer.Util.Logger;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace AudioReplacer.Windows.MainWindow.Util;
public class AudioRecordingUtils
{
    public float pitchChange = 1;
    public string effectCommand = "";
    public bool requiresExtraEdits;
    private MediaCapture recordingCapture;

    public AudioRecordingUtils()
    {
        Task.Run(InitializeMediaCapture);
    }

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
        float validatedPitchChange = MathF.Max(pitchChange, 0.001f);
        string filter = string.IsNullOrWhiteSpace(effectCommand)
            ? $"rubberband=pitch={validatedPitchChange}"
            : $"rubberband=pitch={validatedPitchChange}, {effectCommand}";

        await Generic.SpawnProcess(Generic.FfmpegPath, $"-i \"{file}\" -af \"{filter}\" -y {tempOutFile}");

        if (File.Exists(tempOutFile))
        {
            File.Delete(file);
            File.Move(tempOutFile, file);
        }
    }
}
