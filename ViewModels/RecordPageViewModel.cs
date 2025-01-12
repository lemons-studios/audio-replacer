using System.Collections.Generic;
using System.Threading.Tasks;
using AudioReplacer.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AudioReplacer.ViewModels;

public partial class RecordPageViewModel : ObservableObject
{
    [ObservableProperty] private int selectedPitchIndex;
    [ObservableProperty] private int selectedEffectIndex;
    [ObservableProperty] private List<string> pitchTitles = Generic.pitchMenuTitles;
    [ObservableProperty] private List<string> effectTiles = Generic.effectMenuTitles;
    [ObservableProperty] private bool isRecording;
    [ObservableProperty] private bool isReviewing;
    [ObservableProperty] private bool isIdle = true;
    [ObservableProperty] private string mainFileHeader =
        App.AppSettings.LastSelectedFolder == string.Empty || App.AppSettings.RememberSelectedFolder == 0
            ? "Select a folder to begin"
            : ProjectFileUtils.GetCurrentFile();

    public RecordPageViewModel()
    {

    }


    private readonly AudioRecordingUtils audioRecordingUtils = new();
    private void SwitchStates()
    {
        (IsIdle, IsRecording, IsReviewing) =
            IsIdle ? (false, true, false) : IsRecording ? (false, false, true) : (true, false, false);

        MainFileHeader = IsReviewing || IsRecording
            ? ProjectFileUtils.GetCurrentFile()
            : "Review Your Changes";
    }

    [RelayCommand]
    private async Task StartRecord()
    {
        if (ProjectFileUtils.IsProjectLoaded)
        {
            SwitchStates();
            await audioRecordingUtils.StartRecordingAudio();
        }
    }

    [RelayCommand]
    private void CancelRecord()
    {
        IsRecording = false;
        IsIdle = true;
    }

    [RelayCommand]
    private void SkipFile()
    {

    }

    [RelayCommand]
    private void RejectRecording()
    {
        SwitchStates();
    }

    [RelayCommand]
    private void AcceptRecording()
    {
        SwitchStates();
    }

    [RelayCommand]
    private void StopRecording()
    {
        SwitchStates();
    }

    [RelayCommand]
    private void UpdateEffects()
    {

    }

    [RelayCommand]
    private void ToggleFurtherEdits()
    {

    }


}