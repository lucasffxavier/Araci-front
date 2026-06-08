using System;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public class UpdateProjectTableSortingCommand : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectTable _tabela;
        private readonly ProjectTableSorting? _ordenacaoAnterior;
        private readonly ProjectTableSorting? _ordenacaoNova;

        public UpdateProjectTableSortingCommand(
            AraciDocument document,
            ProjectTable tabela,
            ProjectTableSorting? ordenacaoAnterior,
            ProjectTableSorting? ordenacaoNova)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tabela = tabela ?? throw new ArgumentNullException(nameof(tabela));
            _ordenacaoAnterior = CopiarOrdenacao(ordenacaoAnterior);
            _ordenacaoNova = CopiarOrdenacao(ordenacaoNova);
        }

        public void Execute()
        {
            Aplicar(_ordenacaoNova);
        }

        public void Undo()
        {
            Aplicar(_ordenacaoAnterior);
        }

        public void Redo()
        {
            Execute();
        }

        private void Aplicar(ProjectTableSorting? ordenacao)
        {
            _tabela.Ordenacao = CopiarOrdenacao(ordenacao);
            _document.AtualizarPropriedadesTabela(_tabela);
        }

        private static ProjectTableSorting? CopiarOrdenacao(ProjectTableSorting? ordenacao)
        {
            return ordenacao == null
                ? null
                : new ProjectTableSorting
                {
                    Categoria = ordenacao.Categoria,
                    CampoId = ordenacao.CampoId,
                    NomeExibicao = ordenacao.NomeExibicao,
                    Direcao = ordenacao.Direcao
                };
        }
    }
}
