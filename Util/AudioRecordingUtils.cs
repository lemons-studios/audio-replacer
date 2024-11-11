using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace Audio_Replacer_2.Util
{
    
    public class AudioRecordingUtils
    {
        public float pitchChange = 1;
        public bool requiresExtraEdits = false;

        private MediaCapture recordingCapture;
        public AudioRecordingUtils()
        {
            Task.Run(InitializeMediaCapture);
        }

        private async Task InitializeMediaCapture()
        {
            recordingCapture = new MediaCapture();
            MediaCaptureInitializationSettings captureSettings = new MediaCaptureInitializationSettings()
            {
                StreamingCaptureMode = StreamingCaptureMode.Audio
            };

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
            await Task.Delay(250); // Artificial delay to add in the last moment of audio recording if the end-user stopped the recording prematurely

            await recordingCapture.StopRecordAsync();
            var outFile = $"{file}0.wav"; // Temporary name, gets renamed back to actual file name at the end

            var ffmpegProcess = new Process
            {
                StartInfo =
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{file}\" -af rubberband=pitch={pitchChange} -y \"{outFile}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            ffmpegProcess.Start();
            Task<string> outputTask = ffmpegProcess.StandardOutput.ReadToEndAsync();
            Task<string> errorOutputTask = ffmpegProcess.StandardError.ReadToEndAsync();

            // Wait for the process to exit
            await Task.WhenAll(outputTask, errorOutputTask, Task.Run(() => ffmpegProcess.WaitForExit()));

            if (ffmpegProcess.ExitCode != 0) throw new Exception();

            // Delete unedited FFMpeg file
            File.Delete(file);

            // Move the FFMpeg file to the unedited location after deletion
            File.Move(outFile, file);
        }

        public async Task CancelRecording(string path)
        {
            // No need to delay here since the recording is getting discarded
            await recordingCapture.StopRecordAsync();
            File.Delete(path);
        }

        public void DeleteRecording(string path)
        {
            File.Delete(path);
        }

        private string FormatFileName(string fileName)
        {
            return requiresExtraEdits ? $"{fileName}-ExtraEditsRequired.wav" : fileName;
        }
    }
}
