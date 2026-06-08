using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Araci.Core.Documents;

namespace Araci.Properties
{
    public partial class FiltrosTabelaWindow : Window
    {
        private readonly List<CampoFiltroItem> _campos;
        private readonly List<OperadorFiltroItem> _operadores;
        private readonly ComboBox[] _campoComboBoxes;
        private readonly ComboBox[] _operadorComboBoxes;
        private readonly TextBox[] _valorTextBoxes;

        public FiltrosTabelaWindow(
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados,
            ProjectTableFilterLogicalMode modo,
            IReadOnlyList<ProjectTableFilterRule> filtros)
        {
            InitializeComponent();

            _campos = camposSelecionados
                .OrderBy(c => c.Ordem)
                .Select(c => new CampoFiltroItem(c))
                .ToList();

            _operadores = CriarOperadores();
            _campoComboBoxes = new[] { Campo1ComboBox, Campo2ComboBox, Campo3ComboBox, Campo4ComboBox, Campo5ComboBox };
            _operadorComboBoxes = new[] { Operador1ComboBox, Operador2ComboBox, Operador3ComboBox, Operador4ComboBox, Operador5ComboBox };
            _valorTextBoxes = new[] { Valor1TextBox, Valor2TextBox, Valor3TextBox, Valor4TextBox, Valor5TextBox };

            AplicarModo(modo);
            ConfigurarLinhas();
            AplicarFiltros(filtros);
            AtualizarEstadoSemCampos();
        }

        public ProjectTableFilterLogicalMode ModoFiltro { get; private set; } = ProjectTableFilterLogicalMode.Todas;
        public IReadOnlyList<ProjectTableFilterRule> Filtros { get; private set; } = new List<ProjectTableFilterRule>();

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ModoFiltro = QualquerRadioButton.IsChecked == true
                ? ProjectTableFilterLogicalMode.Qualquer
                : ProjectTableFilterLogicalMode.Todas;

            Filtros = ObterFiltros();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void AplicarModo(ProjectTableFilterLogicalMode modo)
        {
            if (modo == ProjectTableFilterLogicalMode.Qualquer)
                QualquerRadioButton.IsChecked = true;
            else
                TodasRadioButton.IsChecked = true;
        }

        private void ConfigurarLinhas()
        {
            for (int i = 0; i < _campoComboBoxes.Length; i++)
            {
                _campoComboBoxes[i].ItemsSource = _campos;
                _operadorComboBoxes[i].ItemsSource = _operadores;
                _operadorComboBoxes[i].SelectedItem = _operadores.First();
            }
        }

        private void AplicarFiltros(IReadOnlyList<ProjectTableFilterRule> filtros)
        {
            List<ProjectTableFilterRule> filtrosValidos = filtros
                .OrderBy(f => f.Ordem)
                .Where(f => _campos.Any(c =>
                    c.Categoria == f.Categoria &&
                    string.Equals(c.CampoId, f.CampoId, StringComparison.Ordinal)))
                .Take(5)
                .ToList();

            for (int i = 0; i < filtrosValidos.Count; i++)
            {
                ProjectTableFilterRule filtro = filtrosValidos[i];
                _campoComboBoxes[i].SelectedItem = _campos.FirstOrDefault(c =>
                    c.Categoria == filtro.Categoria &&
                    string.Equals(c.CampoId, filtro.CampoId, StringComparison.Ordinal));
                _operadorComboBoxes[i].SelectedItem = _operadores.FirstOrDefault(o => o.Operador == filtro.Operador)
                    ?? _operadores.First();
                _valorTextBoxes[i].Text = filtro.Valor ?? string.Empty;
            }
        }

        private void AtualizarEstadoSemCampos()
        {
            bool semCampos = _campos.Count == 0;
            FiltrosGrid.IsEnabled = !semCampos;
            SemCamposTextBlock.Visibility = semCampos ? Visibility.Visible : Visibility.Collapsed;
        }

        private IReadOnlyList<ProjectTableFilterRule> ObterFiltros()
        {
            var filtros = new List<ProjectTableFilterRule>();

            for (int i = 0; i < _campoComboBoxes.Length; i++)
            {
                if (_campoComboBoxes[i].SelectedItem is not CampoFiltroItem campo)
                    continue;

                ProjectTableFilterOperator operador = _operadorComboBoxes[i].SelectedItem is OperadorFiltroItem operadorItem
                    ? operadorItem.Operador
                    : ProjectTableFilterOperator.Contem;

                filtros.Add(new ProjectTableFilterRule
                {
                    Ordem = filtros.Count,
                    Categoria = campo.Categoria,
                    CampoId = campo.CampoId,
                    NomeExibicao = campo.NomeExibicao,
                    Operador = operador,
                    Valor = _valorTextBoxes[i].Text?.Trim() ?? string.Empty
                });
            }

            return filtros;
        }

        private static List<OperadorFiltroItem> CriarOperadores()
        {
            return new List<OperadorFiltroItem>
            {
                new(ProjectTableFilterOperator.Contem, "contém"),
                new(ProjectTableFilterOperator.NaoContem, "não contém"),
                new(ProjectTableFilterOperator.ComecaCom, "começa com"),
                new(ProjectTableFilterOperator.TerminaCom, "termina com"),
                new(ProjectTableFilterOperator.IgualA, "igual a"),
                new(ProjectTableFilterOperator.DiferenteDe, "diferente de")
            };
        }

        private static string ObterRotuloCategoria(ProjectTableElementCategory categoria)
        {
            return categoria == ProjectTableElementCategory.Sin ? "SIN" : categoria.ToString();
        }

        private sealed class CampoFiltroItem
        {
            public CampoFiltroItem(ProjectTableFieldSelection campo)
            {
                Categoria = campo.Categoria;
                CampoId = campo.CampoId;
                NomeExibicao = campo.NomeExibicao;
                Texto = $"{ObterRotuloCategoria(campo.Categoria)} - {campo.NomeExibicao}";
            }

            public ProjectTableElementCategory Categoria { get; }
            public string CampoId { get; }
            public string NomeExibicao { get; }
            public string Texto { get; }
        }

        private sealed class OperadorFiltroItem
        {
            public OperadorFiltroItem(ProjectTableFilterOperator operador, string texto)
            {
                Operador = operador;
                Texto = texto;
            }

            public ProjectTableFilterOperator Operador { get; }
            public string Texto { get; }
        }
    }
}
