using AudioReplacer.MainWindow.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AudioReplacer.MainWindow.PageData;
partial class HomePageData : ObservableObject
{
    [ObservableProperty] private bool previousProjectExists = !string.IsNullOrEmpty(App.AppSettings.LastSelectedFolder) && Path.Exists(App.AppSettings.LastSelectedFolder);

    [ObservableProperty] private string folderName = !string.IsNullOrEmpty(App.AppSettings.LastSelectedFolder)
        ? Path.GetDirectoryName(App.AppSettings.LastSelectedFolder)
        : "No Previous Project In Memory";

    [RelayCommand]
    private void LoadPreviousProject()
    {
        ProjectFileUtils.SetProjectData(App.AppSettings.LastSelectedFolder);
        App.MainWindow.OpenRecordPage();
    }

    [RelayCommand]
    private async Task OpenNewProject()
    {
        var folderPicker = new FolderPicker { FileTypeFilter = { "*" } };
        InitializeWithWindow.Initialize(folderPicker, WindowNative.GetWindowHandle(App.MainWindow));
        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null && !string.IsNullOrEmpty(folder.Path))
        {
            var folderPath = folder.Path;
            if (AppFunctions.IntToBool(App.AppSettings.RememberSelectedFolder))
                App.AppSettings.LastSelectedFolder = folderPath;

            ProjectFileUtils.SetProjectData(folderPath);
            App.MainWindow.OpenRecordPage();
        }
    }
}