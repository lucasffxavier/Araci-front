using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Applications.Abstractions;
using Araci.Models;
using Araci.Properties;
using Araci.ViewModels;

namespace Araci.Services.UI
{
    public class TypePropertiesDialogService : ITypePropertiesDialogService
    {
        private ICommandHistory? _commands;
        private Action? _afterTypeLibraryChanged;
        private Func<IEnumerable<TextoAnotativo>>? _textosAnotativosProvider;

        public void Configure(ICommandHistory commands, Action? afterTypeLibraryChanged, Func<IEnumerable<TextoAnotativo>>? textosAnotativosProvider = null)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _afterTypeLibraryChanged = afterTypeLibraryChanged;
            _textosAnotativosProvider = textosAnotativosProvider;
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
                var textos = _textosAnotativosProvider?.Invoke().ToList();
                var command = textoViewModel.CreateCommitCommand(_afterTypeLibraryChanged, textos);

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
