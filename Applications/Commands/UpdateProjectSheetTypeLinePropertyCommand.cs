using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public sealed class UpdateProjectSheetTypeLinePropertyCommand<T> : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly Guid _linhaId;
        private readonly Action<ProjectSheetTemplateLine, T> _aplicar;
        private readonly T _valorAnterior;
        private readonly T _valorNovo;

        public UpdateProjectSheetTypeLinePropertyCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            Guid linhaId,
            Action<ProjectSheetTemplateLine, T> aplicar,
            T valorAnterior,
            T valorNovo)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _linhaId = linhaId;
            _aplicar = aplicar ?? throw new ArgumentNullException(nameof(aplicar));
            _valorAnterior = valorAnterior;
            _valorNovo = valorNovo;
        }

        public void Execute()
        {
            Aplicar(_valorNovo);
        }

        public void Undo()
        {
            Aplicar(_valorAnterior);
        }

        public void Redo()
        {
            Execute();
        }

        private void Aplicar(T valor)
        {
            ProjectSheetTemplateLine? linha = _tipo.Linhas.FirstOrDefault(l => l.Id == _linhaId);

            if (linha == null)
                return;

            _aplicar(linha, valor);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }
    }

    public readonly struct ProjectSheetTemplateLineGraphicTypeState
    {
        public ProjectSheetTemplateLineGraphicTypeState(string nomeTipo, string familia, string categoria)
        {
            NomeTipo = nomeTipo ?? string.Empty;
            Familia = familia ?? string.Empty;
            Categoria = categoria ?? string.Empty;
        }

        public string NomeTipo { get; }
        public string Familia { get; }
        public string Categoria { get; }

        public void Aplicar(ProjectSheetTemplateLine linha)
        {
            linha.DefinirTipoLinha(NomeTipo, Familia, Categoria);
        }

        public static ProjectSheetTemplateLineGraphicTypeState FromLine(ProjectSheetTemplateLine linha)
        {
            return new ProjectSheetTemplateLineGraphicTypeState(
                linha.TipoLinhaNome,
                linha.TipoLinhaFamilia,
                linha.TipoLinhaCategoria);
        }
    }
}