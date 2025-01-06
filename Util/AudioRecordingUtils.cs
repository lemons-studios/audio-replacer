using AudioReplacer.Generic;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media;
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

        public AudioRecordingUtils()
        {
            Task.Run(InitializeMediaCapture);
        }

        private async Task InitializeMediaCapture()
        {
            recordingCapture = new MediaCapture();
            var captureSettings = new MediaCaptureInitializationSettings { StreamingCaptureMode = StreamingCaptureMode.Audio, AudioProcessing = AudioProcessing.Raw }; // MediaCategory.Speech applies additional audio effects like noise and speech cancellation
            await recordingCapture.InitializeAsync(captureSettings);
        }

        public async Task StartRecordingAudio(string saveLocation, string fileName)
        {
            var formattedFileName = requiresExtraEdits ? $"ExtraEditsRequired-{fileName}" : fileName;
            var outputFolder = await StorageFolder.GetFolderFromPathAsync(saveLocation);
            var fileSaveLocation = await outputFolder.CreateFileAsync(formattedFileName, CreationCollisionOption.ReplaceExisting);
            var encodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);

            await Task.Delay(AppGeneric.RecordStartDelay);
            await recordingCapture.StartRecordToStorageFileAsync(encodingProfile, fileSaveLocation);
        }

        public async Task StopRecordingAudio(string file, bool discarding = false)
        {
            switch (discarding)
            {
                case true:
                    await recordingCapture.StopRecordAsync();
                    File.Delete(file);
                    return;
                case false:
                    await Task.Delay(AppGeneric.RecordStopDelay);
                    await recordingCapture.StopRecordAsync();
                    await ApplyFilters(file);
                    return;
            }
        }

        private async Task ApplyFilters(string file)
        {
            var outFile = $"{file}0.wav"; // Temporary name. FFmpeg doesn't like it when the user tries to set the output as the input since
            var validatedPitchChange = MathF.Max(pitchChange, 0.001f); // Pitch values below zero do not work
            var command = string.IsNullOrEmpty(effectCommand)
                ? $"rubberband=pitch={validatedPitchChange}"
                : $"rubberband=pitch{validatedPitchChange},{effectCommand}";
            await AppGeneric.SpawnProcess("ffmpeg", $"-i \"{file}\" -af \"{command}\" -y \"{outFile}\"");

            // Move the ffmpeg-modified audio file to the intended location after deleting the unedited output file
            File.Delete(file);
            File.Move(outFile, file);
        }
    }
}
