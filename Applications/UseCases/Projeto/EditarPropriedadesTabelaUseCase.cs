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

            if (categoriasAnteriores.SequenceEqual(categoriasNovas) &&
                CamposIguais(camposAnteriores, camposNovos) &&
                FiltrosIguais(filtrosAnteriores, filtrosNovos))
                return true;

            _commands.Execute(new UpdateProjectTableElementsCommand(
                _document,
                tabela,
                categoriasAnteriores,
                categoriasNovas,
                camposAnteriores,
                camposNovos,
                filtrosAnteriores,
                filtrosNovos));

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

        private static string CriarChaveCampo(ProjectTableElementCategory categoria, string campoId)
        {
            return $"{categoria}|{campoId.Trim()}";
        }
    }
}
