using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class UpdateProjectTableSortingCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectTable _tabela;
        private readonly IReadOnlyList<ProjectTableSorting> _ordenacoesAnteriores;
        private readonly IReadOnlyList<ProjectTableSorting> _ordenacoesNovas;

        public UpdateProjectTableSortingCommand(
            AraciDocument document,
            ProjectTable tabela,
            IReadOnlyList<ProjectTableSorting> ordenacoesAnteriores,
            IReadOnlyList<ProjectTableSorting> ordenacoesNovas)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tabela = tabela ?? throw new ArgumentNullException(nameof(tabela));
            _ordenacoesAnteriores = CopiarOrdenacoes(ordenacoesAnteriores);
            _ordenacoesNovas = CopiarOrdenacoes(ordenacoesNovas);
        }

        public void Execute()
        {
            Aplicar(_ordenacoesNovas);
        }

        public void Undo()
        {
            Aplicar(_ordenacoesAnteriores);
        }

        public void Redo()
        {
            Execute();
        }

        private void Aplicar(IReadOnlyList<ProjectTableSorting> ordenacoes)
        {
            _tabela.Ordenacoes = CopiarOrdenacoes(ordenacoes).ToList();
            _document.AtualizarPropriedadesTabela(_tabela);
        }

        private static IReadOnlyList<ProjectTableSorting> CopiarOrdenacoes(IReadOnlyList<ProjectTableSorting> ordenacoes)
        {
            return (ordenacoes ?? Array.Empty<ProjectTableSorting>())
                .Select(ordenacao => new ProjectTableSorting
                {
                    Ordem = ordenacao.Ordem,
                    Categoria = ordenacao.Categoria,
                    CampoId = ordenacao.CampoId,
                    NomeExibicao = ordenacao.NomeExibicao,
                    Direcao = ordenacao.Direcao
                })
                .ToList();
        }
    }
}
