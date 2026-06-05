using System;
using System.Windows;
using Araci.Applications.Abstractions;
using Araci.Properties;
using Araci.ViewModels;

namespace Araci.Services.UI
{
    public class TypePropertiesDialogService : ITypePropertiesDialogService
    {
        private ICommandHistory? _commands;
        private Action? _afterTypeLibraryChanged;

        public void Configure(ICommandHistory commands, Action? afterTypeLibraryChanged)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _afterTypeLibraryChanged = afterTypeLibraryChanged;
        }

        public void Show(TipoElementoViewModel? viewModel)
        {
            if (viewModel == null)
                return;

            var window = new TypePropertiesWindow
            {
                DataContext = viewModel
            };

            if (Application.Current?.MainWindow != null)
                window.Owner = Application.Current.MainWindow;

            bool confirmado = window.ShowDialog() == true;

            if (!confirmado)
            {
                viewModel.CancelChanges();
                return;
            }

            if (viewModel is TipoTextoAnotativoViewModel textoViewModel && _commands != null)
            {
                var command = textoViewModel.CreateCommitCommand(_afterTypeLibraryChanged);

                if (command != null)
                {
                    _commands.Execute(command);
                    return;
                }
            }

            viewModel.CommitChanges();
        }
    }
}
