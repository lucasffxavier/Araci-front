using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class UpdateProjectTableFiltersCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectTable _tabela;
        private readonly ProjectTableFilterLogicalMode _modoAnterior;
        private readonly ProjectTableFilterLogicalMode _modoNovo;
        private readonly IReadOnlyList<ProjectTableFilterRule> _filtrosAnteriores;
        private readonly IReadOnlyList<ProjectTableFilterRule> _filtrosNovos;

        public UpdateProjectTableFiltersCommand(
            AraciDocument document,
            ProjectTable tabela,
            ProjectTableFilterLogicalMode modoAnterior,
            ProjectTableFilterLogicalMode modoNovo,
            IReadOnlyList<ProjectTableFilterRule> filtrosAnteriores,
            IReadOnlyList<ProjectTableFilterRule> filtrosNovos)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tabela = tabela ?? throw new ArgumentNullException(nameof(tabela));
            _modoAnterior = modoAnterior;
            _modoNovo = modoNovo;
            _filtrosAnteriores = CopiarFiltros(filtrosAnteriores);
            _filtrosNovos = CopiarFiltros(filtrosNovos);
        }

        public void Execute()
        {
            Aplicar(_modoNovo, _filtrosNovos);
        }

        public void Undo()
        {
            Aplicar(_modoAnterior, _filtrosAnteriores);
        }

        public void Redo()
        {
            Execute();
        }

        private void Aplicar(ProjectTableFilterLogicalMode modo, IReadOnlyList<ProjectTableFilterRule> filtros)
        {
            _tabela.ModoFiltro = modo;
            _tabela.Filtros = CopiarFiltros(filtros).ToList();
            _document.AtualizarPropriedadesTabela(_tabela);
        }

        private static IReadOnlyList<ProjectTableFilterRule> CopiarFiltros(IReadOnlyList<ProjectTableFilterRule> filtros)
        {
            return filtros
                .Select(f => new ProjectTableFilterRule
                {
                    Ordem = f.Ordem,
                    Categoria = f.Categoria,
                    CampoId = f.CampoId,
                    NomeExibicao = f.NomeExibicao,
                    Operador = f.Operador,
                    Valor = f.Valor
                })
                .ToList();
        }
    }
}
