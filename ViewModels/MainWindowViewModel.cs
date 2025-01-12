using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AudioReplacer.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string tempButtonTesting = "Change Project Folder";
    [ObservableProperty] private string appTitle = "Audio replacer 4.0";



    [RelayCommand]
    private void NavigatePages()
    {

    }

    [RelayCommand]
    private void OpenGitHub()
    {

    }

    [RelayCommand]
    private void DownloadDependencies()
    {

    }
}