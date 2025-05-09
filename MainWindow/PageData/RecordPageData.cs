﻿using AudioReplacer.MainWindow.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

#pragma warning disable MVVMTK0045
namespace AudioReplacer.MainWindow.PageData;
public partial class RecordPageData : ObservableObject
{
    [ObservableProperty] private bool isIdle = true, isRecording, isReviewing;

    [RelayCommand]
    private void SwitchButtonStates()
    {
        if (!ProjectFileUtils.IsProjectLoaded) return;

        // Toggle between button states when performing actions that change the state of the record page
        (IsIdle, IsRecording, IsReviewing) = IsIdle 
            ? (false, true, false) 
            : IsRecording 
                ? (false, false, true) 
                : (true, false, false);
    }

    [RelayCommand]
    private void CancelRecord()
    {
        IsRecording = false;
        IsIdle = true;
    }

    [RelayCommand]
    private void FlagExtraEdits()
    {
        ProjectFileUtils.ExtraEditsFlagged = !ProjectFileUtils.ExtraEditsFlagged;
    }
}
