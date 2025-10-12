using Ookii.Dialogs.Wpf;

namespace OffRouteMap
{
    public class FolderDialogService : IFolderDialogService
    {
        public string ShowSelectFolderDialog(string title, string initialDirectory = null)
        {
            var dlg = new VistaFolderBrowserDialog
            {
                Description = title,
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
                SelectedPath = initialDirectory ?? string.Empty
            };

            bool? result = dlg.ShowDialog();
            return result == true ? dlg.SelectedPath : null;
        }
    }
}
