using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Araci.Core.Documents;

namespace Araci.Properties
{
    public partial class OrdenacaoTabelaWindow : Window
    {
        private readonly List<CampoOrdenacaoItem> _campos;
        private readonly List<DirecaoOrdenacaoItem> _direcoes;

        public OrdenacaoTabelaWindow(
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados,
            ProjectTableSorting? ordenacao)
        {
            InitializeComponent();

            _campos = CriarCampos(camposSelecionados);
            _direcoes = CriarDirecoes();

            CampoComboBox.ItemsSource = _campos;
            DirecaoComboBox.ItemsSource = _direcoes;
            DirecaoComboBox.SelectedItem = _direcoes.First();

            AplicarOrdenacao(ordenacao);
            AtualizarEstadoDirecao();
        }

        public ProjectTableSorting? Ordenacao { get; private set; }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (CampoComboBox.SelectedItem is CampoOrdenacaoItem { Campo: not null } campo)
            {
                ProjectTableSortDirection direcao = DirecaoComboBox.SelectedItem is DirecaoOrdenacaoItem direcaoItem
                    ? direcaoItem.Direcao
                    : ProjectTableSortDirection.Crescente;

                Ordenacao = new ProjectTableSorting
                {
                    Categoria = campo.Campo.Categoria,
                    CampoId = campo.Campo.CampoId,
                    NomeExibicao = campo.Campo.NomeExibicao,
                    Direcao = direcao
                };
            }
            else
            {
                Ordenacao = null;
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CampoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AtualizarEstadoDirecao();
        }

        private void AplicarOrdenacao(ProjectTableSorting? ordenacao)
        {
            CampoComboBox.SelectedItem = _campos.FirstOrDefault(c =>
                c.Campo != null &&
                ordenacao != null &&
                c.Campo.Categoria == ordenacao.Categoria &&
                string.Equals(c.Campo.CampoId, ordenacao.CampoId, StringComparison.Ordinal)) ?? _campos.First();

            DirecaoComboBox.SelectedItem = _direcoes.FirstOrDefault(d =>
                ordenacao != null &&
                d.Direcao == ordenacao.Direcao) ?? _direcoes.First();
        }

        private void AtualizarEstadoDirecao()
        {
            DirecaoComboBox.IsEnabled = CampoComboBox.SelectedItem is CampoOrdenacaoItem { Campo: not null };
        }

        private static List<CampoOrdenacaoItem> CriarCampos(IReadOnlyList<ProjectTableFieldSelection> camposSelecionados)
        {
            var itens = new List<CampoOrdenacaoItem>
            {
                new(null, "Sem ordenação")
            };

            itens.AddRange(camposSelecionados
                .OrderBy(c => c.Ordem)
                .Select(c => new CampoOrdenacaoItem(c, $"{ObterRotuloCategoria(c.Categoria)} - {c.NomeExibicao}")));

            return itens;
        }

        private static List<DirecaoOrdenacaoItem> CriarDirecoes()
        {
            return new List<DirecaoOrdenacaoItem>
            {
                new(ProjectTableSortDirection.Crescente, "Crescente"),
                new(ProjectTableSortDirection.Decrescente, "Decrescente")
            };
        }

        private static string ObterRotuloCategoria(ProjectTableElementCategory categoria)
        {
            return categoria == ProjectTableElementCategory.Sin ? "SIN" : categoria.ToString();
        }

        private sealed class CampoOrdenacaoItem
        {
            public CampoOrdenacaoItem(ProjectTableFieldSelection? campo, string texto)
            {
                Campo = campo;
                Texto = texto;
            }

            public ProjectTableFieldSelection? Campo { get; }
            public string Texto { get; }
        }

        private sealed class DirecaoOrdenacaoItem
        {
            public DirecaoOrdenacaoItem(ProjectTableSortDirection direcao, string texto)
            {
                Direcao = direcao;
                Texto = texto;
            }

            public ProjectTableSortDirection Direcao { get; }
            public string Texto { get; }
        }
    }
}
