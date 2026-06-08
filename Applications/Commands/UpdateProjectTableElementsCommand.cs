using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class UpdateProjectTableElementsCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectTable _tabela;
        private readonly IReadOnlyList<ProjectTableElementCategory> _categoriasAnteriores;
        private readonly IReadOnlyList<ProjectTableElementCategory> _categoriasNovas;
        private readonly IReadOnlyList<ProjectTableFieldSelection> _camposAnteriores;
        private readonly IReadOnlyList<ProjectTableFieldSelection> _camposNovos;
        private readonly IReadOnlyList<ProjectTableFilterRule> _filtrosAnteriores;
        private readonly IReadOnlyList<ProjectTableFilterRule> _filtrosNovos;

        public UpdateProjectTableElementsCommand(
            AraciDocument document,
            ProjectTable tabela,
            IReadOnlyList<ProjectTableElementCategory> categoriasAnteriores,
            IReadOnlyList<ProjectTableElementCategory> categoriasNovas,
            IReadOnlyList<ProjectTableFieldSelection> camposAnteriores,
            IReadOnlyList<ProjectTableFieldSelection> camposNovos,
            IReadOnlyList<ProjectTableFilterRule> filtrosAnteriores,
            IReadOnlyList<ProjectTableFilterRule> filtrosNovos)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tabela = tabela ?? throw new ArgumentNullException(nameof(tabela));
            _categoriasAnteriores = categoriasAnteriores.ToList();
            _categoriasNovas = categoriasNovas.ToList();
            _camposAnteriores = CopiarCampos(camposAnteriores);
            _camposNovos = CopiarCampos(camposNovos);
            _filtrosAnteriores = CopiarFiltros(filtrosAnteriores);
            _filtrosNovos = CopiarFiltros(filtrosNovos);
        }

        public void Execute()
        {
            Aplicar(_categoriasNovas, _camposNovos, _filtrosNovos);
        }

        public void Undo()
        {
            Aplicar(_categoriasAnteriores, _camposAnteriores, _filtrosAnteriores);
        }

        public void Redo()
        {
            Execute();
        }

        private void Aplicar(
            IReadOnlyList<ProjectTableElementCategory> categorias,
            IReadOnlyList<ProjectTableFieldSelection> campos,
            IReadOnlyList<ProjectTableFilterRule> filtros)
        {
            _tabela.CategoriasElementos = categorias.ToList();
            _tabela.CamposSelecionados = CopiarCampos(campos).ToList();
            _tabela.Filtros = CopiarFiltros(filtros).ToList();
            _document.AtualizarPropriedadesTabela(_tabela);
        }

        private static IReadOnlyList<ProjectTableFieldSelection> CopiarCampos(IReadOnlyList<ProjectTableFieldSelection> campos)
        {
            return campos
                .Select(c => new ProjectTableFieldSelection
                {
                    Categoria = c.Categoria,
                    CampoId = c.CampoId,
                    NomeExibicao = c.NomeExibicao,
                    Ordem = c.Ordem
                })
                .ToList();
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
