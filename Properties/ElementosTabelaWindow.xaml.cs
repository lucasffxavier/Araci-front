using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Core.Documents;

namespace Araci.Properties
{
    public partial class ElementosTabelaWindow : Window
    {
        private readonly List<CampoDisponivelItem> _camposDisponiveis = new();
        private readonly List<CampoSelecionadoItem> _camposSelecionados = new();

        public ElementosTabelaWindow(
            IReadOnlyList<ProjectTableElementCategory> categoriasSelecionadas,
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados)
        {
            InitializeComponent();
            AplicarSelecaoInicial(categoriasSelecionadas);
            _camposSelecionados.AddRange(camposSelecionados
                .OrderBy(c => c.Ordem)
                .Select(c => new CampoSelecionadoItem(CopiarCampo(c))));
            RemoverCamposDeCategoriasDesmarcadas();
            AtualizarCamposDisponiveis();
            AtualizarCamposSelecionados();
            AtualizarCamposDisponiveis();
        }

        public IReadOnlyList<ProjectTableElementCategory> CategoriasSelecionadas { get; private set; } =
            new List<ProjectTableElementCategory>();

        public IReadOnlyList<ProjectTableFieldSelection> CamposSelecionados { get; private set; } =
            new List<ProjectTableFieldSelection>();

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            CategoriasSelecionadas = ObterCategoriasSelecionadas();
            CamposSelecionados = _camposSelecionados
                .Select((item, index) => new ProjectTableFieldSelection
                {
                    Categoria = item.Campo.Categoria,
                    CampoId = item.Campo.CampoId,
                    NomeExibicao = item.Campo.NomeExibicao,
                    Ordem = index
                })
                .ToList();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void AplicarSelecaoInicial(IReadOnlyList<ProjectTableElementCategory>? categoriasSelecionadas)
        {
            HashSet<ProjectTableElementCategory> selecionadas = categoriasSelecionadas?.ToHashSet()
                ?? new HashSet<ProjectTableElementCategory>();

            BarrasCheckBox.IsChecked = selecionadas.Contains(ProjectTableElementCategory.Barras);
            CabosCheckBox.IsChecked = selecionadas.Contains(ProjectTableElementCategory.Cabos);
            CargasCheckBox.IsChecked = selecionadas.Contains(ProjectTableElementCategory.Cargas);
            GeradoresCheckBox.IsChecked = selecionadas.Contains(ProjectTableElementCategory.Geradores);
            TransformadoresCheckBox.IsChecked = selecionadas.Contains(ProjectTableElementCategory.Transformadores);
            SinCheckBox.IsChecked = selecionadas.Contains(ProjectTableElementCategory.Sin);
        }

        private void OnCategoriaChanged(object sender, RoutedEventArgs e)
        {
            RemoverCamposDeCategoriasDesmarcadas();
            AtualizarCamposDisponiveis();
            AtualizarCamposSelecionados();
        }

        private void AdicionarCampo_Click(object sender, RoutedEventArgs e)
        {
            List<CampoDisponivelItem> itens = CamposDisponiveisListBox.SelectedItems
                .OfType<CampoDisponivelItem>()
                .ToList();

            if (itens.Count == 0)
                return;

            foreach (CampoDisponivelItem item in itens)
            {
                if (_camposSelecionados.Any(c => c.Campo.Categoria == item.Categoria && c.Campo.CampoId == item.CampoId))
                    continue;

                _camposSelecionados.Add(new CampoSelecionadoItem(new ProjectTableFieldSelection
                {
                    Categoria = item.Categoria,
                    CampoId = item.CampoId,
                    NomeExibicao = item.NomeExibicao,
                    Ordem = _camposSelecionados.Count
                }));
            }

            AtualizarCamposSelecionados();
            AtualizarCamposDisponiveis();
        }

        private void RemoverCampo_Click(object sender, RoutedEventArgs e)
        {
            List<CampoSelecionadoItem> itens = CamposSelecionadosListBox.SelectedItems
                .OfType<CampoSelecionadoItem>()
                .ToList();

            if (itens.Count == 0)
                return;

            int index = itens
                .Select(item => _camposSelecionados.IndexOf(item))
                .Where(i => i >= 0)
                .DefaultIfEmpty(-1)
                .Min();

            foreach (CampoSelecionadoItem item in itens)
                _camposSelecionados.Remove(item);

            AtualizarCamposSelecionados(Math.Min(index, _camposSelecionados.Count - 1));
            AtualizarCamposDisponiveis();
        }

        private void SubirCampo_Click(object sender, RoutedEventArgs e)
        {
            MoverCampoSelecionado(-1);
        }

        private void DescerCampo_Click(object sender, RoutedEventArgs e)
        {
            MoverCampoSelecionado(1);
        }

        private void MoverCampoSelecionado(int delta)
        {
            if (CamposSelecionadosListBox.SelectedItems.Count != 1)
                return;

            if (CamposSelecionadosListBox.SelectedItem is not CampoSelecionadoItem item)
                return;

            int index = _camposSelecionados.IndexOf(item);
            int novoIndex = index + delta;

            if (index < 0 || novoIndex < 0 || novoIndex >= _camposSelecionados.Count)
                return;

            _camposSelecionados.RemoveAt(index);
            _camposSelecionados.Insert(novoIndex, item);
            AtualizarCamposSelecionados(novoIndex);
        }

        private void AtualizarCamposDisponiveis()
        {
            _camposDisponiveis.Clear();

            HashSet<string> selecionados = _camposSelecionados
                .Select(c => CriarChaveCampo(c.Campo.Categoria, c.Campo.CampoId))
                .ToHashSet(StringComparer.Ordinal);

            foreach (ProjectTableElementCategory categoria in ObterCategoriasSelecionadas())
            {
                foreach (CampoDisponivelItem campo in ObterCamposDisponiveis(categoria))
                {
                    if (!selecionados.Contains(CriarChaveCampo(campo.Categoria, campo.CampoId)))
                        _camposDisponiveis.Add(campo);
                }
            }

            CamposDisponiveisListBox.ItemsSource = null;
            CamposDisponiveisListBox.ItemsSource = _camposDisponiveis;
        }

        private void AtualizarCamposSelecionados(int selectedIndex = -1)
        {
            for (int i = 0; i < _camposSelecionados.Count; i++)
                _camposSelecionados[i].Campo.Ordem = i;

            CamposSelecionadosListBox.ItemsSource = null;
            CamposSelecionadosListBox.ItemsSource = _camposSelecionados;

            if (selectedIndex >= 0 && selectedIndex < _camposSelecionados.Count)
                CamposSelecionadosListBox.SelectedIndex = selectedIndex;
        }

        private void RemoverCamposDeCategoriasDesmarcadas()
        {
            HashSet<ProjectTableElementCategory> categorias = ObterCategoriasSelecionadas().ToHashSet();
            _camposSelecionados.RemoveAll(c => !categorias.Contains(c.Campo.Categoria));
        }

        private IReadOnlyList<ProjectTableElementCategory> ObterCategoriasSelecionadas()
        {
            var selecionadas = new List<ProjectTableElementCategory>();

            if (BarrasCheckBox.IsChecked == true)
                selecionadas.Add(ProjectTableElementCategory.Barras);

            if (CabosCheckBox.IsChecked == true)
                selecionadas.Add(ProjectTableElementCategory.Cabos);

            if (CargasCheckBox.IsChecked == true)
                selecionadas.Add(ProjectTableElementCategory.Cargas);

            if (GeradoresCheckBox.IsChecked == true)
                selecionadas.Add(ProjectTableElementCategory.Geradores);

            if (TransformadoresCheckBox.IsChecked == true)
                selecionadas.Add(ProjectTableElementCategory.Transformadores);

            if (SinCheckBox.IsChecked == true)
                selecionadas.Add(ProjectTableElementCategory.Sin);

            return selecionadas;
        }

        private static ProjectTableFieldSelection CopiarCampo(ProjectTableFieldSelection campo)
        {
            return new ProjectTableFieldSelection
            {
                Categoria = campo.Categoria,
                CampoId = campo.CampoId,
                NomeExibicao = campo.NomeExibicao,
                Ordem = campo.Ordem
            };
        }

        private static string CriarChaveCampo(ProjectTableElementCategory categoria, string campoId)
        {
            return $"{categoria}|{campoId.Trim()}";
        }

        private static IReadOnlyList<CampoDisponivelItem> ObterCamposDisponiveis(ProjectTableElementCategory categoria)
        {
            return categoria switch
            {
                ProjectTableElementCategory.Barras => CriarCampos(categoria, ("Nome", "Nome"), ("Tipo", "Tipo"), ("TensaoNominal", "Tensão nominal"), ("Fases", "Fases")),
                ProjectTableElementCategory.Cabos => CriarCampos(categoria, ("Nome", "Nome"), ("Tipo", "Tipo"), ("Comprimento", "Comprimento"), ("Condutor", "Condutor"), ("Corrente", "Corrente"), ("BarraOrigem", "Barra origem"), ("BarraDestino", "Barra destino")),
                ProjectTableElementCategory.Cargas => CriarCampos(categoria, ("Nome", "Nome"), ("Tipo", "Tipo"), ("PotenciaAtiva", "Potência ativa"), ("PotenciaReativa", "Potência reativa"), ("Tensao", "Tensão")),
                ProjectTableElementCategory.Geradores => CriarCampos(categoria, ("Nome", "Nome"), ("Tipo", "Tipo"), ("PotenciaAtiva", "Potência ativa"), ("PotenciaReativa", "Potência reativa"), ("Tensao", "Tensão")),
                ProjectTableElementCategory.Transformadores => CriarCampos(categoria, ("Nome", "Nome"), ("Tipo", "Tipo"), ("PotenciaNominal", "Potência nominal"), ("TensaoPrimaria", "Tensão primária"), ("TensaoSecundaria", "Tensão secundária")),
                ProjectTableElementCategory.Sin => CriarCampos(categoria, ("Nome", "Nome"), ("Tipo", "Tipo"), ("Tensao", "Tensão"), ("PotenciaCurtoCircuito", "Potência de curto-circuito")),
                _ => Array.Empty<CampoDisponivelItem>()
            };
        }

        private static IReadOnlyList<CampoDisponivelItem> CriarCampos(ProjectTableElementCategory categoria, params (string id, string nome)[] campos)
        {
            return campos
                .Select(c => new CampoDisponivelItem(categoria, c.id, c.nome))
                .ToList();
        }

        private static string ObterRotuloCategoria(ProjectTableElementCategory categoria)
        {
            return categoria == ProjectTableElementCategory.Sin ? "SIN" : categoria.ToString();
        }

        private sealed class CampoDisponivelItem
        {
            public CampoDisponivelItem(ProjectTableElementCategory categoria, string campoId, string nomeExibicao)
            {
                Categoria = categoria;
                CampoId = campoId;
                NomeExibicao = nomeExibicao;
                Texto = $"{ObterRotuloCategoria(categoria)} - {nomeExibicao}";
            }

            public ProjectTableElementCategory Categoria { get; }
            public string CampoId { get; }
            public string NomeExibicao { get; }
            public string Texto { get; }
        }

        private sealed class CampoSelecionadoItem
        {
            public CampoSelecionadoItem(ProjectTableFieldSelection campo)
            {
                Campo = campo;
            }

            public ProjectTableFieldSelection Campo { get; }
            public string Texto => $"{Campo.Ordem + 1}. {ObterRotuloCategoria(Campo.Categoria)} - {Campo.NomeExibicao}";
        }
    }
}
