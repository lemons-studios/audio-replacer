using AudioReplacer2.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage.Pickers;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Audio_Replacer_2.Util;
using WinRT.Interop;
using System.Threading.Tasks;


namespace Audio_Replacer_2
{
    public sealed partial class MainWindow : Window
    {
        private FileInteractionUtils fileInteractionUtils;
        private AudioRecordingUtils audioRecordingUtils;
        private string previousSuccessfulCharacterConfirm = "No Pitch Change";
        public MainWindow()
        {
            InitializeComponent();
            AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(500, 500, 950, 450));
            audioPreview.MediaPlayer.IsLoopingEnabled = true;
            audioPreview.MediaPlayer.MediaEnded += OnMediaEnded;
            audioRecordingUtils = new AudioRecordingUtils();

            AppWindow.SetIcon("Assets/Titlebar.ico");
        }

        private void OnMediaEnded(MediaPlayer sender, object args)
        {
            sender.PlaybackSession.Position = TimeSpan.Zero;
        }

        private async void SelectProjectFolder(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            folderPicker.FileTypeFilter.Add("*");

            // Initialize with window handle (required for desktop apps in WinUI 3, especially on Windows 10)
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                folderSelectButton.Visibility = Visibility.Collapsed;
                InitialFileSetup(folder.Path);
            }
        }

        private async void SkipCurrentAudioFile(object sender, RoutedEventArgs e)
        {
            if (fileInteractionUtils == null) return;
            audioPreview.MediaPlayer.Pause();

            var confirmSkip = new ContentDialog
            {
                Title = "Skip this file?",
                Content = "Are you sure you want to skip this file? You cannot reverse this action",
                PrimaryButtonText = "Skip",
                CloseButtonText = "Do not skip",
                XamlRoot = Content.XamlRoot
            };

            var confirmResult = await confirmSkip.ShowAsync();
            if (confirmResult == ContentDialogResult.Primary)
            {
                fileInteractionUtils.SkipAudioTrack();
                UpdateFileElements();
            }
            else audioPreview.MediaPlayer.Play();
            
        }

        private async void StartRecordingAudio(object sender, RoutedEventArgs e)
        {
            if (fileInteractionUtils == null) return;
            audioPreview.MediaPlayer.Pause();

            submitAudioButton.Visibility = Visibility.Visible;
            recordAudioButton.IsEnabled = false;
            recordAudioButton.Visibility = Visibility.Collapsed;
            if (fileInteractionUtils != null)
            {
                StorageFolder currentOutFolder = await fileInteractionUtils.GetDirNameAsStorageFolder();
                await audioRecordingUtils.StartRecordingAudio(currentOutFolder, fileInteractionUtils.GetCurrentFileName());
            }

            // Prevent audio from being submitted until after everything gets initialized
            submitAudioButton.IsEnabled = true;
            cancelRecordingButton.Visibility = Visibility.Visible;
            cancelRecordingButton.IsEnabled = true;
        }

        private async void SubmitRecordedAudio(object sender, RoutedEventArgs e)
        {
            if (fileInteractionUtils == null) return;

            recordAudioButton.Visibility = Visibility.Visible;
            submitAudioButton.IsEnabled = false;
            submitAudioButton.Visibility = Visibility.Collapsed;
            if (fileInteractionUtils != null)
            {
                await audioRecordingUtils.StopRecordingAudio(fileInteractionUtils.GetOutFolderFile());
                fileInteractionUtils.DeleteCurrentFile();
            }

            // Prevent accidental record starts before the previous file is done processing
            recordAudioButton.IsEnabled = true;
            cancelRecordingButton.Visibility = Visibility.Collapsed;
            cancelRecordingButton.IsEnabled = false;

            UpdateFileElements();
        }

        private void ConfirmAudioProfile(object sender, RoutedEventArgs e)
        {
            if (voiceTuneMenu.SelectedItem != null)
            {
                audioRecordingUtils.pitchChange = GetPitchModifier();
                previousSuccessfulCharacterConfirm = voiceTuneMenu.SelectedItem.ToString();
            }

            if (extraEffectsPrompt.SelectedItem != null)
            {
                audioRecordingUtils.requiresExtraEdits = ToBool(extraEffectsPrompt.SelectedIndex);
            }

            pitchValuesTest.Text = $"Pitch Modifier: {audioRecordingUtils.pitchChange} ({previousSuccessfulCharacterConfirm})\nDoes file require extra edits? {BoolToYesNo(audioRecordingUtils.requiresExtraEdits)}";
        }

        private void InitialFileSetup(string path)
        {
            remainingFilesCounter.Visibility = Visibility.Visible;

            fileInteractionUtils = new FileInteractionUtils(path);

            currentFile.Text = fileInteractionUtils.GetCurrentFile();
            remainingFilesCounter.Text = $"Files Remaining: {fileInteractionUtils.GetFilesRemaining().ToString("N0")}";
            audioPreview.Source = MediaSource.CreateFromUri(new Uri(fileInteractionUtils.GetCurrentFile(false)));
        }

        private void UpdateFileElements()
        {
            currentFile.Text = fileInteractionUtils.GetCurrentFile();
            remainingFilesCounter.Text = $"Files Remaining: {fileInteractionUtils.GetFilesRemaining().ToString("N0")}";
            audioPreview.Source = MediaSource.CreateFromUri(new Uri(fileInteractionUtils.GetCurrentFile(false)));
        }

        private async void CancelCurrentRecording(object sender, RoutedEventArgs e)
        {
            await audioRecordingUtils.CancelRecording(fileInteractionUtils.GetOutFolderFile());

            recordAudioButton.Visibility = Visibility.Visible;
            recordAudioButton.IsEnabled = true;
            submitAudioButton.IsEnabled = false;
            submitAudioButton.Visibility = Visibility.Collapsed;
            cancelRecordingButton.Visibility = Visibility.Collapsed;
            cancelRecordingButton.IsEnabled = false;
        }


        private float GetPitchModifier()
        {
            // "The best solution to a problem is usually the easiest one" -GlaDOS (In hindsight, maybe I should have made this a JSON)
            switch (voiceTuneMenu.SelectedIndex)
            {
                case 0:
                    return 1.025f; // Ai Ebihara
                case 1:
                    return 1.1f; // Ayane Matsunaga
                case 2:
                    return 0.475f; // Ameno-Sagiri
                case 3:
                    return 1.02f; // Chie Satonaka
                case 4:
                    return 1.05f; // Chihiro Fushimi
                case 5:
                    return 0.96f; // Daisuke Nagase
                case 6:
                    return 1.065f; // Eri Minami
                case 7:
                    return 1.05f; // Hanako Ohtani
                case 8:
                    return 1f; // Igor
                case 9:
                    return 1.085f; // Izanami
                case 10:
                    return 0.92f; // Kanji Tatsumi
                case 11:
                    return 0.94f; // Kinshiro Morooka
                case 12:
                    return 0.98f; // Kou Ichijo
                case 13:
                    return 0.91f; // Kunino-Sagiri
                case 14:
                    return 1.055f; // Kusumi-no-Okami
                case 15:
                    return 1.025f; // Margaret
                case 16:
                    return 1.03f; // Marie
                case 17:
                    return 0.965f; // Mitsuo Kubo
                case 18:
                    return 1.1f; // Nanako Dojima
                case 19:
                    return 0.94f; // Naoki Konishi
                case 20:
                    return 1.025f; // Naoto Shirogane
                case 21:
                    return 1.0475f; // Noriko Kashiwagi
                case 22:
                    return 0.92f; // Principal (Gekkoukan)
                case 23:
                    return 0.93f; // Principal (Yasogami)
                case 24:
                    return 1.0675f; // Rise Kujikawa
                case 25:
                    return 0.945f; // Ryotaro Dojima
                case 26:
                    return 1.065f; // Saki Konishi
                case 27:
                    return 0.015f; // Sayoko Uehara
                case 28:
                    return 1.0175f; // Shu Nakajima
                case 29:
                    return 0.935f; // Taro Namatame
                case 30:
                    return 1.07f; // Teddie
                case 31:
                    return 0.955f; // Tohru Adachi
                case 32:
                    return 1.017f; // Yukiko Amagi
                case 33:
                    return 0.945f; // Yosuke Hanamura
                case 34:
                    return 0.98f; // Yu Narukami
                case 35:
                    return 1.042f; // Yumi Ozawa
                case 36:
                    return 1f; // No pitch change 

                default:
                    return 1; // If for whatever reason the switch kills itself
            }
        }

        private bool ToBool(int value)
        {
            return value > 0;
        }

        private string BoolToYesNo(bool value)
        {
            return value ? "Yes" : "No";
        }

    }
}
