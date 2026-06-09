using Araci.Applications.Abstractions;
using Araci.Core.Documents;
using Araci.Properties;
using Araci.Services.Simulation;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;

namespace Araci.Services.UI
{
    public class DialogService : IUserDialogService
    {
        public void ShowInfo(string title, string message)
        {
            Show(title, message, MessageBoxImage.Information);
        }

        public void ShowWarning(string title, string message)
        {
            Show(title, message, MessageBoxImage.Warning);
        }

        public void ShowError(string title, string message)
        {
            Show(title, message, MessageBoxImage.Error);
        }

        public string? ShowSaveCsvDialog(string suggestedFileName)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Exportar CSV",
                Filter = "Arquivos CSV (*.csv)|*.csv",
                DefaultExt = ".csv",
                AddExtension = true,
                FileName = string.IsNullOrWhiteSpace(suggestedFileName) ? "Tabela.csv" : suggestedFileName
            };

            return dialog.ShowDialog(Application.Current?.MainWindow) == true
                ? dialog.FileName
                : null;
        }

        public ElementosTabelaDialogResult? ShowElementosTabelaDialog(
            IReadOnlyList<ProjectTableElementCategory> categorias,
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados)
        {
            var window = new ElementosTabelaWindow(categorias, camposSelecionados)
            {
                Owner = Application.Current?.MainWindow
            };

            return window.ShowDialog() == true
                ? new ElementosTabelaDialogResult(window.CategoriasSelecionadas, window.CamposSelecionados)
                : null;
        }

        public FiltrosTabelaDialogResult? ShowFiltrosTabelaDialog(
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados,
            IReadOnlyList<ProjectViewDialogOption> vistasDisponiveis,
            Guid? filtroVistaId,
            ProjectTableFilterLogicalMode modo,
            IReadOnlyList<ProjectTableFilterRule> filtros)
        {
            var window = new FiltrosTabelaWindow(camposSelecionados, vistasDisponiveis, filtroVistaId, modo, filtros)
            {
                Owner = Application.Current?.MainWindow
            };

            return window.ShowDialog() == true
                ? new FiltrosTabelaDialogResult(window.FiltroVistaId, window.ModoFiltro, window.Filtros)
                : null;
        }

        public OrdenacaoTabelaDialogResult? ShowOrdenacaoTabelaDialog(
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados,
            IReadOnlyList<ProjectTableSorting> ordenacoes)
        {
            var window = new OrdenacaoTabelaWindow(camposSelecionados, ordenacoes)
            {
                Owner = Application.Current?.MainWindow
            };

            return window.ShowDialog() == true
                ? new OrdenacaoTabelaDialogResult(window.Ordenacoes)
                : null;
        }

        public bool Confirm(string title, string message)
        {
            MessageBoxResult result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        public void Show(SimulationMessage message)
        {
            if (message == null)
                return;

            MessageBox.Show(message.Text, message.Title, MessageBoxButton.OK, message.Icon);
        }

        private static void Show(string title, string message, MessageBoxImage icon)
        {
            MessageBox.Show(
                message ?? string.Empty,
                string.IsNullOrWhiteSpace(title) ? "Araci" : title,
                MessageBoxButton.OK,
                icon);
        }
    }
}
