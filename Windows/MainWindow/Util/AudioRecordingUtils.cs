using AudioReplacer.Util;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using AudioReplacer.Util.Logger;

namespace AudioReplacer.Windows.MainWindow.Util;
public class AudioRecordingUtils
{
    private float pitchChange = 1;
    private string effectCommand = string.Empty;
    public bool requiresExtraEdits;
    private MediaCapture recordingCapture;

    public AudioRecordingUtils()
    {
        Task.Run(InitializeMediaCapture);
    }

    public void SetEffectCommands(float newPitch = 1, string newEffect = "", bool extraEdits = false)
    {
        pitchChange = newPitch;
        effectCommand = newEffect;
        requiresExtraEdits = extraEdits;
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
        var formattedFileName = requiresExtraEdits ? $"ExtraEditsRequired-{fileName}" : fileName;
        var outputFolder = await ProjectFileUtils.GetDirectoryAsStorageFolder();
        var fileSaveLocation = await outputFolder.CreateFileAsync(formattedFileName, CreationCollisionOption.ReplaceExisting);
        var encodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);

        await Task.Delay(App.AppSettings.RecordStartWaitTime);
        await recordingCapture.StartRecordToStorageFileAsync(encodingProfile, fileSaveLocation);
    }

    [Log]
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

    [Log]
    private async Task ApplyFilters(string file)
    {
        var outFile = $"{file}0.wav"; // Temporary name. FFmpeg doesn't like it when the user tries to set the output as the input since
        var validatedPitchChange = MathF.Max(pitchChange, 0.001f); // Pitch values below zero do not work
        var command = string.IsNullOrEmpty(effectCommand)
            ? $"rubberband=pitch={validatedPitchChange}"
            : $"rubberband=pitch={validatedPitchChange}, {effectCommand}";
        await File.WriteAllTextAsync(Path.Join(Generic.ExtraApplicationData, "coolThing.txt"), command);
        await Generic.SpawnProcess($"{Generic.FfmpegPath}", $"-i {file} -af \"{command}\" -y {outFile}");
        File.Delete(file);
        File.Move(outFile, file);
    }
}
