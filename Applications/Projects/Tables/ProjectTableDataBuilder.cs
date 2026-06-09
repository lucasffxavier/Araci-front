using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Araci.Core.Documents;
using Araci.Models;

namespace Araci.Applications.Projects.Tables
{
    public sealed class ProjectTableDataBuilder
    {
        public ProjectTableDataResult Build(AraciDocument document, ProjectTable table)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(table);

            List<ProjectTableDataColumn> columns = CriarColunas(table);

            if (columns.Count == 0 || table.CategoriasElementos.Count == 0)
                return new ProjectTableDataResult(columns, new List<ProjectTableDataRow>());

            HashSet<ProjectTableElementCategory> categorias = table.CategoriasElementos.ToHashSet();
            Guid? filtroVistaId = ObterFiltroVistaValido(document, table.FiltroVistaId);

            List<RowBuildData> rows = document.Elementos
                .Select((elemento, index) => new { Elemento = elemento, Index = index, Categoria = ObterCategoria(elemento) })
                .Where(item => item.Categoria.HasValue && categorias.Contains(item.Categoria.Value))
                .Where(item => PassaFiltroVista(item.Elemento, filtroVistaId))
                .Select(item => CriarLinha(item.Elemento, item.Categoria!.Value, item.Index, columns))
                .Where(row => PassaFiltros(row, table))
                .ToList();

            rows = OrdenarLinhas(rows, table, columns);

            return new ProjectTableDataResult(
                columns,
                rows.Select(row => row.Row).ToList());
        }

        private static List<ProjectTableDataColumn> CriarColunas(ProjectTable table)
        {
            return (table.CamposSelecionados ?? new List<ProjectTableFieldSelection>())
                .Where(c => !string.IsNullOrWhiteSpace(c.CampoId))
                .OrderBy(c => c.Ordem)
                .GroupBy(c => CriarChaveCampo(c.Categoria, c.CampoId))
                .Select((g, index) =>
                {
                    ProjectTableFieldSelection campo = g.First();
                    string campoId = campo.CampoId.Trim();
                    return new ProjectTableDataColumn(
                        campo.Categoria,
                        campoId,
                        string.IsNullOrWhiteSpace(campo.NomeExibicao) ? campoId : campo.NomeExibicao.Trim(),
                        index);
                })
                .ToList();
        }

        private static Guid? ObterFiltroVistaValido(AraciDocument document, Guid? filtroVistaId)
        {
            if (!filtroVistaId.HasValue || filtroVistaId.Value == Guid.Empty)
                return null;

            return document.Vistas.Any(v => v.Id == filtroVistaId.Value)
                ? filtroVistaId
                : null;
        }

        private static bool PassaFiltroVista(Elemento elemento, Guid? filtroVistaId)
        {
            return !filtroVistaId.HasValue || elemento.ViewId == filtroVistaId.Value;
        }

        private static RowBuildData CriarLinha(
            Elemento elemento,
            ProjectTableElementCategory categoria,
            int documentIndex,
            IReadOnlyList<ProjectTableDataColumn> columns)
        {
            List<ProjectTableDataCell> cells = columns
                .Select(column =>
                {
                    object? valor = ExtrairValor(elemento, categoria, column);
                    return new ProjectTableDataCell(
                        column.Categoria,
                        column.CampoId,
                        column.NomeExibicao,
                        valor,
                        FormatarValor(valor));
                })
                .ToList();

            var row = new ProjectTableDataRow(
                elemento.Id,
                elemento.Nome,
                categoria,
                cells);

            return new RowBuildData(elemento, documentIndex, row);
        }

        private static bool PassaFiltros(RowBuildData row, ProjectTable table)
        {
            List<ProjectTableFilterRule> filtros = NormalizarFiltros(table.Filtros, row.Row.Cells);

            if (filtros.Count == 0)
                return true;

            return table.ModoFiltro == ProjectTableFilterLogicalMode.Qualquer
                ? filtros.Any(f => AvaliarFiltro(row, f))
                : filtros.All(f => AvaliarFiltro(row, f));
        }

        private static List<ProjectTableFilterRule> NormalizarFiltros(
            IEnumerable<ProjectTableFilterRule>? filtros,
            IReadOnlyList<ProjectTableDataCell> cells)
        {
            HashSet<string> campos = cells
                .Select(c => CriarChaveCampo(c.Categoria, c.CampoId))
                .ToHashSet(StringComparer.Ordinal);

            return (filtros ?? Enumerable.Empty<ProjectTableFilterRule>())
                .Where(f => !string.IsNullOrWhiteSpace(f.CampoId))
                .OrderBy(f => f.Ordem)
                .Where(f => campos.Contains(CriarChaveCampo(f.Categoria, f.CampoId)))
                .Take(5)
                .ToList();
        }

        private static bool AvaliarFiltro(RowBuildData row, ProjectTableFilterRule filtro)
        {
            ProjectTableDataCell? cell = ObterCell(row, filtro.Categoria, filtro.CampoId);

            if (cell == null)
                return false;

            string valorFiltro = filtro.Valor?.Trim() ?? string.Empty;
            string display = cell.DisplayValue ?? string.Empty;

            if (filtro.Operador is ProjectTableFilterOperator.IgualA or ProjectTableFilterOperator.DiferenteDe &&
                TentarConverterNumero(cell.RawValue, out double numeroCelula) &&
                TentarConverterNumero(valorFiltro, out double numeroFiltro))
            {
                bool igualNumero = Math.Abs(numeroCelula - numeroFiltro) < 0.0000001;
                return filtro.Operador == ProjectTableFilterOperator.IgualA ? igualNumero : !igualNumero;
            }

            int comparacao = string.Compare(display, valorFiltro, StringComparison.OrdinalIgnoreCase);
            bool resultado = filtro.Operador switch
            {
                ProjectTableFilterOperator.Contem => display.Contains(valorFiltro, StringComparison.OrdinalIgnoreCase),
                ProjectTableFilterOperator.NaoContem => !display.Contains(valorFiltro, StringComparison.OrdinalIgnoreCase),
                ProjectTableFilterOperator.ComecaCom => display.StartsWith(valorFiltro, StringComparison.OrdinalIgnoreCase),
                ProjectTableFilterOperator.TerminaCom => display.EndsWith(valorFiltro, StringComparison.OrdinalIgnoreCase),
                ProjectTableFilterOperator.IgualA => comparacao == 0,
                ProjectTableFilterOperator.DiferenteDe => comparacao != 0,
                _ => display.Contains(valorFiltro, StringComparison.OrdinalIgnoreCase)
            };

            return resultado;
        }

        private static List<RowBuildData> OrdenarLinhas(
            List<RowBuildData> rows,
            ProjectTable table,
            IReadOnlyList<ProjectTableDataColumn> columns)
        {
            List<ProjectTableSorting> ordenacoes = NormalizarOrdenacoes(table.Ordenacoes, columns);

            if (ordenacoes.Count == 0)
                return rows.OrderBy(row => row.DocumentIndex).ToList();

            return rows
                .OrderBy(row => row, new RowBuildDataComparer(ordenacoes))
                .ToList();
        }

        private static List<ProjectTableSorting> NormalizarOrdenacoes(
            IEnumerable<ProjectTableSorting>? ordenacoes,
            IReadOnlyList<ProjectTableDataColumn> columns)
        {
            HashSet<string> campos = columns
                .Select(c => CriarChaveCampo(c.Categoria, c.CampoId))
                .ToHashSet(StringComparer.Ordinal);

            var chavesUsadas = new HashSet<string>(StringComparer.Ordinal);
            var resultado = new List<ProjectTableSorting>();

            foreach (ProjectTableSorting ordenacao in (ordenacoes ?? Enumerable.Empty<ProjectTableSorting>())
                .Where(o => !string.IsNullOrWhiteSpace(o.CampoId))
                .OrderBy(o => o.Ordem))
            {
                string chave = CriarChaveCampo(ordenacao.Categoria, ordenacao.CampoId);

                if (!campos.Contains(chave) || !chavesUsadas.Add(chave))
                    continue;

                resultado.Add(new ProjectTableSorting
                {
                    Ordem = resultado.Count,
                    Categoria = ordenacao.Categoria,
                    CampoId = ordenacao.CampoId.Trim(),
                    NomeExibicao = ordenacao.NomeExibicao,
                    Direcao = Enum.IsDefined(typeof(ProjectTableSortDirection), ordenacao.Direcao)
                        ? ordenacao.Direcao
                        : ProjectTableSortDirection.Crescente
                });

                if (resultado.Count == 5)
                    break;
            }

            return resultado;
        }

        private static object? ExtrairValor(
            Elemento elemento,
            ProjectTableElementCategory categoriaElemento,
            ProjectTableDataColumn column)
        {
            if (categoriaElemento != column.Categoria)
                return null;

            string campoId = column.CampoId.Trim();

            if (string.Equals(campoId, Elemento.PARAM_NOME, StringComparison.Ordinal))
                return elemento.Nome;

            if (string.Equals(campoId, "Tipo", StringComparison.Ordinal))
                return elemento.Tipo?.NomeTipo ?? string.Empty;

            string? parametro = ObterParametroAlias(categoriaElemento, campoId);

            if (parametro != null && elemento.Parametros.TryGetValue(parametro, out Parameter? aliasParameter))
                return aliasParameter.ValorObjeto;

            return elemento.Parametros.TryGetValue(campoId, out Parameter? parameter)
                ? parameter.ValorObjeto
                : null;
        }

        private static string? ObterParametroAlias(ProjectTableElementCategory categoria, string campoId)
        {
            return categoria switch
            {
                ProjectTableElementCategory.Barras when campoId == "TensaoNominal" => Barra.PARAM_TENSAO,
                ProjectTableElementCategory.Cabos when campoId == "Corrente" => Cabo.PARAM_CORRENTE_LINHA,
                ProjectTableElementCategory.Cargas when campoId == "Tensao" => ElementoEquipamento.PARAM_TENSAO_LINHA,
                ProjectTableElementCategory.Geradores when campoId == "Tensao" => ElementoEquipamento.PARAM_TENSAO_LINHA,
                ProjectTableElementCategory.Transformadores when campoId == "PotenciaNominal" => Transformador.PARAM_POTENCIA_APARENTE,
                ProjectTableElementCategory.Transformadores when campoId == "TensaoPrimaria" => Transformador.PARAM_TENSAO_PRIMARIO_KV,
                ProjectTableElementCategory.Transformadores when campoId == "TensaoSecundaria" => Transformador.PARAM_TENSAO_SECUNDARIO_KV,
                ProjectTableElementCategory.Sin when campoId == "Tensao" => ElementoEquipamento.PARAM_TENSAO_LINHA,
                _ => null
            };
        }

        private static ProjectTableElementCategory? ObterCategoria(Elemento elemento)
        {
            return elemento switch
            {
                Barra => ProjectTableElementCategory.Barras,
                Cabo => ProjectTableElementCategory.Cabos,
                Carga => ProjectTableElementCategory.Cargas,
                Gerador => ProjectTableElementCategory.Geradores,
                Transformador => ProjectTableElementCategory.Transformadores,
                Sin => ProjectTableElementCategory.Sin,
                _ => null
            };
        }

        private static ProjectTableDataCell? ObterCell(
            RowBuildData row,
            ProjectTableElementCategory categoria,
            string campoId)
        {
            string chave = CriarChaveCampo(categoria, campoId);
            return row.Row.Cells.FirstOrDefault(c =>
                string.Equals(CriarChaveCampo(c.Categoria, c.CampoId), chave, StringComparison.Ordinal));
        }

        private static int CompararValores(object? a, object? b, bool decrescente)
        {
            bool vazioA = ValorVazio(a);
            bool vazioB = ValorVazio(b);

            if (vazioA && vazioB)
                return 0;

            if (vazioA)
                return 1;

            if (vazioB)
                return -1;

            int comparacao;

            if (TentarConverterNumero(a, out double numeroA) &&
                TentarConverterNumero(b, out double numeroB))
                comparacao = numeroA.CompareTo(numeroB);
            else
                comparacao = string.Compare(FormatarValor(a), FormatarValor(b), StringComparison.OrdinalIgnoreCase);

            return decrescente ? -comparacao : comparacao;
        }

        private static bool ValorVazio(object? valor)
        {
            return valor == null || valor is string texto && string.IsNullOrWhiteSpace(texto);
        }

        private static bool TentarConverterNumero(object? valor, out double numero)
        {
            numero = 0;

            if (valor == null)
                return false;

            if (valor is double d)
            {
                numero = d;
                return true;
            }

            if (valor is int i)
            {
                numero = i;
                return true;
            }

            string texto = Convert.ToString(valor, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;

            return double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out numero) ||
                double.TryParse(texto, NumberStyles.Float, CultureInfo.GetCultureInfo("pt-BR"), out numero);
        }

        private static string FormatarValor(object? valor)
        {
            return valor switch
            {
                null => string.Empty,
                double d => d.ToString(CultureInfo.InvariantCulture),
                float f => f.ToString(CultureInfo.InvariantCulture),
                decimal m => m.ToString(CultureInfo.InvariantCulture),
                _ => Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty
            };
        }

        private static string CriarChaveCampo(ProjectTableElementCategory categoria, string campoId)
        {
            return $"{categoria}|{campoId.Trim()}";
        }

        private sealed class RowBuildData
        {
            public RowBuildData(Elemento elemento, int documentIndex, ProjectTableDataRow row)
            {
                Elemento = elemento;
                DocumentIndex = documentIndex;
                Row = row;
            }

            public Elemento Elemento { get; }
            public int DocumentIndex { get; }
            public ProjectTableDataRow Row { get; }
        }

        private sealed class RowBuildDataComparer : IComparer<RowBuildData>
        {
            private readonly IReadOnlyList<ProjectTableSorting> _ordenacoes;

            public RowBuildDataComparer(IReadOnlyList<ProjectTableSorting> ordenacoes)
            {
                _ordenacoes = ordenacoes;
            }

            public int Compare(RowBuildData? x, RowBuildData? y)
            {
                if (ReferenceEquals(x, y))
                    return 0;

                if (x == null)
                    return 1;

                if (y == null)
                    return -1;

                foreach (ProjectTableSorting ordenacao in _ordenacoes)
                {
                    ProjectTableDataCell? cellX = ObterCell(x, ordenacao.Categoria, ordenacao.CampoId);
                    ProjectTableDataCell? cellY = ObterCell(y, ordenacao.Categoria, ordenacao.CampoId);
                    int comparacao = CompararValores(
                        cellX?.RawValue,
                        cellY?.RawValue,
                        ordenacao.Direcao == ProjectTableSortDirection.Decrescente);

                    if (comparacao == 0)
                        continue;

                    return comparacao;
                }

                return x.DocumentIndex.CompareTo(y.DocumentIndex);
            }
        }
    }
}
