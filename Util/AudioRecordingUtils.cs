using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace AudioReplacer.Util
{
    public class AudioRecordingUtils
    {
        public float pitchChange = 1;
        public string effectCommand = "";
        public bool requiresExtraEdits = false;
        private MediaCapture recordingCapture;

        public AudioRecordingUtils() { Task.Run(InitializeMediaCapture); }

        private async Task InitializeMediaCapture()
        {
            recordingCapture = new MediaCapture();
            var captureSettings = new MediaCaptureInitializationSettings { StreamingCaptureMode = StreamingCaptureMode.Audio };
            await recordingCapture.InitializeAsync(captureSettings);
        }

        public async Task StartRecordingAudio(StorageFolder saveFolder, string fileName)
        {
            var fileSaveLocation = await saveFolder.CreateFileAsync(FormatFileName(fileName), CreationCollisionOption.ReplaceExisting);
            var encodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
            await recordingCapture.StartRecordToStorageFileAsync(encodingProfile, fileSaveLocation);
        }

        public async Task StopRecordingAudio(string file)
        {
            await Task.Delay(GlobalData.RecordStopDelay); 
            await recordingCapture.StopRecordAsync();
            string outFile = $"{file}0.wav"; // Temporary name, gets renamed back to actual file name at the end
            float truePitchChange = MathF.Max(pitchChange, 0.001f); // Rubberband does not support pitch values <= 0

            // FFMpeg is used with shell commands here simply because I cannot bother trying to figure out .NET FFMpeg frameworks that are all just command wrappers anyway
            var ffmpegProcess = ShellCommandManager.CreateProcess("ffmpeg", $"-i \"{file}\" -af \"rubberband=pitch={truePitchChange}, volume=1.25\" -y \"{outFile}\"");
            ffmpegProcess.Start();
            await ffmpegProcess.WaitForExitAsync();
            if (ffmpegProcess.ExitCode != 0) throw new Exception();

            // Delete unedited file and move the processed file to the unedited location after deletion
            File.Delete(file);
            File.Move(outFile, file);

            // Repeat the last few lines but for adding audio effects (changing pitch should come before extra audio effects always) IF any effects are selected
            if (effectCommand != "")
            {
                var ffmpegEffectProcess = ShellCommandManager.CreateProcess("ffmpeg", $"-i \"{file}\" {effectCommand} -y \"{outFile}\"");
                ffmpegEffectProcess.Start();
                await ffmpegEffectProcess.WaitForExitAsync();
                if (ffmpegProcess.ExitCode != 0) throw new Exception();

                File.Delete(file);
                File.Move(outFile, file);
            }
        }

        public async Task CancelRecording(string path)
        {
            // No need to delay here since the recording is getting discarded
            await recordingCapture.StopRecordAsync();
            File.Delete(path);
        }

        private string FormatFileName(string fileName)
        {
            return requiresExtraEdits ? $"ExtraEditsRequired-{fileName}" : fileName;
        }
    }
}
