using AudioReplacer.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace AudioReplacer.Windows.MainWindow.PageData;
partial class RecordPageData : ObservableObject
{
    [ObservableProperty] private bool isIdle = true, isRecording, isReviewing;
    [ObservableProperty] private List<string> pitchList = Generic.PitchTitles, effectsList = Generic.EffectTitles;

    [RelayCommand]
    private void SwitchButtonStates()
    {
        (IsIdle, IsRecording, IsReviewing) =
            IsIdle ? (false, true, false)
            : IsRecording ? (false, false, true)
            : (true, false, false);
    }

    [RelayCommand]
    private void CancelRecord()
    {
        IsRecording = false;
        IsIdle = true;
    }
}
