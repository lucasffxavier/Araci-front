using System;
using System.Linq;
using Araci.Core.Documents;

namespace Araci.Core.Commands
{
    public sealed class UpdateProjectSheetTypeCirclePropertyCommand<T> : IUndoableCommand
    {
        private readonly AraciDocument _document;
        private readonly ProjectSheetType _tipo;
        private readonly Guid _circuloId;
        private readonly Action<ProjectSheetTemplateCircle, T> _aplicar;
        private readonly T _valorAnterior;
        private readonly T _valorNovo;

        public UpdateProjectSheetTypeCirclePropertyCommand(
            AraciDocument document,
            ProjectSheetType tipo,
            Guid circuloId,
            Action<ProjectSheetTemplateCircle, T> aplicar,
            T valorAnterior,
            T valorNovo)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            _circuloId = circuloId;
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
            ProjectSheetTemplateCircle? circulo = _tipo.Circulos.FirstOrDefault(c => c.Id == _circuloId);

            if (circulo == null)
                return;

            _aplicar(circulo, valor);
            _document.AtualizarPropriedadesTipoPrancha(_tipo);
        }
    }

    public readonly struct ProjectSheetTemplateCirclePositionState
    {
        public ProjectSheetTemplateCirclePositionState(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }

        public void Aplicar(ProjectSheetTemplateCircle circulo)
        {
            circulo.X = X;
            circulo.Y = Y;
        }

        public static ProjectSheetTemplateCirclePositionState FromCircle(ProjectSheetTemplateCircle circulo)
        {
            return new ProjectSheetTemplateCirclePositionState(circulo.X, circulo.Y);
        }
    }

    public readonly struct ProjectSheetTemplateCircleGeometryState
    {
        public ProjectSheetTemplateCircleGeometryState(double x, double y, double raio)
        {
            X = x;
            Y = y;
            Raio = raio;
        }

        public double X { get; }
        public double Y { get; }
        public double Raio { get; }

        public void Aplicar(ProjectSheetTemplateCircle circulo)
        {
            circulo.X = X;
            circulo.Y = Y;
            circulo.Raio = Raio;
        }

        public static ProjectSheetTemplateCircleGeometryState FromCircle(ProjectSheetTemplateCircle circulo)
        {
            return new ProjectSheetTemplateCircleGeometryState(
                circulo.X,
                circulo.Y,
                circulo.Raio);
        }
    }

    public readonly struct ProjectSheetTemplateCircleGraphicTypeState
    {
        public ProjectSheetTemplateCircleGraphicTypeState(string nomeTipo, string familia, string categoria)
        {
            NomeTipo = nomeTipo ?? string.Empty;
            Familia = familia ?? string.Empty;
            Categoria = categoria ?? string.Empty;
        }

        public string NomeTipo { get; }
        public string Familia { get; }
        public string Categoria { get; }

        public void Aplicar(ProjectSheetTemplateCircle circulo)
        {
            circulo.DefinirTipoLinha(NomeTipo, Familia, Categoria);
        }

        public static ProjectSheetTemplateCircleGraphicTypeState FromCircle(ProjectSheetTemplateCircle circulo)
        {
            return new ProjectSheetTemplateCircleGraphicTypeState(
                circulo.TipoLinhaNome,
                circulo.TipoLinhaFamilia,
                circulo.TipoLinhaCategoria);
        }
    }
}