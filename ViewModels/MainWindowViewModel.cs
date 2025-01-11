using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using AudioReplacer.Util;
using WinRT.Interop;

namespace AudioReplacer.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string tempButtonTesting = "Change Project Folder";
    [ObservableProperty] private string appTitle = "Audio replacer 4.0";

    public MainWindowViewModel()
    {
        string lastSelectedFolder = App.AppSettings.LastSelectedFolder;
        if (App.AppSettings.RememberSelectedFolder == 1 && lastSelectedFolder != string.Empty)
        {
            // SetupProject(App.AppSettings.LastSelectedFolder);
        }
    }

    [RelayCommand]
    private void ChangeProjectFolder()
    {
        var folderPicker = new FolderPicker {FileTypeFilter = { "*" }};
        InitializeWithWindow.Initialize(folderPicker, WindowNative.GetWindowHandle(App.MainWindow));
        var folder = Task.Run(async() => await folderPicker.PickSingleFolderAsync()).Result;
        if (folder != null)
        {
            Generic.SetProject(folder.Path);
        }
    }

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