using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public class EditarPropriedadesTabelaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public EditarPropriedadesTabelaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool AlterarDisciplina(Guid id, ProjectViewDiscipline disciplina)
        {
            ProjectTable? tabela = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (tabela == null)
                return false;

            if (tabela.Disciplina == disciplina)
                return true;

            _commands.Execute(new UpdateProjectTablePropertyCommand<ProjectViewDiscipline>(
                _document,
                tabela,
                (t, valor) => t.Disciplina = valor,
                tabela.Disciplina,
                disciplina));

            return true;
        }

        public bool AlterarCategoriasElementos(Guid id, IReadOnlyList<ProjectTableElementCategory> categorias)
        {
            ProjectTable? tabela = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (tabela == null)
                return false;

            List<ProjectTableElementCategory> valorNovo = NormalizarCategorias(categorias);
            List<ProjectTableElementCategory> valorAnterior = NormalizarCategorias(tabela.CategoriasElementos);

            if (valorAnterior.SequenceEqual(valorNovo))
                return true;

            _commands.Execute(new UpdateProjectTablePropertyCommand<IReadOnlyList<ProjectTableElementCategory>>(
                _document,
                tabela,
                (t, valor) => t.CategoriasElementos = valor.ToList(),
                valorAnterior,
                valorNovo));

            return true;
        }

        public bool AlterarElementosTabela(
            Guid id,
            IReadOnlyList<ProjectTableElementCategory> categorias,
            IReadOnlyList<ProjectTableFieldSelection> campos)
        {
            ProjectTable? tabela = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (tabela == null)
                return false;

            List<ProjectTableElementCategory> categoriasNovas = NormalizarCategorias(categorias);
            List<ProjectTableElementCategory> categoriasAnteriores = NormalizarCategorias(tabela.CategoriasElementos);
            List<ProjectTableFieldSelection> camposNovos = NormalizarCampos(campos, categoriasNovas);
            List<ProjectTableFieldSelection> camposAnteriores = NormalizarCampos(tabela.CamposSelecionados, categoriasAnteriores);
            List<ProjectTableFilterRule> filtrosNovos = NormalizarFiltros(tabela.Filtros, camposNovos);
            List<ProjectTableFilterRule> filtrosAnteriores = NormalizarFiltros(tabela.Filtros, camposAnteriores);
            List<ProjectTableSorting> ordenacoesNovas = NormalizarOrdenacoes(tabela.Ordenacoes, camposNovos);
            List<ProjectTableSorting> ordenacoesAnteriores = NormalizarOrdenacoes(tabela.Ordenacoes, camposAnteriores);

            if (categoriasAnteriores.SequenceEqual(categoriasNovas) &&
                CamposIguais(camposAnteriores, camposNovos) &&
                FiltrosIguais(filtrosAnteriores, filtrosNovos) &&
                OrdenacoesIguais(ordenacoesAnteriores, ordenacoesNovas))
                return true;

            _commands.Execute(new UpdateProjectTableElementsCommand(
                _document,
                tabela,
                categoriasAnteriores,
                categoriasNovas,
                camposAnteriores,
                camposNovos,
                filtrosAnteriores,
                filtrosNovos,
                ordenacoesAnteriores,
                ordenacoesNovas));

            return true;
        }

        public bool AlterarOrdenacaoTabela(Guid id, IReadOnlyList<ProjectTableSorting> ordenacoes)
        {
            ProjectTable? tabela = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (tabela == null)
                return false;

            List<ProjectTableSorting> ordenacoesNovas = NormalizarOrdenacoes(ordenacoes, tabela.CamposSelecionados);
            List<ProjectTableSorting> ordenacoesAnteriores = NormalizarOrdenacoes(tabela.Ordenacoes, tabela.CamposSelecionados);

            if (OrdenacoesIguais(ordenacoesAnteriores, ordenacoesNovas))
                return true;

            _commands.Execute(new UpdateProjectTableSortingCommand(
                _document,
                tabela,
                ordenacoesAnteriores,
                ordenacoesNovas));

            return true;
        }

        public bool AlterarFiltrosTabela(
            Guid id,
            Guid? filtroVistaId,
            ProjectTableFilterLogicalMode modo,
            IReadOnlyList<ProjectTableFilterRule> filtros)
        {
            ProjectTable? tabela = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (tabela == null)
                return false;

            Guid? filtroVistaNovo = NormalizarFiltroVista(filtroVistaId);
            Guid? filtroVistaAnterior = NormalizarFiltroVista(tabela.FiltroVistaId);
            ProjectTableFilterLogicalMode modoNovo = NormalizarModoFiltro(modo);
            ProjectTableFilterLogicalMode modoAnterior = NormalizarModoFiltro(tabela.ModoFiltro);
            List<ProjectTableFilterRule> filtrosNovos = NormalizarFiltros(filtros, tabela.CamposSelecionados);
            List<ProjectTableFilterRule> filtrosAnteriores = NormalizarFiltros(tabela.Filtros, tabela.CamposSelecionados);

            if (filtroVistaAnterior == filtroVistaNovo &&
                modoAnterior == modoNovo &&
                FiltrosIguais(filtrosAnteriores, filtrosNovos))
                return true;

            _commands.Execute(new UpdateProjectTableFiltersCommand(
                _document,
                tabela,
                filtroVistaAnterior,
                filtroVistaNovo,
                modoAnterior,
                modoNovo,
                filtrosAnteriores,
                filtrosNovos));

            return true;
        }


        public bool AlterarExibicaoTabela(Guid id, ProjectTableDisplaySettings exibicao)
        {
            ProjectTable? tabela = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (tabela == null)
                return false;

            ProjectTableDisplaySettings valorNovo = NormalizarExibicao(exibicao);
            ProjectTableDisplaySettings valorAnterior = NormalizarExibicao(tabela.Exibicao);

            if (ExibicoesIguais(valorAnterior, valorNovo))
                return true;

            _commands.Execute(new UpdateProjectTablePropertyCommand<ProjectTableDisplaySettings>(
                _document,
                tabela,
                (t, valor) => t.Exibicao = NormalizarExibicao(valor),
                valorAnterior,
                valorNovo));

            return true;
        }

        private static List<ProjectTableElementCategory> NormalizarCategorias(IEnumerable<ProjectTableElementCategory>? categorias)
        {
            return (categorias ?? Enumerable.Empty<ProjectTableElementCategory>())
                .Distinct()
                .OrderBy(categoria => categoria)
                .ToList();
        }

        private static List<ProjectTableFieldSelection> NormalizarCampos(
            IEnumerable<ProjectTableFieldSelection>? campos,
            IReadOnlyList<ProjectTableElementCategory> categorias)
        {
            HashSet<ProjectTableElementCategory> categoriasPermitidas = categorias.ToHashSet();

            return (campos ?? Enumerable.Empty<ProjectTableFieldSelection>())
                .Where(c => categoriasPermitidas.Contains(c.Categoria))
                .Where(c => !string.IsNullOrWhiteSpace(c.CampoId))
                .OrderBy(c => c.Ordem)
                .GroupBy(c => new { c.Categoria, CampoId = c.CampoId.Trim() })
                .Select((g, index) =>
                {
                    ProjectTableFieldSelection campo = g.First();
                    return new ProjectTableFieldSelection
                    {
                        Categoria = campo.Categoria,
                        CampoId = campo.CampoId.Trim(),
                        NomeExibicao = string.IsNullOrWhiteSpace(campo.NomeExibicao) ? campo.CampoId.Trim() : campo.NomeExibicao.Trim(),
                        Ordem = index
                    };
                })
                .ToList();
        }

        private static bool CamposIguais(IReadOnlyList<ProjectTableFieldSelection> a, IReadOnlyList<ProjectTableFieldSelection> b)
        {
            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].Categoria != b[i].Categoria ||
                    !string.Equals(a[i].CampoId, b[i].CampoId, StringComparison.Ordinal) ||
                    !string.Equals(a[i].NomeExibicao, b[i].NomeExibicao, StringComparison.Ordinal) ||
                    a[i].Ordem != b[i].Ordem)
                    return false;
            }

            return true;
        }

        private static ProjectTableFilterLogicalMode NormalizarModoFiltro(ProjectTableFilterLogicalMode modo)
        {
            return Enum.IsDefined(typeof(ProjectTableFilterLogicalMode), modo)
                ? modo
                : ProjectTableFilterLogicalMode.Todas;
        }

        private Guid? NormalizarFiltroVista(Guid? filtroVistaId)
        {
            if (!filtroVistaId.HasValue || filtroVistaId.Value == Guid.Empty)
                return null;

            return _document.Vistas.Any(v => v.Id == filtroVistaId.Value)
                ? filtroVistaId
                : null;
        }

        private static List<ProjectTableFilterRule> NormalizarFiltros(
            IEnumerable<ProjectTableFilterRule>? filtros,
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados)
        {
            Dictionary<string, ProjectTableFieldSelection> camposPermitidos = camposSelecionados
                .GroupBy(c => CriarChaveCampo(c.Categoria, c.CampoId))
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            return (filtros ?? Enumerable.Empty<ProjectTableFilterRule>())
                .Where(f => !string.IsNullOrWhiteSpace(f.CampoId))
                .OrderBy(f => f.Ordem)
                .Select(f => new { Filtro = f, Chave = CriarChaveCampo(f.Categoria, f.CampoId) })
                .Where(item => camposPermitidos.ContainsKey(item.Chave))
                .Take(5)
                .Select((item, index) =>
                {
                    ProjectTableFieldSelection campo = camposPermitidos[item.Chave];
                    return new ProjectTableFilterRule
                    {
                        Ordem = index,
                        Categoria = campo.Categoria,
                        CampoId = campo.CampoId,
                        NomeExibicao = campo.NomeExibicao,
                        Operador = Enum.IsDefined(typeof(ProjectTableFilterOperator), item.Filtro.Operador)
                            ? item.Filtro.Operador
                            : ProjectTableFilterOperator.Contem,
                        Valor = item.Filtro.Valor?.Trim() ?? string.Empty
                    };
                })
                .ToList();
        }

        private static bool FiltrosIguais(IReadOnlyList<ProjectTableFilterRule> a, IReadOnlyList<ProjectTableFilterRule> b)
        {
            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].Ordem != b[i].Ordem ||
                    a[i].Categoria != b[i].Categoria ||
                    !string.Equals(a[i].CampoId, b[i].CampoId, StringComparison.Ordinal) ||
                    !string.Equals(a[i].NomeExibicao, b[i].NomeExibicao, StringComparison.Ordinal) ||
                    a[i].Operador != b[i].Operador ||
                    !string.Equals(a[i].Valor, b[i].Valor, StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        private static List<ProjectTableSorting> NormalizarOrdenacoes(
            IEnumerable<ProjectTableSorting>? ordenacoes,
            IReadOnlyList<ProjectTableFieldSelection> camposSelecionados)
        {
            Dictionary<string, ProjectTableFieldSelection> camposPermitidos = camposSelecionados
                .GroupBy(c => CriarChaveCampo(c.Categoria, c.CampoId))
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            var chavesUsadas = new HashSet<string>(StringComparer.Ordinal);
            var resultado = new List<ProjectTableSorting>();

            foreach (ProjectTableSorting ordenacao in (ordenacoes ?? Enumerable.Empty<ProjectTableSorting>())
                .Where(o => !string.IsNullOrWhiteSpace(o.CampoId))
                .OrderBy(o => o.Ordem))
            {
                string chave = CriarChaveCampo(ordenacao.Categoria, ordenacao.CampoId);

                if (!camposPermitidos.TryGetValue(chave, out ProjectTableFieldSelection? campo) ||
                    !chavesUsadas.Add(chave))
                    continue;

                resultado.Add(new ProjectTableSorting
                {
                    Ordem = resultado.Count,
                    Categoria = campo.Categoria,
                    CampoId = campo.CampoId,
                    NomeExibicao = campo.NomeExibicao,
                    Direcao = Enum.IsDefined(typeof(ProjectTableSortDirection), ordenacao.Direcao)
                        ? ordenacao.Direcao
                        : ProjectTableSortDirection.Crescente
                });

                if (resultado.Count == 5)
                    break;
            }

            return resultado;
        }

        private static bool OrdenacoesIguais(IReadOnlyList<ProjectTableSorting> a, IReadOnlyList<ProjectTableSorting> b)
        {
            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].Ordem != b[i].Ordem ||
                    a[i].Categoria != b[i].Categoria ||
                    !string.Equals(a[i].CampoId, b[i].CampoId, StringComparison.Ordinal) ||
                    !string.Equals(a[i].NomeExibicao, b[i].NomeExibicao, StringComparison.Ordinal) ||
                    a[i].Direcao != b[i].Direcao)
                    return false;
            }

            return true;
        }


        private static ProjectTableDisplaySettings NormalizarExibicao(ProjectTableDisplaySettings? valor)
        {
            ProjectTableDisplaySettings origem = valor ?? new ProjectTableDisplaySettings();

            return new ProjectTableDisplaySettings
            {
                ExibirTitulo = origem.ExibirTitulo,
                FonteTitulo = NormalizarFonte(origem.FonteTitulo, ProjectTableDisplaySettings.DefaultFontFamily),
                TamanhoFonteTitulo = NormalizarIntervalo(origem.TamanhoFonteTitulo, 11.0, ProjectTableDisplaySettings.MinFontSize, ProjectTableDisplaySettings.MaxFontSize),
                TituloNegrito = origem.TituloNegrito,
                CorTextoTitulo = NormalizarCor(origem.CorTextoTitulo, ProjectTableDisplaySettings.DefaultTitleTextColor),
                CorFundoTitulo = NormalizarCor(origem.CorFundoTitulo, ProjectTableDisplaySettings.DefaultTitleBackgroundColor),
                AlturaTitulo = NormalizarIntervalo(origem.AlturaTitulo, 32.0, ProjectTableDisplaySettings.MinRowHeight, ProjectTableDisplaySettings.MaxRowHeight),
                AlinhamentoTitulo = Enum.IsDefined(typeof(ProjectTableTextAlignment), origem.AlinhamentoTitulo) ? origem.AlinhamentoTitulo : ProjectTableTextAlignment.Esquerda,
                ExibirCabecalho = origem.ExibirCabecalho,
                FonteCabecalho = NormalizarFonte(origem.FonteCabecalho, ProjectTableDisplaySettings.DefaultFontFamily),
                TamanhoFonteCabecalho = NormalizarIntervalo(origem.TamanhoFonteCabecalho, 10.0, ProjectTableDisplaySettings.MinFontSize, ProjectTableDisplaySettings.MaxFontSize),
                CabecalhoNegrito = origem.CabecalhoNegrito,
                CorTextoCabecalho = NormalizarCor(origem.CorTextoCabecalho, ProjectTableDisplaySettings.DefaultHeaderTextColor),
                CorFundoCabecalho = NormalizarCor(origem.CorFundoCabecalho, ProjectTableDisplaySettings.DefaultHeaderBackgroundColor),
                AlturaCabecalho = NormalizarIntervalo(origem.AlturaCabecalho, 26.0, ProjectTableDisplaySettings.MinRowHeight, ProjectTableDisplaySettings.MaxRowHeight),
                AlinhamentoCabecalho = Enum.IsDefined(typeof(ProjectTableTextAlignment), origem.AlinhamentoCabecalho) ? origem.AlinhamentoCabecalho : ProjectTableTextAlignment.Esquerda,
                FonteCorpo = NormalizarFonte(origem.FonteCorpo, ProjectTableDisplaySettings.DefaultFontFamily),
                TamanhoFonteCorpo = NormalizarIntervalo(origem.TamanhoFonteCorpo, 10.5, ProjectTableDisplaySettings.MinFontSize, ProjectTableDisplaySettings.MaxFontSize),
                CorTextoCorpo = NormalizarCor(origem.CorTextoCorpo, ProjectTableDisplaySettings.DefaultBodyTextColor),
                CorFundoCorpo = NormalizarCor(origem.CorFundoCorpo, ProjectTableDisplaySettings.DefaultBodyBackgroundColor),
                AlturaLinhaCorpo = NormalizarIntervalo(origem.AlturaLinhaCorpo, 24.0, ProjectTableDisplaySettings.MinRowHeight, ProjectTableDisplaySettings.MaxRowHeight),
                AlinhamentoCorpo = Enum.IsDefined(typeof(ProjectTableTextAlignment), origem.AlinhamentoCorpo) ? origem.AlinhamentoCorpo : ProjectTableTextAlignment.Esquerda,
                UsarLinhasAlternadas = origem.UsarLinhasAlternadas,
                CorLinhaAlternada = NormalizarCor(origem.CorLinhaAlternada, ProjectTableDisplaySettings.DefaultAlternateRowBackgroundColor),
                ExibirLinhasGrade = origem.ExibirLinhasGrade,
                CorGrade = NormalizarCor(origem.CorGrade, ProjectTableDisplaySettings.DefaultGridColor),
                EspessuraGrade = NormalizarIntervalo(origem.EspessuraGrade, 1.0, ProjectTableDisplaySettings.MinThickness, ProjectTableDisplaySettings.MaxThickness),
                ExibirContornoExterno = origem.ExibirContornoExterno,
                CorContorno = NormalizarCor(origem.CorContorno, ProjectTableDisplaySettings.DefaultOutlineColor),
                EspessuraContorno = NormalizarIntervalo(origem.EspessuraContorno, 1.0, ProjectTableDisplaySettings.MinThickness, ProjectTableDisplaySettings.MaxThickness)
            };
        }

        private static bool ExibicoesIguais(ProjectTableDisplaySettings a, ProjectTableDisplaySettings b)
        {
            return a.ExibirTitulo == b.ExibirTitulo &&
                string.Equals(a.FonteTitulo, b.FonteTitulo, StringComparison.Ordinal) &&
                ValoresIguais(a.TamanhoFonteTitulo, b.TamanhoFonteTitulo) &&
                a.TituloNegrito == b.TituloNegrito &&
                string.Equals(a.CorTextoTitulo, b.CorTextoTitulo, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(a.CorFundoTitulo, b.CorFundoTitulo, StringComparison.OrdinalIgnoreCase) &&
                ValoresIguais(a.AlturaTitulo, b.AlturaTitulo) &&
                a.AlinhamentoTitulo == b.AlinhamentoTitulo &&
                a.ExibirCabecalho == b.ExibirCabecalho &&
                string.Equals(a.FonteCabecalho, b.FonteCabecalho, StringComparison.Ordinal) &&
                ValoresIguais(a.TamanhoFonteCabecalho, b.TamanhoFonteCabecalho) &&
                a.CabecalhoNegrito == b.CabecalhoNegrito &&
                string.Equals(a.CorTextoCabecalho, b.CorTextoCabecalho, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(a.CorFundoCabecalho, b.CorFundoCabecalho, StringComparison.OrdinalIgnoreCase) &&
                ValoresIguais(a.AlturaCabecalho, b.AlturaCabecalho) &&
                a.AlinhamentoCabecalho == b.AlinhamentoCabecalho &&
                string.Equals(a.FonteCorpo, b.FonteCorpo, StringComparison.Ordinal) &&
                ValoresIguais(a.TamanhoFonteCorpo, b.TamanhoFonteCorpo) &&
                string.Equals(a.CorTextoCorpo, b.CorTextoCorpo, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(a.CorFundoCorpo, b.CorFundoCorpo, StringComparison.OrdinalIgnoreCase) &&
                ValoresIguais(a.AlturaLinhaCorpo, b.AlturaLinhaCorpo) &&
                a.AlinhamentoCorpo == b.AlinhamentoCorpo &&
                a.UsarLinhasAlternadas == b.UsarLinhasAlternadas &&
                string.Equals(a.CorLinhaAlternada, b.CorLinhaAlternada, StringComparison.OrdinalIgnoreCase) &&
                a.ExibirLinhasGrade == b.ExibirLinhasGrade &&
                string.Equals(a.CorGrade, b.CorGrade, StringComparison.OrdinalIgnoreCase) &&
                ValoresIguais(a.EspessuraGrade, b.EspessuraGrade) &&
                a.ExibirContornoExterno == b.ExibirContornoExterno &&
                string.Equals(a.CorContorno, b.CorContorno, StringComparison.OrdinalIgnoreCase) &&
                ValoresIguais(a.EspessuraContorno, b.EspessuraContorno);
        }

        private static bool ValoresIguais(double a, double b)
        {
            return Math.Abs(a - b) < 0.000001;
        }

        private static string NormalizarFonte(string? valor, string fallback)
        {
            return string.IsNullOrWhiteSpace(valor) ? fallback : valor.Trim();
        }

        private static string NormalizarCor(string? valor, string fallback)
        {
            string cor = string.IsNullOrWhiteSpace(valor) ? fallback : valor.Trim();
            return cor.StartsWith("#", StringComparison.Ordinal) ? cor : fallback;
        }

        private static double NormalizarIntervalo(double valor, double fallback, double minimo, double maximo)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor))
                return fallback;

            if (valor < minimo)
                return minimo;

            if (valor > maximo)
                return maximo;

            return valor;
        }

        private static string CriarChaveCampo(ProjectTableElementCategory categoria, string campoId)
        {
            return $"{categoria}|{campoId.Trim()}";
        }
    }
}