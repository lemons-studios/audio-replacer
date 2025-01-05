using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace AudioReplacer.Generic
{
    public static class PageBase
    {
        public static ContentDialog CreateContentDialog(string title, string message, string primaryButtonText, XamlRoot contentRoot, string closeButtonText = "Cancel")
        {
            return new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
        }

        public static ContentDialog CreateContentDialog(string title, string message, string primaryButtonText, string secondaryButtonText, XamlRoot contentRoot, string closeButtonText = "Cancel")
        {
            return new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText,
                CloseButtonText = closeButtonText,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
        }

        public static async Task<int> GetContentDialogChoice(ContentDialog dialog)
        {
            var userSelection = await dialog.ShowAsync();
            return (int) userSelection;
        }
    }
}
