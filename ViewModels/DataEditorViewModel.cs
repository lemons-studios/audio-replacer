using AudioReplacer.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AudioReplacer.ViewModels;

public partial class DataEditorViewModel : ObservableObject
{
    [RelayCommand]
    private void ImportData()
    {
        
    }

    [RelayCommand]
    private void OpenHelpPage()
    {
        Generic.OpenUrl("https://github.com/lemons-studios/audio-replacer/wiki");
    }
}