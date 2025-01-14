using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AudioReplacer.Windows.MainWindow.PageData;
partial class RecordPageViewModel : ObservableObject
{
    [ObservableProperty] private bool isIdle = true , isRecording = false, isReviewing = false;

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
