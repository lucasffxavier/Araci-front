using Araci.Applications.Abstractions;
using Microsoft.Win32;

namespace Araci.Infrastructure.Persistence
{
    public sealed class ProjectFileDialogService : IProjectFileDialogService
    {
        private const string FileFilter =
            "Projeto Araci (*.araci)|*.araci|JSON (*.json)|*.json|Todos os arquivos (*.*)|*.*";

        public string? ShowSaveDialog()
        {
            var dialog = new SaveFileDialog
            {
                Filter = FileFilter,
                DefaultExt = ".araci",
                AddExtension = true
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? ShowOpenDialog()
        {
            var dialog = new OpenFileDialog
            {
                Filter = FileFilter,
                DefaultExt = ".araci",
                CheckFileExists = true
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
