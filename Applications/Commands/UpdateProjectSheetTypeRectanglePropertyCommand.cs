using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public sealed class UpdateProjectSheetTypeRectanglePropertyCommand<T> : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly Guid _retanguloId;
        private readonly Action<ProjectSheetTemplateRectangle, T> _aplicar;
        private readonly T _valorAnterior;
        private readonly T _valorNovo;

        public UpdateProjectSheetTypeRectanglePropertyCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            Guid retanguloId,
            Action<ProjectSheetTemplateRectangle, T> aplicar,
            T valorAnterior,
            T valorNovo)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _retanguloId = retanguloId;
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
            ProjectSheetTemplateRectangle? retangulo = _tipo.Retangulos.FirstOrDefault(r => r.Id == _retanguloId);

            if (retangulo == null)
                return;

            _aplicar(retangulo, valor);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }
    }

    public readonly struct ProjectSheetTemplateRectangleGraphicTypeState
    {
        public ProjectSheetTemplateRectangleGraphicTypeState(string nomeTipo, string familia, string categoria)
        {
            NomeTipo = nomeTipo ?? string.Empty;
            Familia = familia ?? string.Empty;
            Categoria = categoria ?? string.Empty;
        }

        public string NomeTipo { get; }
        public string Familia { get; }
        public string Categoria { get; }

        public void Aplicar(ProjectSheetTemplateRectangle retangulo)
        {
            retangulo.DefinirTipoLinha(NomeTipo, Familia, Categoria);
        }

        public static ProjectSheetTemplateRectangleGraphicTypeState FromRectangle(ProjectSheetTemplateRectangle retangulo)
        {
            return new ProjectSheetTemplateRectangleGraphicTypeState(
                retangulo.TipoLinhaNome,
                retangulo.TipoLinhaFamilia,
                retangulo.TipoLinhaCategoria);
        }
    }
}