using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Araci.Applications.Abstractions;

namespace Araci.Properties
{
    public partial class InserirTabelaPranchaWindow : Window
    {
        private readonly ObservableCollection<ProjectItemDialogOption> _tabelasDisponiveis = new();
        private readonly ObservableCollection<ProjectItemDialogOption> _tabelasSelecionadas = new();

        public InserirTabelaPranchaWindow(
            IReadOnlyList<ProjectItemDialogOption> pranchas,
            IReadOnlyList<ProjectItemDialogOption> tabelas)
        {
            InitializeComponent();

            PranchaComboBox.ItemsSource = pranchas ?? new List<ProjectItemDialogOption>();
            TabelasDisponiveisListBox.ItemsSource = _tabelasDisponiveis;
            TabelasSelecionadasListBox.ItemsSource = _tabelasSelecionadas;

            foreach (ProjectItemDialogOption tabela in tabelas ?? Array.Empty<ProjectItemDialogOption>())
            {
                if (!_tabelasDisponiveis.Any(t => t.Id == tabela.Id))
                    _tabelasDisponiveis.Add(tabela);
            }

            PranchaComboBox.SelectedItem = PranchaComboBox.Items.OfType<ProjectItemDialogOption>().FirstOrDefault();
        }

        public ProjectItemDialogOption? PranchaSelecionada => PranchaComboBox.SelectedItem as ProjectItemDialogOption;
        public IReadOnlyList<ProjectItemDialogOption> TabelasSelecionadas => _tabelasSelecionadas.ToList();

        public IReadOnlyList<Guid> TableIdsSelecionados => _tabelasSelecionadas
            .Select(t => t.Id)
            .ToList();

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = PranchaSelecionada != null && _tabelasSelecionadas.Count > 0;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void AdicionarButton_Click(object sender, RoutedEventArgs e)
        {
            MoverParaSelecionadas(TabelasDisponiveisListBox.SelectedItems
                .OfType<ProjectItemDialogOption>()
                .ToList());
        }

        private void RemoverButton_Click(object sender, RoutedEventArgs e)
        {
            MoverParaDisponiveis(TabelasSelecionadasListBox.SelectedItems
                .OfType<ProjectItemDialogOption>()
                .ToList());
        }

        private void AdicionarTodasButton_Click(object sender, RoutedEventArgs e)
        {
            MoverParaSelecionadas(_tabelasDisponiveis.ToList());
        }

        private void RemoverTodasButton_Click(object sender, RoutedEventArgs e)
        {
            MoverParaDisponiveis(_tabelasSelecionadas.ToList());
        }

        public void MoverParaSelecionadas(IReadOnlyList<ProjectItemDialogOption> tabelas)
        {
            foreach (ProjectItemDialogOption tabela in tabelas)
            {
                if (_tabelasSelecionadas.Any(t => t.Id == tabela.Id))
                    continue;

                _tabelasDisponiveis.Remove(tabela);
                _tabelasSelecionadas.Add(tabela);
            }
        }

        public void MoverParaDisponiveis(IReadOnlyList<ProjectItemDialogOption> tabelas)
        {
            foreach (ProjectItemDialogOption tabela in tabelas)
            {
                if (!_tabelasSelecionadas.Remove(tabela) || _tabelasDisponiveis.Any(t => t.Id == tabela.Id))
                    continue;

                _tabelasDisponiveis.Add(tabela);
            }
        }
    }
}
