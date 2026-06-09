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
        private readonly List<ComboBox> _campoComboBoxes;
        private readonly List<ComboBox> _direcaoComboBoxes;

        public OrdenacaoTabelaWindow(
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados,
            IReadOnlyList<ProjectTableSorting> ordenacoes)
        {
            InitializeComponent();

            _campos = CriarCampos(camposSelecionados);
            _direcoes = CriarDirecoes();
            _campoComboBoxes = new List<ComboBox>
            {
                Campo0ComboBox,
                Campo1ComboBox,
                Campo2ComboBox,
                Campo3ComboBox,
                Campo4ComboBox
            };
            _direcaoComboBoxes = new List<ComboBox>
            {
                Direcao0ComboBox,
                Direcao1ComboBox,
                Direcao2ComboBox,
                Direcao3ComboBox,
                Direcao4ComboBox
            };

            for (int i = 0; i < _campoComboBoxes.Count; i++)
            {
                _campoComboBoxes[i].ItemsSource = _campos;
                _campoComboBoxes[i].SelectedItem = _campos.First();
                _direcaoComboBoxes[i].ItemsSource = _direcoes;
                _direcaoComboBoxes[i].SelectedItem = _direcoes.First();
            }

            AplicarOrdenacoes(ordenacoes);
            AtualizarEstadoDirecoes();
        }

        public IReadOnlyList<ProjectTableSorting> Ordenacoes { get; private set; } = new List<ProjectTableSorting>();

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var resultado = new List<ProjectTableSorting>();
            var chavesUsadas = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < _campoComboBoxes.Count; i++)
            {
                if (_campoComboBoxes[i].SelectedItem is not CampoOrdenacaoItem { Campo: not null } campo)
                    continue;

                string chave = CriarChaveCampo(campo.Campo.Categoria, campo.Campo.CampoId);

                if (!chavesUsadas.Add(chave))
                    continue;

                ProjectTableSortDirection direcao = _direcaoComboBoxes[i].SelectedItem is DirecaoOrdenacaoItem direcaoItem
                    ? direcaoItem.Direcao
                    : ProjectTableSortDirection.Crescente;

                resultado.Add(new ProjectTableSorting
                {
                    Ordem = resultado.Count,
                    Categoria = campo.Campo.Categoria,
                    CampoId = campo.Campo.CampoId,
                    NomeExibicao = campo.Campo.NomeExibicao,
                    Direcao = direcao
                });
            }

            Ordenacoes = resultado;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CampoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AtualizarEstadoDirecoes();
        }

        private void AplicarOrdenacoes(IReadOnlyList<ProjectTableSorting> ordenacoes)
        {
            foreach (ProjectTableSorting ordenacao in (ordenacoes ?? Array.Empty<ProjectTableSorting>())
                .Where(o => !string.IsNullOrWhiteSpace(o.CampoId))
                .OrderBy(o => o.Ordem)
                .Take(5))
            {
                int indice = Math.Max(0, Math.Min(4, ordenacao.Ordem));

                _campoComboBoxes[indice].SelectedItem = _campos.FirstOrDefault(c =>
                    c.Campo != null &&
                    c.Campo.Categoria == ordenacao.Categoria &&
                    string.Equals(c.Campo.CampoId, ordenacao.CampoId, StringComparison.Ordinal)) ?? _campos.First();

                _direcaoComboBoxes[indice].SelectedItem = _direcoes.FirstOrDefault(d =>
                    d.Direcao == ordenacao.Direcao) ?? _direcoes.First();
            }
        }

        private void AtualizarEstadoDirecoes()
        {
            for (int i = 0; i < _campoComboBoxes.Count; i++)
                _direcaoComboBoxes[i].IsEnabled = _campoComboBoxes[i].SelectedItem is CampoOrdenacaoItem { Campo: not null };
        }

        private static List<CampoOrdenacaoItem> CriarCampos(IReadOnlyList<ProjectTableFieldSelection> camposSelecionados)
        {
            var itens = new List<CampoOrdenacaoItem>
            {
                new(null, "Sem ordenação")
            };

            itens.AddRange((camposSelecionados ?? Array.Empty<ProjectTableFieldSelection>())
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

        private static string CriarChaveCampo(ProjectTableElementCategory categoria, string campoId)
        {
            return $"{categoria}|{campoId.Trim()}";
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
