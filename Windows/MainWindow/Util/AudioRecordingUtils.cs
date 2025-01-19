using AudioReplacer.Util;
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
    public bool requiresExtraEdits = false;
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

    public async Task StartRecordingAudio()
    {
        string fileName = ProjectFileUtils.GetCurrentFileName();

        string formattedFileName = requiresExtraEdits ? $"ExtraEditsRequired-{fileName}" : fileName;
        var outputFolder = await ProjectFileUtils.GetDirectoryAsStorageFolder();
        var fileSaveLocation = await outputFolder.CreateFileAsync(formattedFileName, CreationCollisionOption.ReplaceExisting);
        var encodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);

        await Task.Delay(App.AppSettings.RecordStartWaitTime);
        await recordingCapture.StartRecordToStorageFileAsync(encodingProfile, fileSaveLocation);
    }

    public async Task StopRecordingAudio(bool discarding = false)
    {
        var file = ProjectFileUtils.GetOutFilePath();
        switch (discarding)
        {
            case true:
                await recordingCapture.StopRecordAsync();
                File.Delete(file);
                return;
            case false:
                await Task.Delay(App.AppSettings.RecordEndWaitTime);
                await recordingCapture.StopRecordAsync();
                await ApplyFilters(file);
                return;
        }
    }

    private async Task ApplyFilters(string file)
    {
        string outFile = $"{file}0.wav"; // Temporary name. FFmpeg doesn't like it when the user tries to set the output as the input since
        float validatedPitchChange = MathF.Max(pitchChange, 0.001f); // Pitch values below zero do not work
        string command = string.IsNullOrEmpty(effectCommand)
            ? $"rubberband=pitch={validatedPitchChange}"
            : $"rubberband=pitch={validatedPitchChange}, {effectCommand}";
        await File.WriteAllTextAsync(Path.Join(Generic.extraApplicationData, "coolThing.txt"), command);
        await Generic.SpawnProcess("ffmpeg", $"-i {file} -af \"{command}\" -y {outFile}");
        File.Delete(file);
        File.Move(outFile, file);
    }
}

