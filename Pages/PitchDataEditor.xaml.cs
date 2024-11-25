using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using AudioReplacer2.Util;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WinUIEditor;


namespace AudioReplacer2.Resources
{
    public sealed partial class PitchDataEditor : Page
    {
        public PitchDataEditor()
        {
            this.InitializeComponent();
            PitchEditor.HighlightingLanguage = "json";
            PitchEditor.Editor.SetText(File.ReadAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AudioReplacer2-Config\\PitchData.json"));
        }
    }
}
